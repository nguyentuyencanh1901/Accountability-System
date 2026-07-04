import '../../core/utils/vietnam_time_helper.dart';
import 'exam_session_question.dart';

/// Phiên thi — tương đương `ExamSessionModel`.
class ExamSession {
  final int id;
  final int examSetId;
  final int userId;
  final int? examPeriodAssignmentId;
  final int examType;
  final DateTime startedAt;
  final DateTime? finishedAt;
  final double totalScore;
  final double maxScore;
  final int status;
  final int violationCount;
  final String examSetName;
  final String userFullName;
  final List<ExamSessionQuestion> questions;
  final List<ExamSessionAnswer> answers;

  const ExamSession({
    required this.id,
    required this.examSetId,
    required this.userId,
    this.examPeriodAssignmentId,
    required this.examType,
    required this.startedAt,
    this.finishedAt,
    required this.totalScore,
    required this.maxScore,
    required this.status,
    this.violationCount = 0,
    this.examSetName = '',
    this.userFullName = '',
    this.questions = const [],
    this.answers = const [],
  });

  String get displayExamSetName =>
      examSetName.trim().isNotEmpty ? examSetName.trim() : 'Bộ đề #$examSetId';

  factory ExamSession.fromJson(Map<String, dynamic> json) {
    return ExamSession(
      id: _readInt(json, 'id'),
      examSetId: _readInt(json, 'examSetId'),
      userId: _readInt(json, 'userId'),
      examPeriodAssignmentId: _readIntOrNull(json, 'examPeriodAssignmentId'),
      examType: _readInt(json, 'examType'),
      startedAt:
          _readDate(json, 'startedAt') ?? DateTime.now().toUtc(),
      finishedAt: _readDate(json, 'finishedAt'),
      totalScore: _readDouble(json, 'totalScore'),
      maxScore: _readDouble(json, 'maxScore'),
      status: _readInt(json, 'status'),
      violationCount: _readInt(json, 'violationCount'),
      examSetName: _readString(json, 'examSetName'),
      userFullName: _readString(json, 'userFullName'),
      questions: (json['questions'] as List<dynamic>?)
              ?.map((e) => ExamSessionQuestion.fromJson(e as Map<String, dynamic>))
              .toList() ??
          const [],
      answers: (json['answers'] as List<dynamic>?)
              ?.map((e) => ExamSessionAnswer.fromJson(e as Map<String, dynamic>))
              .toList() ??
          const [],
    );
  }

  static String _readString(Map<String, dynamic> json, String key) {
    final pascal = key.isEmpty ? key : '${key[0].toUpperCase()}${key.substring(1)}';
    final value = json[key] ?? json[pascal];
    if (value == null) return '';
    return value.toString();
  }

  static int _readInt(Map<String, dynamic> json, String key) {
    final pascal = key.isEmpty ? key : '${key[0].toUpperCase()}${key.substring(1)}';
    final value = json[key] ?? json[pascal];
    if (value is int) return value;
    if (value is num) return value.toInt();
    if (value is String) return int.tryParse(value) ?? 0;
    return 0;
  }

  static int? _readIntOrNull(Map<String, dynamic> json, String key) {
    final pascal = key.isEmpty ? key : '${key[0].toUpperCase()}${key.substring(1)}';
    final value = json[key] ?? json[pascal];
    if (value == null) return null;
    if (value is int) return value;
    if (value is num) return value.toInt();
    if (value is String) return int.tryParse(value);
    return null;
  }

  static double _readDouble(Map<String, dynamic> json, String key) {
    final pascal = key.isEmpty ? key : '${key[0].toUpperCase()}${key.substring(1)}';
    final value = json[key] ?? json[pascal];
    if (value is num) return value.toDouble();
    if (value is String) return double.tryParse(value) ?? 0;
    return 0;
  }

  static DateTime? _readDate(Map<String, dynamic> json, String key) {
    final pascal = key.isEmpty ? key : '${key[0].toUpperCase()}${key.substring(1)}';
    final value = json[key] ?? json[pascal];
    if (value == null) return null;
    return VietnamTimeHelper.parseApiInstant(value.toString());
  }
}

/// Đáp án đã chọn trong phiên thi.
class ExamSessionAnswer {
  final int questionId;
  final List<int> selectedAnswerOptionIds;

  const ExamSessionAnswer({
    required this.questionId,
    this.selectedAnswerOptionIds = const [],
  });

  factory ExamSessionAnswer.fromJson(Map<String, dynamic> json) {
    return ExamSessionAnswer(
      questionId: json['questionId'] as int? ?? 0,
      selectedAnswerOptionIds: (json['selectedAnswerOptionIds'] as List<dynamic>?)
              ?.map((e) => (e as num).toInt())
              .toList() ??
          const [],
    );
  }
}
