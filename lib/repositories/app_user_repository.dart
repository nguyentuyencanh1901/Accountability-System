import '../../core/models/response_data.dart';
import '../../core/network/api_client.dart';
import '../../models/user/app_user.dart';

/// Repository người dùng — tương đương `AppUserRepository` + `AppUserClient`.
class AppUserRepository {
  AppUserRepository(this._api);

  final ApiClient _api;
  static const _controller = 'AppUser';

  /// Lấy thông tin user theo Id.
  Future<ResponseData<AppUser>> getById(int id) {
    return _api.get<AppUser>(
      _api.buildPath(_controller, 'get-by-id'),
      queryParameters: {'id': id},
      fromJson: (json) => AppUser.fromJson(json as Map<String, dynamic>),
    );
  }
}
