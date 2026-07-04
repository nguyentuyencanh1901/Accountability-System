/// Request bắt đầu thi — tương đương `StartExamModel`.
class StartExamRequest {
  final int examPeriodAssignmentId;
  final int examSetId;
  final int examType;

  const StartExamRequest({
    required this.examPeriodAssignmentId,
    required this.examSetId,
    required this.examType,
  });

  Map<String, dynamic> toJson() => {
        'examPeriodAssignmentId': examPeriodAssignmentId,
        'examSetId': examSetId,
        'examType': examType,
      };
}

/// Một câu trả lời khi nộp/lưu bài.
class SubmitAnswerRequest {
  final int questionId;
  final List<int> selectedAnswerOptionIds;

  const SubmitAnswerRequest({
    required this.questionId,
    required this.selectedAnswerOptionIds,
  });

  Map<String, dynamic> toJson() => {
        'questionId': questionId,
        'selectedAnswerOptionIds': selectedAnswerOptionIds,
      };
}

/// Request nộp/lưu bài — tương đương `SubmitExamModel`.
class SubmitExamRequest {
  final int examSessionId;
  final List<SubmitAnswerRequest> answers;
  final int violationCount;

  const SubmitExamRequest({
    required this.examSessionId,
    required this.answers,
    this.violationCount = 0,
  });

  Map<String, dynamic> toJson() => {
        'examSessionId': examSessionId,
        'answers': answers.map((a) => a.toJson()).toList(),
        'violationCount': violationCount,
      };
}
