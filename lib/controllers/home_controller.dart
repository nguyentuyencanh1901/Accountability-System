import 'package:flutter/foundation.dart';
import '../services/home_service.dart';

/// Controller trang chủ — tương đương `HomeController` (WebApp).
class HomeController extends ChangeNotifier {
  HomeController(this._service);

  final HomeService _service;

  bool _isLoading = false;
  DashboardData? _dashboard;

  bool get isLoading => _isLoading;
  DashboardData? get dashboard => _dashboard;

  Future<void> loadDashboard({
    required int userId,
    String? fullName,
    String? username,
  }) async {
    _isLoading = true;
    notifyListeners();

    _dashboard = await _service.buildDashboard(
      userId: userId,
      fullName: fullName,
      username: username,
    );

    _isLoading = false;
    notifyListeners();
  }
}
