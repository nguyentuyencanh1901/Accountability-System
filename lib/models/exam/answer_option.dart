/// Đáp án trắc nghiệm — tương đương `AnswerOptionSaveModel`.
class AnswerOption {
  final int id;
  final int questionId;
  final String content;
  final bool isCorrect;
  final int sortOrder;

  const AnswerOption({
    required this.id,
    required this.questionId,
    required this.content,
    required this.isCorrect,
    required this.sortOrder,
  });

  factory AnswerOption.fromJson(Map<String, dynamic> json) {
    return AnswerOption(
      id: json['id'] as int? ?? 0,
      questionId: json['questionId'] as int? ?? 0,
      content: json['content'] as String? ?? '',
      isCorrect: json['isCorrect'] as bool? ?? false,
      sortOrder: json['sortOrder'] as int? ?? 0,
    );
  }
}
