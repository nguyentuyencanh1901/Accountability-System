import 'answer_option.dart';

/// Câu hỏi trong phiên thi — tương đương `ExamSessionQuestionModel`.
class ExamSessionQuestion {
  final int id;
  final int examSessionId;
  final int questionId;
  final int points;
  final int sortOrder;
  final String content;
  final int questionType;
  final String? imageUrl;
  final List<AnswerOption> answerOptions;
  final bool? isAnswerCorrect;
  final double? answerScore;
  final List<int> selectedAnswerOptionIds;

  const ExamSessionQuestion({
    required this.id,
    required this.examSessionId,
    required this.questionId,
    required this.points,
    required this.sortOrder,
    required this.content,
    required this.questionType,
    this.imageUrl,
    this.answerOptions = const [],
    this.isAnswerCorrect,
    this.answerScore,
    this.selectedAnswerOptionIds = const [],
  });

  factory ExamSessionQuestion.fromJson(Map<String, dynamic> json) {
    return ExamSessionQuestion(
      id: json['id'] as int? ?? 0,
      examSessionId: json['examSessionId'] as int? ?? 0,
      questionId: json['questionId'] as int? ?? 0,
      points: json['points'] as int? ?? 0,
      sortOrder: json['sortOrder'] as int? ?? 0,
      content: json['content'] as String? ?? '',
      questionType: json['questionType'] as int? ?? 1,
      imageUrl: json['imageUrl'] as String? ?? json['ImageUrl'] as String?,
      answerOptions: (json['answerOptions'] as List<dynamic>?)
              ?.map((e) => AnswerOption.fromJson(e as Map<String, dynamic>))
              .toList() ??
          const [],
      isAnswerCorrect: json['isAnswerCorrect'] as bool?,
      answerScore: (json['answerScore'] as num?)?.toDouble(),
      selectedAnswerOptionIds:
          (json['selectedAnswerOptionIds'] as List<dynamic>?)
                  ?.map((e) => (e as num).toInt())
                  .toList() ??
              const [],
    );
  }
}
