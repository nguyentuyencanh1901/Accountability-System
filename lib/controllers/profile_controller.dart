import 'package:flutter/foundation.dart';

import '../l10n/app_strings.dart';
import '../models/user/app_user.dart';
import '../services/profile_service.dart';

/// Controller hồ sơ — tương đương `ProfileController` (WebApp).
class ProfileController extends ChangeNotifier {
  ProfileController(this._service);

  final ProfileService _service;

  bool _isLoading = false;
  String? _errorMessage;
  String? _successMessage;
  AppUser? _user;

  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;
  String? get successMessage => _successMessage;
  AppUser? get user => _user;

  void clearMessages() {
    _errorMessage = null;
    _successMessage = null;
    notifyListeners();
  }

  Future<void> loadProfile(int userId) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    final result = await _service.getProfile(userId);
    _isLoading = false;

    if (!result.success) {
      _errorMessage = result.errorMessage;
    } else {
      _user = result.data;
    }
    notifyListeners();
  }

  Future<bool> updateProfile({
    required int userId,
    required String fullName,
    required String email,
    String? phone,
  }) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    final result = await _service.updateProfile(
      userId: userId,
      fullName: fullName,
      email: email,
      phone: phone,
    );

    _isLoading = false;
    if (!result.success) {
      _errorMessage = result.errorMessage;
      notifyListeners();
      return false;
    }

    _successMessage = S.current.updateSuccess;
    await loadProfile(userId);
    return true;
  }

  Future<bool> changePassword({
    required int userId,
    required String currentPassword,
    required String newPassword,
    required String confirmPassword,
  }) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    final result = await _service.changePassword(
      userId: userId,
      currentPassword: currentPassword,
      newPassword: newPassword,
      confirmPassword: confirmPassword,
    );

    _isLoading = false;
    if (!result.success) {
      _errorMessage = result.errorMessage;
      notifyListeners();
      return false;
    }

    _successMessage = S.current.changePasswordSuccess;
    notifyListeners();
    return true;
  }
}
