import 'package:intl/intl.dart';

/// Chuẩn hóa múi giờ VN (+07) — port từ `VietnamTimeHelper.cs`.
class VietnamTimeHelper {
  static const Duration offset = Duration(hours: 7);

  /// Parse ISO-8601 từ API → instant UTC (dùng lưu model, tính timer).
  static DateTime? parseApiInstant(String? raw) {
    if (raw == null || raw.isEmpty) return null;
    return DateTime.tryParse(raw)?.toUtc();
  }

  /// Chuyển instant sang giờ VN wall-clock để so sánh / hiển thị (giống `ToVietnamOffset`).
  static DateTime toVietnamWallClock(DateTime value) {
    final shifted = value.toUtc().add(offset);
    return DateTime(
      shifted.year,
      shifted.month,
      shifted.day,
      shifted.hour,
      shifted.minute,
      shifted.second,
      shifted.millisecond,
      shifted.microsecond,
    );
  }

  /// Thời điểm hiện tại theo giờ VN (+07).
  static DateTime nowVietnam() => toVietnamWallClock(DateTime.now().toUtc());

  /// Định dạng instant UTC ra chuỗi giờ VN (giống `ToDisplayString`).
  static String formatInstant(
    DateTime value, {
    String pattern = 'dd/MM/yyyy HH:mm',
  }) {
    return DateFormat(pattern).format(toVietnamWallClock(value));
  }
}
