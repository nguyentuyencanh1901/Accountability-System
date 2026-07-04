/// Bộ đề thi — tương đương `ExamSetModel`.
class ExamSet {
  final int id;
  final String name;
  final String description;
  final int durationMinutes;
  final int questionCount;
  final int status;
  final int type;

  const ExamSet({
    required this.id,
    required this.name,
    required this.description,
    required this.durationMinutes,
    required this.questionCount,
    required this.status,
    required this.type,
  });

  factory ExamSet.fromJson(Map<String, dynamic> json) {
    return ExamSet(
      id: json['id'] as int? ?? 0,
      name: json['name'] as String? ?? '',
      description: json['description'] as String? ?? '',
      durationMinutes: json['durationMinutes'] as int? ?? 60,
      questionCount: json['questionCount'] as int? ?? 0,
      status: json['status'] as int? ?? 0,
      type: json['type'] as int? ?? 0,
    );
  }
}
