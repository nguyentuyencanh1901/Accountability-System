/// Request đăng nhập — tương đương `LoginModel`.
class LoginRequest {
  final String username;
  final String password;

  const LoginRequest({required this.username, required this.password});

  Map<String, dynamic> toJson() => {
        'username': username,
        'password': password,
      };
}

/// Request đăng ký — tương đương `RegisterModel`.
class RegisterRequest {
  final String username;
  final String password;
  final String email;
  final String? phone;
  final String fullName;
  final int status;
  final int userType;
  final List<int> roles;

  const RegisterRequest({
    required this.username,
    required this.password,
    required this.email,
    this.phone,
    required this.fullName,
    required this.status,
    required this.userType,
    this.roles = const [],
  });

  Map<String, dynamic> toJson() => {
        'username': username,
        'password': password,
        'email': email,
        'phone': phone,
        'fullName': fullName,
        'status': status,
        'userType': userType,
        'roles': roles,
      };
}

/// Request cập nhật hồ sơ — tương đương `UpdateProfileModel`.
class UpdateProfileRequest {
  final String fullName;
  final String email;
  final String? phone;

  const UpdateProfileRequest({
    required this.fullName,
    required this.email,
    this.phone,
  });

  Map<String, dynamic> toJson() => {
        'fullName': fullName,
        'email': email,
        'phone': phone,
      };
}

/// Request đổi mật khẩu — tương đương `ChangePasswordModel`.
class ChangePasswordRequest {
  final String currentPassword;
  final String newPassword;
  final String confirmPassword;

  const ChangePasswordRequest({
    required this.currentPassword,
    required this.newPassword,
    required this.confirmPassword,
  });

  Map<String, dynamic> toJson() => {
        'currentPassword': currentPassword,
        'newPassword': newPassword,
        'confirmPassword': confirmPassword,
      };
}
