/// Hằng số ứng dụng — tương đương `SessionKeys`, `RegisterDefaults` trong WebApp.
class AppConstants {
  static const String appName = 'Hệ thống đánh giá';
  static const String supportEmail = 'support@example.com';

  // --- SharedPreferences keys (lưu phiên đăng nhập) ---
  static const String keyToken = 'auth_token';
  static const String keyUserId = 'user_id';
  static const String keyUsername = 'username';
  static const String keyFullName = 'full_name';
  static const String keyEmail = 'email';
  static const String keyUserType = 'user_type';
  static const String keyLocale = 'app_locale';

  /// Số bản ghi mỗi trang (giống WebApp: pageSize = 10).
  static const int defaultPageSize = 10;

  /// Số lần vi phạm tối đa trước khi hủy bài (giống Take.cshtml).
  static const int maxExamViolations = 5;
}

/// Giá trị mặc định khi đăng ký từ app thí sinh — tương đương `RegisterDefaults`.
class RegisterDefaults {
  /// UserType = 2: Thí sinh (TestTaker).
  static const int userType = 2;
  static const int status = 1;
}
