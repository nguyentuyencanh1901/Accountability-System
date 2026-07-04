import 'package:flutter/material.dart';

import '../../l10n/app_strings.dart';

/// Mã enum tương ứng `ExamSessionStatusEnum` trong Example.Common.
class ExamSessionStatus {
  static const int inProgress = 1;
  static const int completed = 2;
  static const int expired = 3;
  static const int cancelled = 4;

  static String label(int status) => S.current.examSessionStatus(status);
}

/// Mã enum tương ứng `ExamPeriodAssignmentStatusEnum`.
class ExamPeriodAssignmentStatus {
  static const int assigned = 1;
  static const int inProgress = 2;
  static const int completed = 3;
  static const int absent = 4;
  static const int cancelled = 5;

  static String label(int status) =>
      S.current.examPeriodAssignmentStatus(status);
}

/// Loại tài khoản — tương ứng `UserTypeEnum`.
class UserType {
  static const int testTaker = 2;

  static String label(int userType) => switch (userType) {
        testTaker => S.current.isEnglish ? 'Test taker' : 'Thí sinh',
        _ => S.current.isEnglish ? 'Admin' : 'Quản lý',
      };
}

/// Mã enum tương ứng `ExamTypeEnum`.
class ExamType {
  static const int trial = 1;
  static const int real = 2;

  static String label(int examType) => S.current.examType(examType);
}

/// Trạng thái khung giờ kỳ thi — tương đương `ExamPeriodAvailability`.
enum ExamPeriodAvailability { notYetStarted, open, ended }

/// Loại câu hỏi — tương đương `QuestionTypeEnum` trong Example.Common.
class QuestionType {
  static const int singleChoice = 1;
  static const int multipleChoice = 2;

  static bool isMultiple(int type) => type == multipleChoice;

  static String label(int type) => S.current.questionType(type);

  static String shortLabel(int type) => S.current.questionTypeShort(type);

  static IconData icon(int type) => switch (type) {
        multipleChoice => Icons.checklist,
        singleChoice => Icons.radio_button_checked,
        _ => Icons.radio_button_checked,
      };
}
