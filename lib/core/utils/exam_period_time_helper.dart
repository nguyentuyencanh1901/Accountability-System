import '../enums/app_enums.dart';
import '../../models/exam/exam_period_assignment.dart';
import '../../l10n/app_strings.dart';
import 'vietnam_time_helper.dart';

/// Helper kiểm tra khung giờ kỳ thi — port từ `ExamPeriodTimeHelper.cs` (WebApp).
class ExamPeriodTimeHelper {
  static ExamPeriodAvailability getAvailability(ExamPeriodAssignment assignment) {
    if (assignment.examPeriodStatus == 3) {
      return ExamPeriodAvailability.ended;
    }

    final now = VietnamTimeHelper.nowVietnam();

    if (assignment.examPeriodEndAt != null) {
      final end = VietnamTimeHelper.toVietnamWallClock(assignment.examPeriodEndAt!);
      if (now.isAfter(end)) return ExamPeriodAvailability.ended;
    }

    if (assignment.examPeriodStartAt != null) {
      final start =
          VietnamTimeHelper.toVietnamWallClock(assignment.examPeriodStartAt!);
      if (now.isBefore(start)) return ExamPeriodAvailability.notYetStarted;
    }

    return ExamPeriodAvailability.open;
  }

  static bool canStartExam(ExamPeriodAssignment assignment) {
    if (assignment.status == 3 || assignment.status == 5) return false;
    return getAvailability(assignment) == ExamPeriodAvailability.open;
  }

  static bool canContinueExam(ExamPeriodAssignment assignment) {
    if (assignment.status == 3 || assignment.status == 5) return false;
    return getAvailability(assignment) != ExamPeriodAvailability.ended;
  }

  static String examPeriodStatusName(ExamPeriodAssignment assignment) {
    final s = S.current;
    return switch (getAvailability(assignment)) {
      ExamPeriodAvailability.notYetStarted => s.examPeriodPublished,
      ExamPeriodAvailability.open => s.examPeriodPublished,
      ExamPeriodAvailability.ended => s.examPeriodClosed,
    };
  }

  static String displayStatusName(ExamPeriodAssignment assignment) {
    return S.current.displayStatusName(
      assignment.status,
      getAvailability(assignment),
    );
  }
}
