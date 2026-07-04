import '../core/constants/app_constants.dart';
import '../core/enums/app_enums.dart';
import '../models/exam/exam_session.dart';
import '../repositories/exam_period_assignment_repository.dart';
import '../repositories/exam_session_repository.dart';

/// ViewModel dashboard — tương đương `DashboardViewModel` trong WebApp.
class DashboardData {
  final String? fullName;
  final String? username;
  final int totalExamSets;
  final int inProgressSessions;
  final int completedSessions;
  final List<ExamSession> recentInProgress;
  final List<ExamSession> recentCompleted;
  final String? errorMessage;

  const DashboardData({
    this.fullName,
    this.username,
    this.totalExamSets = 0,
    this.inProgressSessions = 0,
    this.completedSessions = 0,
    this.recentInProgress = const [],
    this.recentCompleted = const [],
    this.errorMessage,
  });
}

/// Dịch vụ trang chủ — port từ `HomeService.cs`.
class HomeService {
  HomeService(this._assignmentRepo, this._sessionRepo);

  final ExamPeriodAssignmentRepository _assignmentRepo;
  final ExamSessionRepository _sessionRepo;

  Future<DashboardData> buildDashboard({
    required int userId,
    String? fullName,
    String? username,
  }) async {
    var data = DashboardData(fullName: fullName, username: username);
    String? errorMessage;

    final assignmentResult = await _assignmentRepo.getMyAssignments();
    final completedAssignmentIds = <int>{};

    if (assignmentResult.success) {
      final assignments = assignmentResult.data ?? [];
      for (final a in assignments) {
        if (a.status == ExamPeriodAssignmentStatus.completed) {
          completedAssignmentIds.add(a.id);
        }
      }
      data = DashboardData(
        fullName: fullName,
        username: username,
        totalExamSets: assignments.length,
        inProgressSessions: data.inProgressSessions,
        completedSessions: data.completedSessions,
        recentInProgress: data.recentInProgress,
        recentCompleted: data.recentCompleted,
      );
    } else if (assignmentResult.message.isNotEmpty) {
      errorMessage = assignmentResult.message;
    }

    final inProgressResult = await _sessionRepo.getList(
      pageIndex: 1,
      pageSize: 5,
      userId: userId,
      status: ExamSessionStatus.inProgress,
    );

    var recentInProgress = <ExamSession>[];
    if (inProgressResult.success) {
      final raw = inProgressResult.data ?? [];
      recentInProgress = raw
          .where(
            (s) =>
                s.examPeriodAssignmentId == null ||
                !completedAssignmentIds.contains(s.examPeriodAssignmentId),
          )
          .toList();
    }

    final historyResult = await _sessionRepo.getHistory(
      pageIndex: 1,
      userId: userId,
      pageSize: AppConstants.defaultPageSize,
    );

    var recentCompleted = <ExamSession>[];
    var completedCount = 0;
    if (historyResult.success) {
      recentCompleted = historyResult.data ?? [];
      completedCount = historyResult.metaData?.totalItems ??
          recentCompleted.length;
    }

    return DashboardData(
      fullName: fullName,
      username: username,
      totalExamSets: data.totalExamSets,
      inProgressSessions: recentInProgress.length,
      completedSessions: completedCount,
      recentInProgress: recentInProgress,
      recentCompleted: recentCompleted,
      errorMessage: errorMessage,
    );
  }
}
