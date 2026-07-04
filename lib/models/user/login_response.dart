/// Thông tin user trong response đăng nhập — tương đương `LoginUserResponse`.
class LoginUser {
  final int id;
  final String username;
  final String fullName;
  final String email;
  final int userType;

  const LoginUser({
    required this.id,
    required this.username,
    required this.fullName,
    required this.email,
    required this.userType,
  });

  factory LoginUser.fromJson(Map<String, dynamic> json) {
    return LoginUser(
      id: json['id'] as int? ?? 0,
      username: json['username'] as String? ?? '',
      fullName: json['fullName'] as String? ?? '',
      email: json['email'] as String? ?? '',
      userType: json['userType'] as int? ?? 0,
    );
  }
}

/// DTO đăng nhập thành công — tương đương `LoginResponse` trong Example.API.Client.
class LoginResponse {
  final String token;
  final LoginUser user;

  const LoginResponse({required this.token, required this.user});

  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    return LoginResponse(
      token: json['token'] as String? ?? '',
      user: LoginUser.fromJson(json['user'] as Map<String, dynamic>? ?? {}),
    );
  }
}
