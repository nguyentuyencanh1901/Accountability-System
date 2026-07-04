/// Model người dùng — tương đương `AppUserModel` trong API.
class AppUser {
  final int id;
  final String username;
  final String email;
  final String phone;
  final String fullName;
  final int status;
  final int userType;

  const AppUser({
    required this.id,
    required this.username,
    required this.email,
    required this.phone,
    required this.fullName,
    required this.status,
    required this.userType,
  });

  factory AppUser.fromJson(Map<String, dynamic> json) {
    return AppUser(
      id: _readInt(json, 'id'),
      username: _readString(json, 'username'),
      email: _readString(json, 'email'),
      phone: _readString(json, 'phone'),
      fullName: _readString(json, 'fullName'),
      status: _readInt(json, 'status'),
      userType: _readInt(json, 'userType'),
    );
  }

  /// Dự phòng từ session đăng nhập khi API chưa trả về.
  factory AppUser.fromAuthStorage({
    required int userId,
    required String username,
    required String fullName,
    required String email,
    required int userType,
    String phone = '',
  }) {
    return AppUser(
      id: userId,
      username: username,
      email: email,
      phone: phone,
      fullName: fullName,
      status: 1,
      userType: userType,
    );
  }

  AppUser mergeWithStorage({
    required String username,
    required String fullName,
    required String email,
    required int userType,
  }) {
    return AppUser(
      id: id,
      username: _prefer(username, this.username),
      email: _prefer(email, this.email),
      phone: phone,
      fullName: _prefer(fullName, this.fullName),
      status: status,
      userType: userType != 0 ? userType : this.userType,
    );
  }

  static String _prefer(String primary, String fallback) {
    final value = primary.trim();
    return value.isNotEmpty ? value : fallback.trim();
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
    if (value is String) return int.tryParse(value) ?? 0;
    return 0;
  }
}
