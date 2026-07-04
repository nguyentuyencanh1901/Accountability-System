import '../../l10n/app_strings.dart';

/// Helper hiển thị nhãn — tương đương `DisplayHelper` / `ExamSetService` trong WebApp.
class DisplayHelper {
  static String examTypeName(int examType) => S.current.examType(examType);

  static String scorePercentLabel(double total, double max) {
    if (max <= 0) return '0%';
    return '${((total / max) * 100).toStringAsFixed(0)}%';
  }

  static String scoreLevel(double total, double max) {
    if (max <= 0) return 'low';
    final percent = (total / max) * 100;
    if (percent >= 80) return 'high';
    if (percent >= 50) return 'mid';
    return 'low';
  }
}
