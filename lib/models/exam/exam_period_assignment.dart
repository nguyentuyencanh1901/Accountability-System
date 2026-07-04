import '../../core/utils/vietnam_time_helper.dart';

/// Phân công kỳ thi — tương đương `ExamPeriodAssignmentModel`.
class ExamPeriodAssignment {
  final int id;
  final int examPeriodId;
  final int userId;
  final int? examSetId;
  final int examType;
  final int status;
  final int? examSessionId;
  final String examPeriodName;
  final String userFullName;
  final String examSetName;
  final int examPeriodStatus;
  final DateTime? examPeriodStartAt;
  final DateTime? examPeriodEndAt;
  final double? sessionTotalScore;
  final double? sessionMaxScore;
  final DateTime? sessionStartedAt;
  final DateTime? sessionFinishedAt;
  final int? sessionStatus;

  const ExamPeriodAssignment({
    required this.id,
    required this.examPeriodId,
    required this.userId,
    this.examSetId,
    required this.examType,
    required this.status,
    this.examSessionId,
    this.examPeriodName = '',
    this.userFullName = '',
    this.examSetName = '',
    this.examPeriodStatus = 0,
    this.examPeriodStartAt,
    this.examPeriodEndAt,
    this.sessionTotalScore,
    this.sessionMaxScore,
    this.sessionStartedAt,
    this.sessionFinishedAt,
    this.sessionStatus,
  });

  factory ExamPeriodAssignment.fromJson(Map<String, dynamic> json) {
    return ExamPeriodAssignment(
      id: json['id'] as int? ?? 0,
      examPeriodId: json['examPeriodId'] as int? ?? 0,
      userId: json['userId'] as int? ?? 0,
      examSetId: json['examSetId'] as int?,
      examType: json['examType'] as int? ?? 0,
      status: json['status'] as int? ?? 0,
      examSessionId: json['examSessionId'] as int?,
      examPeriodName: json['examPeriodName'] as String? ?? '',
      userFullName: json['userFullName'] as String? ?? '',
      examSetName: json['examSetName'] as String? ?? '',
      examPeriodStatus: json['examPeriodStatus'] as int? ?? 0,
      examPeriodStartAt: _parseDate(json['examPeriodStartAt']),
      examPeriodEndAt: _parseDate(json['examPeriodEndAt']),
      sessionTotalScore: (json['sessionTotalScore'] as num?)?.toDouble(),
      sessionMaxScore: (json['sessionMaxScore'] as num?)?.toDouble(),
      sessionStartedAt: _parseDate(json['sessionStartedAt']),
      sessionFinishedAt: _parseDate(json['sessionFinishedAt']),
      sessionStatus: json['sessionStatus'] as int?,
    );
  }

  static DateTime? _parseDate(dynamic value) {
    if (value is String) return VietnamTimeHelper.parseApiInstant(value);
    return null;
  }
}
