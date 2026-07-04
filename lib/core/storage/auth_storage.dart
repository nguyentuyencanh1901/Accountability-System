import 'package:shared_preferences/shared_preferences.dart';
import '../constants/app_constants.dart';
import '../../models/user/login_response.dart';

/// Lưu trữ phiên đăng nhập cục bộ — tương đương Session + `IApiTokenProvider` trong WebApp.
class AuthStorage {
  AuthStorage(this._prefs);

  final SharedPreferences _prefs;
  String? _token;

  String? get token => _token ?? _prefs.getString(AppConstants.keyToken);

  set token(String? value) {
    _token = value;
    if (value == null) {
      _prefs.remove(AppConstants.keyToken);
    } else {
      _prefs.setString(AppConstants.keyToken, value);
    }
  }

  bool get isLoggedIn => token != null && token!.isNotEmpty;

  int get userId => _prefs.getInt(AppConstants.keyUserId) ?? 0;

  String get username => _prefs.getString(AppConstants.keyUsername) ?? '';

  String get fullName => _prefs.getString(AppConstants.keyFullName) ?? '';

  String get email => _prefs.getString(AppConstants.keyEmail) ?? '';

  int get userType => _prefs.getInt(AppConstants.keyUserType) ?? 0;

  /// Lưu JWT và thông tin user sau đăng nhập thành công.
  Future<void> saveSession(LoginResponse loginResponse) async {
    token = loginResponse.token;
    await _prefs.setInt(AppConstants.keyUserId, loginResponse.user.id);
    await _prefs.setString(
      AppConstants.keyUsername,
      loginResponse.user.username,
    );
    await _prefs.setString(
      AppConstants.keyFullName,
      loginResponse.user.fullName,
    );
    await _prefs.setString(AppConstants.keyEmail, loginResponse.user.email);
    await _prefs.setInt(AppConstants.keyUserType, loginResponse.user.userType);
  }

  /// Cập nhật tên và email sau khi sửa hồ sơ.
  Future<void> updateProfileInfo({
    required String fullName,
    required String email,
  }) async {
    await _prefs.setString(AppConstants.keyFullName, fullName);
    await _prefs.setString(AppConstants.keyEmail, email);
  }

  /// Xóa toàn bộ phiên — tương đương Logout + Session.Clear().
  Future<void> clear() async {
    _token = null;
    await _prefs.remove(AppConstants.keyToken);
    await _prefs.remove(AppConstants.keyUserId);
    await _prefs.remove(AppConstants.keyUsername);
    await _prefs.remove(AppConstants.keyFullName);
    await _prefs.remove(AppConstants.keyEmail);
    await _prefs.remove(AppConstants.keyUserType);
  }
}
