import '../../core/models/response_data.dart';
import '../../core/network/api_client.dart';
import '../../core/storage/auth_storage.dart';
import '../../models/auth/auth_requests.dart';
import '../../models/user/login_response.dart';

/// Repository xác thực — tương đương `AuthenticateRepository` + `AuthenticateClient`.
class AuthRepository {
  AuthRepository(this._api, this._storage);

  final ApiClient _api;
  final AuthStorage _storage;

  static const _controller = 'Authenticate';

  /// Đăng nhập thí sinh qua endpoint `login-app` (UserType = TestTaker).
  Future<ResponseData<LoginResponse>> loginApp(LoginRequest request) async {
    final result = await _api.post<LoginResponse>(
      _api.buildPath(_controller, 'login-app'),
      data: request.toJson(),
      fromJson: (json) =>
          LoginResponse.fromJson(json as Map<String, dynamic>),
    );

    if (result.success &&
        result.data != null &&
        result.data!.token.isNotEmpty) {
      await _storage.saveSession(result.data!);
    }

    return result;
  }

  /// Đăng ký tài khoản mới.
  Future<ResponseData<void>> register(RegisterRequest request) {
    return _api.post<void>(
      _api.buildPath(_controller, 'register'),
      data: request.toJson(),
      fromJson: (_) => null,
    );
  }

  /// Cập nhật hồ sơ (yêu cầu JWT).
  Future<ResponseData<void>> updateProfile(UpdateProfileRequest request) {
    return _api.put<void>(
      _api.buildPath(_controller, 'update-profile'),
      data: request.toJson(),
      fromJson: (_) => null,
    );
  }

  /// Đổi mật khẩu (yêu cầu JWT).
  Future<ResponseData<void>> changePassword(ChangePasswordRequest request) {
    return _api.post<void>(
      _api.buildPath(_controller, 'change-password'),
      data: request.toJson(),
      fromJson: (_) => null,
    );
  }

  /// Đăng xuất: xóa token cục bộ (không gọi API).
  Future<void> logout() => _storage.clear();
}
