import 'package:flutter/foundation.dart';
import '../services/exam_set_service.dart';

/// Controller danh sách kỳ thi — tương đương `ExamSetController` (WebApp).
class ExamSetController extends ChangeNotifier {
  ExamSetController(this._service);

  final ExamSetService _service;

  bool _isLoading = false;
  String? _errorMessage;
  List<AssignedExamItem> _items = [];
  ExamSetDetails? _details;

  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;
  List<AssignedExamItem> get items => _items;
  ExamSetDetails? get details => _details;

  Future<void> loadIndex() async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    final result = await _service.getIndex();
    _items = result.items;
    _errorMessage = result.error;
    _isLoading = false;
    notifyListeners();
  }

  Future<bool> loadDetails({
    required int assignmentId,
    required int userId,
  }) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    final result = await _service.getDetails(
      assignmentId: assignmentId,
      userId: userId,
    );

    _isLoading = false;
    if (result.error != null) {
      _errorMessage = result.error;
      notifyListeners();
      return false;
    }

    _details = result.model;
    notifyListeners();
    return true;
  }
}
