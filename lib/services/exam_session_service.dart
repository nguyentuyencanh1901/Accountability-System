import '../core/enums/app_enums.dart';
import '../models/exam/exam_period_assignment.dart';
import '../models/exam/exam_requests.dart';
import '../models/exam/exam_session.dart';
import '../models/exam/exam_set.dart';
import '../repositories/exam_period_assignment_repository.dart';
import '../repositories/exam_session_repository.dart';
import '../repositories/exam_set_repository.dart';
import 'auth_service.dart';

/// Item bài đang làm — gộp phiên thi + phân công + bộ đề.
class InProgressExamItem {
  final ExamSession session;
  final ExamPeriodAssignment? assignment;
  final ExamSet? examSet;

  const InProgressExamItem({
    required this.session,
    this.assignment,
    this.examSet,
  });
}

/// Kết quả bắt đầu thi — tương đương `StartExamServiceResult`.
class StartExamResult {
  final bool success;
  final String? errorMessage;
  final int? sessionId;
  final String? resumeMessage;

  const StartExamResult({
    required this.success,
    this.errorMessage,
    this.sessionId,
    this.resumeMessage,
  });
}

/// Dữ liệu màn làm bài — tương đương `TakeExamViewModel`.
class TakeExamData {
  final ExamSession session;
  final int durationMinutes;
  final DateTime endTime;

  const TakeExamData({
    required this.session,
    required this.durationMinutes,
    required this.endTime,
  });
}

/// Dịch vụ phiên thi — port từ `ExamSessionService.cs`.
class ExamSessionService {
  ExamSessionService(
    this._sessionRepo,
    this._examSetRepo,
    this._assignmentRepo,
  );

  final ExamSessionRepository _sessionRepo;
  final ExamSetRepository _examSetRepo;
  final ExamPeriodAssignmentRepository _assignmentRepo;

  Future<({List<InProgressExamItem> items, int total, String? error})>
      getInProgressList({
    required int userId,
    required int pageIndex,
  }) async {
    final result = await _sessionRepo.getList(
      pageIndex: pageIndex,
      userId: userId,
      status: ExamSessionStatus.inProgress,
    );
    if (!result.success) {
      return (
        items: <InProgressExamItem>[],
        total: 0,
        error: result.message.isNotEmpty
            ? result.message
            : 'Không thể tải dữ liệu từ API.',
      );
    }

    final sessions = result.data ?? [];
    final assignmentsResult = await _assignmentRepo.getMyAssignments();
    final assignmentsById = <int, ExamPeriodAssignment>{};
    final completedAssignmentIds = <int>{};

    if (assignmentsResult.success) {
      for (final a in assignmentsResult.data ?? []) {
        assignmentsById[a.id] = a;
        if (a.status == ExamPeriodAssignmentStatus.completed) {
          completedAssignmentIds.add(a.id);
        }
      }
    }

    final filtered = sessions.where((s) {
      final assignmentId = s.examPeriodAssignmentId;
      return assignmentId == null || !completedAssignmentIds.contains(assignmentId);
    }).toList();

    final examSetCache = <int, ExamSet?>{};
    final items = <InProgressExamItem>[];

    for (final session in filtered) {
      final assignment = session.examPeriodAssignmentId != null
          ? assignmentsById[session.examPeriodAssignmentId]
          : null;
      ExamSet? examSet;
      final examSetId = assignment?.examSetId ?? session.examSetId;
      if (examSetId > 0) {
        if (examSetCache.containsKey(examSetId)) {
          examSet = examSetCache[examSetId];
        } else {
          final examSetResult = await _examSetRepo.getById(examSetId);
          examSet = examSetResult.success ? examSetResult.data : null;
          examSetCache[examSetId] = examSet;
        }
      }
      items.add(
        InProgressExamItem(
          session: session,
          assignment: assignment,
          examSet: examSet,
        ),
      );
    }

    items.sort((a, b) {
      final aStart = a.assignment?.examPeriodStartAt ?? a.session.startedAt;
      final bStart = b.assignment?.examPeriodStartAt ?? b.session.startedAt;
      return aStart.compareTo(bStart);
    });

    return (
      items: items,
      total: items.length,
      error: null,
    );
  }

  Future<({List<ExamSession> items, int total, String? error})> getHistoryList({
    required int userId,
    required int pageIndex,
  }) async {
    final result = await _sessionRepo.getHistory(
      pageIndex: pageIndex,
      userId: userId,
    );
    if (!result.success) {
      return (
        items: <ExamSession>[],
        total: 0,
        error: result.message.isNotEmpty
            ? result.message
            : 'Không thể tải dữ liệu từ API.',
      );
    }
    return (
      items: result.data ?? [],
      total: result.metaData?.totalItems ?? result.data?.length ?? 0,
      error: null,
    );
  }

  Future<ServiceResult<ExamSession>> getOwnedSession(int id, int userId) async {
    final result = await _sessionRepo.getById(id);
    if (!result.success || result.data == null) {
      return ServiceResult.fail(
        result.message.isNotEmpty
            ? result.message
            : 'Không tìm thấy phiên thi.',
      );
    }
    if (userId > 0 && result.data!.userId != userId) {
      return ServiceResult.fail('Bạn không có quyền truy cập phiên thi này.');
    }
    return ServiceResult.ok(result.data);
  }

  Future<StartExamResult> startExam({
    required int examSetId,
    required int examType,
    required int examPeriodAssignmentId,
    required int userId,
  }) async {
    int? existingSessionId;
    final existingResult = await _sessionRepo.getList(
      pageIndex: 1,
      pageSize: 1,
      userId: userId,
      examSetId: examSetId > 0 ? examSetId : null,
      examType: examType,
      status: ExamSessionStatus.inProgress,
    );
    existingSessionId = existingResult.data?.firstOrNull?.id;

    final result = await _sessionRepo.startExam(
      StartExamRequest(
        examPeriodAssignmentId: examPeriodAssignmentId,
        examSetId: examSetId,
        examType: examType,
      ),
    );

    if (!result.success || result.data == null) {
      return StartExamResult(
        success: false,
        errorMessage: result.message.isNotEmpty
            ? result.message
            : 'Không thể bắt đầu bài thi.',
      );
    }

    return StartExamResult(
      success: true,
      sessionId: result.data!.id,
      resumeMessage: existingSessionId != null &&
              existingSessionId == result.data!.id
          ? 'Tiếp tục bài thi đang làm.'
          : null,
    );
  }

  Future<({
    TakeExamData? data,
    String? successMessage,
    String? errorMessage,
    bool redirectToDetails,
  })> getTakeExam(int id, int userId) async {
    final owned = await getOwnedSession(id, userId);
    if (!owned.success || owned.data == null) {
      return (data: null, successMessage: null, errorMessage: owned.errorMessage, redirectToDetails: false);
    }

    final session = owned.data!;
    if (session.status != ExamSessionStatus.inProgress) {
      final msg = switch (session.status) {
        ExamSessionStatus.expired =>
          'Bài thi đã hết giờ và được tính điểm tự động.',
        ExamSessionStatus.cancelled =>
          'Bài thi đã bị hủy do vi phạm quy chế thi.',
        _ => null,
      };
      return (
        data: null,
        successMessage: msg,
        errorMessage: null,
        redirectToDetails: true,
      );
    }

    final examSetResult = await _examSetRepo.getById(session.examSetId);
    final duration = examSetResult.data?.durationMinutes ?? 60;
    final startedAt = session.startedAt.toUtc();
    final endTime = startedAt.add(Duration(minutes: duration));

    return (
      data: TakeExamData(
        session: session,
        durationMinutes: duration,
        endTime: endTime,
      ),
      successMessage: null,
      errorMessage: null,
      redirectToDetails: false,
    );
  }

  Future<ServiceResult<void>> saveProgress({
    required int examSessionId,
    required Map<int, List<int>> answers,
    required int userId,
  }) async {
    final owned = await getOwnedSession(examSessionId, userId);
    if (!owned.success) return ServiceResult.fail(owned.errorMessage!);
    if (owned.data!.status != ExamSessionStatus.inProgress) {
      return ServiceResult.fail('Phiên thi đã kết thúc.');
    }

    final result = await _sessionRepo.saveExamProgress(
      _buildSubmitRequest(examSessionId, answers),
    );
    if (!result.success) {
      return ServiceResult.fail(result.message);
    }
    return ServiceResult.ok();
  }

  Future<ServiceResult<void>> submitExam({
    required int examSessionId,
    required Map<int, List<int>> answers,
    required int userId,
  }) async {
    final owned = await getOwnedSession(examSessionId, userId);
    if (!owned.success) return ServiceResult.fail(owned.errorMessage!);

    if (owned.data!.status != ExamSessionStatus.inProgress) {
      return ServiceResult.ok();
    }

    final result = await _sessionRepo.submitExam(
      _buildSubmitRequest(examSessionId, answers),
    );
    if (!result.success) {
      return ServiceResult.fail(
        result.message.isNotEmpty ? result.message : 'Nộp bài thất bại.',
      );
    }
    return ServiceResult.ok();
  }

  Future<ServiceResult<void>> cancelDueToViolation({
    required int examSessionId,
    required Map<int, List<int>> answers,
    required int violationCount,
    required int userId,
  }) async {
    final owned = await getOwnedSession(examSessionId, userId);
    if (!owned.success) return ServiceResult.fail(owned.errorMessage!);

    if (owned.data!.status != ExamSessionStatus.inProgress) {
      return ServiceResult.ok();
    }

    final request = SubmitExamRequest(
      examSessionId: examSessionId,
      violationCount: violationCount,
      answers: _buildAnswers(answers),
    );

    final result = await _sessionRepo.cancelExamDueToViolation(request);
    if (!result.success) {
      return ServiceResult.fail(
        result.message.isNotEmpty ? result.message : 'Không thể hủy bài thi.',
      );
    }
    return ServiceResult.ok();
  }

  SubmitExamRequest _buildSubmitRequest(
    int examSessionId,
    Map<int, List<int>> answers,
  ) {
    return SubmitExamRequest(
      examSessionId: examSessionId,
      answers: _buildAnswers(answers),
    );
  }

  List<SubmitAnswerRequest> _buildAnswers(Map<int, List<int>> answers) {
    return answers.entries
        .map(
          (e) => SubmitAnswerRequest(
            questionId: e.key,
            selectedAnswerOptionIds: e.value.where((id) => id > 0).toSet().toList(),
          ),
        )
        .where((a) => a.selectedAnswerOptionIds.isNotEmpty)
        .toList();
  }
}

extension _FirstOrNull<E> on Iterable<E> {
  E? get firstOrNull {
    final iterator = this.iterator;
    if (iterator.moveNext()) return iterator.current;
    return null;
  }
}
