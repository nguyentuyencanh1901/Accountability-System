import '../core/enums/app_enums.dart';
import '../core/utils/exam_period_time_helper.dart';
import '../models/exam/exam_period_assignment.dart';
import '../models/exam/exam_session.dart';
import '../models/exam/exam_set.dart';
import '../repositories/exam_period_assignment_repository.dart';
import '../repositories/exam_session_repository.dart';
import '../repositories/exam_set_repository.dart';

/// Item trong danh sách kỳ thi — tương đương `AssignedExamItemViewModel`.
class AssignedExamItem {
  final ExamPeriodAssignment assignment;
  final ExamSet? examSet;

  const AssignedExamItem({required this.assignment, this.examSet});
}

/// Chi tiết kỳ thi — tương đương `ExamSetDetailsViewModel`.
class ExamSetDetails {
  final ExamPeriodAssignment assignment;
  final ExamSet? examSet;
  final List<ExamSession> inProgressSessions;
  final bool canStartExam;
  final bool canContinueExam;
  final String periodStatusName;
  final String displayStatusName;

  const ExamSetDetails({
    required this.assignment,
    this.examSet,
    this.inProgressSessions = const [],
    this.canStartExam = false,
    this.canContinueExam = false,
    this.periodStatusName = '',
    this.displayStatusName = '',
  });
}

/// Dịch vụ kỳ thi — port từ `ExamSetService.cs`.
class ExamSetService {
  ExamSetService(
    this._examSetRepo,
    this._sessionRepo,
    this._assignmentRepo,
  );

  final ExamSetRepository _examSetRepo;
  final ExamSessionRepository _sessionRepo;
  final ExamPeriodAssignmentRepository _assignmentRepo;

  Future<({List<AssignedExamItem> items, String? error})> getIndex() async {
    final result = await _assignmentRepo.getMyAssignments();
    if (!result.success) {
      return (items: <AssignedExamItem>[], error: result.message);
    }

    final items = <AssignedExamItem>[];
    for (final assignment in result.data ?? []) {
      ExamSet? examSet;
      if (assignment.examSetId != null && assignment.examSetId! > 0) {
        final examSetResult =
            await _examSetRepo.getById(assignment.examSetId!);
        if (examSetResult.success &&
            examSetResult.data != null &&
            examSetResult.data!.status == 1) {
          examSet = examSetResult.data;
        }
      }
      items.add(AssignedExamItem(assignment: assignment, examSet: examSet));
    }

    _sortIndexItems(items);

    return (items: items, error: null);
  }

  static bool isAssignmentCompleted(ExamPeriodAssignment assignment) =>
      assignment.status == ExamPeriodAssignmentStatus.completed;

  static void _sortIndexItems(List<AssignedExamItem> items) {
    items.sort((a, b) {
      final aDone = isAssignmentCompleted(a.assignment);
      final bDone = isAssignmentCompleted(b.assignment);
      if (aDone != bDone) return aDone ? 1 : -1;

      if (!aDone) {
        final aStart = a.assignment.examPeriodStartAt;
        final bStart = b.assignment.examPeriodStartAt;
        if (aStart == null && bStart == null) return 0;
        if (aStart == null) return 1;
        if (bStart == null) return -1;
        return aStart.compareTo(bStart);
      }

      final aTime = a.assignment.sessionFinishedAt ??
          a.assignment.examPeriodEndAt ??
          a.assignment.examPeriodStartAt;
      final bTime = b.assignment.sessionFinishedAt ??
          b.assignment.examPeriodEndAt ??
          b.assignment.examPeriodStartAt;
      if (aTime == null && bTime == null) return 0;
      if (aTime == null) return 1;
      if (bTime == null) return -1;
      return bTime.compareTo(aTime);
    });
  }

  Future<({ExamSetDetails? model, String? error})> getDetails({
    required int assignmentId,
    required int userId,
  }) async {
    final assignmentsResult = await _assignmentRepo.getMyAssignments();
    if (!assignmentsResult.success) {
      return (
        model: null,
        error: assignmentsResult.message.isNotEmpty
            ? assignmentsResult.message
            : 'Không thể tải phân công.',
      );
    }

    final assignment = (assignmentsResult.data ?? [])
        .where((x) => x.id == assignmentId)
        .firstOrNull;

    if (assignment == null) {
      return (model: null, error: 'Bạn chưa được phân công kỳ thi này.');
    }

    ExamSet? examSet;
    if (assignment.examSetId != null && assignment.examSetId! > 0) {
      final examSetResult = await _examSetRepo.getById(assignment.examSetId!);
      if (examSetResult.success) examSet = examSetResult.data;
    }

    var inProgress = <ExamSession>[];
    if (assignment.status != ExamPeriodAssignmentStatus.completed &&
        assignment.status != ExamPeriodAssignmentStatus.cancelled &&
        ExamPeriodTimeHelper.canContinueExam(assignment)) {
      final inProgressResult = await _sessionRepo.getList(
        pageIndex: 1,
        pageSize: 10,
        userId: userId,
        status: ExamSessionStatus.inProgress,
      );
      inProgress = (inProgressResult.data ?? [])
          .where(
            (s) =>
                s.examType == assignment.examType &&
                (assignment.examSetId == null ||
                    assignment.examSetId! <= 0 ||
                    s.examSetId == assignment.examSetId),
          )
          .toList();
    }

    return (
      model: ExamSetDetails(
        assignment: assignment,
        examSet: examSet,
        inProgressSessions: inProgress,
        canStartExam: ExamPeriodTimeHelper.canStartExam(assignment),
        canContinueExam: ExamPeriodTimeHelper.canContinueExam(assignment),
        periodStatusName:
            ExamPeriodTimeHelper.examPeriodStatusName(assignment),
        displayStatusName: ExamPeriodTimeHelper.displayStatusName(assignment),
      ),
      error: null,
    );
  }
}

extension _FirstOrNull<E> on Iterable<E> {
  E? get firstOrNull {
    final iterator = this.iterator;
    if (iterator.moveNext()) return iterator.current;
    return null;
  }
}
