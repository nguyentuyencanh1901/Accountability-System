import 'package:flutter/foundation.dart';
import '../core/storage/auth_storage.dart';
import '../services/auth_service.dart';

/// Controller xác thực — tương đương luồng `AccountController` (WebApp).
class AuthController extends ChangeNotifier {
  AuthController(this._authService, this._storage);

  final AuthService _authService;
  final AuthStorage _storage;

  /// Expose storage cho các controller khác cần userId.
  AuthStorage get storage => _storage;

  bool _isLoading = false;
  String? _errorMessage;
  String? _successMessage;

  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;
  String? get successMessage => _successMessage;
  bool get isLoggedIn => _authService.isLoggedIn;

  String get fullName => _storage.fullName;
  String get username => _storage.username;

  void clearMessages() {
    _errorMessage = null;
    _successMessage = null;
    notifyListeners();
  }

  void setSuccessMessage(String message) {
    _successMessage = message;
    _errorMessage = null;
    notifyListeners();
  }

  /// Đăng nhập — validate đã thực hiện ở form layer.
  Future<bool> login({
    required String username,
    required String password,
  }) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    final result = await _authService.login(
      username: username,
      password: password,
    );

    _isLoading = false;
    if (!result.success) {
      _errorMessage = result.errorMessage;
      notifyListeners();
      return false;
    }

    notifyListeners();
    return true;
  }

  /// Đăng ký tài khoản thí sinh.
  Future<bool> register({
    required String username,
    required String password,
    required String email,
    required String fullName,
    String? phone,
  }) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    final result = await _authService.register(
      username: username,
      password: password,
      email: email,
      fullName: fullName,
      phone: phone,
    );

    _isLoading = false;
    if (!result.success) {
      _errorMessage = result.errorMessage;
      notifyListeners();
      return false;
    }

    notifyListeners();
    return true;
  }

  Future<void> logout() async {
    await _authService.logout();
    notifyListeners();
  }
}
