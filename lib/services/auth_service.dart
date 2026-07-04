import '../core/constants/app_constants.dart' show RegisterDefaults;
import '../core/storage/auth_storage.dart';
import '../models/auth/auth_requests.dart';
import '../models/user/login_response.dart';
import '../repositories/auth_repository.dart';

/// Kết quả thao tác service — tuple pattern giống WebApp `(bool Success, string? ErrorMessage, ...)`.
class ServiceResult<T> {
  final bool success;
  final String? errorMessage;
  final T? data;

  const ServiceResult({required this.success, this.errorMessage, this.data});

  factory ServiceResult.ok([T? data]) =>
      ServiceResult(success: true, data: data);

  factory ServiceResult.fail(String message) =>
      ServiceResult(success: false, errorMessage: message);
}

/// Dịch vụ xác thực — port từ `AuthenticateService.cs` (WebApp).
class AuthService {
  AuthService(this._repository, this._storage);

  final AuthRepository _repository;
  final AuthStorage _storage;

  bool get isLoggedIn => _storage.isLoggedIn;

  /// Đăng nhập thí sinh: gọi API login-app, yêu cầu token hợp lệ.
  Future<ServiceResult<LoginResponse>> login({
    required String username,
    required String password,
  }) async {
    final result = await _repository.loginApp(
      LoginRequest(username: username.trim(), password: password),
    );

    if (!result.success ||
        result.data == null ||
        result.data!.token.isEmpty) {
      return ServiceResult.fail(
        result.message.isNotEmpty
            ? result.message
            : 'Đăng nhập thất bại. Vui lòng kiểm tra lại tài khoản.',
      );
    }

    return ServiceResult.ok(result.data);
  }

  /// Đăng ký: luôn gán UserType = Thí sinh, không gán role admin.
  Future<ServiceResult<void>> register({
    required String username,
    required String password,
    required String email,
    required String fullName,
    String? phone,
  }) async {
    final result = await _repository.register(
      RegisterRequest(
        username: username.trim(),
        password: password,
        email: email.trim(),
        fullName: fullName.trim(),
        phone: phone?.trim().isEmpty == true ? null : phone?.trim(),
        userType: RegisterDefaults.userType,
        status: RegisterDefaults.status,
        roles: const [],
      ),
    );

    if (!result.success) {
      return ServiceResult.fail(
        result.message.isNotEmpty
            ? result.message
            : 'Đăng ký thất bại. Vui lòng thử lại.',
      );
    }

    return ServiceResult.ok();
  }

  Future<void> logout() => _repository.logout();
}
