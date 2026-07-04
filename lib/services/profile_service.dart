import '../core/storage/auth_storage.dart';
import '../models/auth/auth_requests.dart';
import '../models/user/app_user.dart';
import '../repositories/app_user_repository.dart';
import '../repositories/auth_repository.dart';
import 'auth_service.dart';

/// Dịch vụ hồ sơ — port từ `ProfileService.cs` (WebApp).
class ProfileService {
  ProfileService(
    this._appUserRepository,
    this._authRepository,
    this._storage,
  );

  final AppUserRepository _appUserRepository;
  final AuthRepository _authRepository;
  final AuthStorage _storage;

  /// Tải hồ sơ theo userId từ session.
  Future<ServiceResult<AppUser>> getProfile(int userId) async {
    if (userId <= 0) {
      return ServiceResult.fail('Không xác định được người dùng.');
    }

    final result = await _appUserRepository.getById(userId);
    if (!result.success || result.data == null) {
      return ServiceResult.fail(
        result.message.isNotEmpty
            ? result.message
            : 'Không thể tải thông tin cá nhân.',
      );
    }

    return ServiceResult.ok(
      result.data!.mergeWithStorage(
        username: _storage.username,
        fullName: _storage.fullName,
        email: _storage.email,
        userType: _storage.userType,
      ),
    );
  }

  /// Cập nhật hồ sơ qua API self-service (JWT xác định user).
  Future<ServiceResult<void>> updateProfile({
    required int userId,
    required String fullName,
    required String email,
    String? phone,
  }) async {
    if (userId <= 0) {
      return ServiceResult.fail('Không xác định được người dùng.');
    }

    final result = await _authRepository.updateProfile(
      UpdateProfileRequest(
        fullName: fullName.trim(),
        email: email.trim(),
        phone: phone?.trim().isEmpty == true ? null : phone?.trim(),
      ),
    );

    if (!result.success) {
      return ServiceResult.fail(
        result.message.isNotEmpty
            ? result.message
            : 'Cập nhật thông tin thất bại.',
      );
    }

    await _storage.updateProfileInfo(
      fullName: fullName.trim(),
      email: email.trim(),
    );

    return ServiceResult.ok();
  }

  /// Đổi mật khẩu.
  Future<ServiceResult<void>> changePassword({
    required int userId,
    required String currentPassword,
    required String newPassword,
    required String confirmPassword,
  }) async {
    if (userId <= 0) {
      return ServiceResult.fail('Không xác định được người dùng.');
    }

    final result = await _authRepository.changePassword(
      ChangePasswordRequest(
        currentPassword: currentPassword,
        newPassword: newPassword,
        confirmPassword: confirmPassword,
      ),
    );

    if (!result.success) {
      return ServiceResult.fail(
        result.message.isNotEmpty
            ? result.message
            : 'Đổi mật khẩu thất bại.',
      );
    }

    return ServiceResult.ok();
  }
}
