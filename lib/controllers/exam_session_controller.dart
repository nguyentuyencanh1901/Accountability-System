import 'dart:async';

import 'package:flutter/foundation.dart';
import '../core/constants/app_constants.dart';
import '../models/exam/exam_session.dart';
import '../services/exam_session_service.dart';

/// Controller phiên thi — tương đương `ExamSessionController` (WebApp).
class ExamSessionController extends ChangeNotifier {
  ExamSessionController(this._service);

  final ExamSessionService _service;

  bool _isLoading = false;
  String? _errorMessage;
  String? _successMessage;
  List<ExamSession> _sessions = [];
  List<InProgressExamItem> _inProgressItems = [];
  int _totalItems = 0;
  ExamSession? _sessionDetail;
  TakeExamData? _takeExamData;

  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;
  String? get successMessage => _successMessage;
  List<ExamSession> get sessions => _sessions;
  List<InProgressExamItem> get inProgressItems => _inProgressItems;
  int get totalItems => _totalItems;
  ExamSession? get sessionDetail => _sessionDetail;
  TakeExamData? get takeExamData => _takeExamData;

  void clearMessages() {
    _errorMessage = null;
    _successMessage = null;
    notifyListeners();
  }

  Future<void> loadInProgress({
    required int userId,
    required int pageIndex,
  }) async {
    _isLoading = true;
    notifyListeners();

    final result = await _service.getInProgressList(
      userId: userId,
      pageIndex: pageIndex,
    );

    _inProgressItems = result.items;
    _totalItems = result.total;
    _errorMessage = result.error;
    _isLoading = false;
    notifyListeners();
  }

  Future<void> loadHistory({
    required int userId,
    required int pageIndex,
  }) async {
    _isLoading = true;
    notifyListeners();

    final result = await _service.getHistoryList(
      userId: userId,
      pageIndex: pageIndex,
    );

    _sessions = result.items;
    _totalItems = result.total;
    _errorMessage = result.error;
    _isLoading = false;
    notifyListeners();
  }

  Future<bool> loadDetail(int id, int userId) async {
    _isLoading = true;
    notifyListeners();

    final result = await _service.getOwnedSession(id, userId);
    _isLoading = false;

    if (!result.success) {
      _errorMessage = result.errorMessage;
      notifyListeners();
      return false;
    }

    _sessionDetail = result.data;
    notifyListeners();
    return true;
  }

  Future<StartExamResult> startExam({
    required int examSetId,
    required int examType,
    required int examPeriodAssignmentId,
    required int userId,
  }) async {
    _isLoading = true;
    notifyListeners();

    final result = await _service.startExam(
      examSetId: examSetId,
      examType: examType,
      examPeriodAssignmentId: examPeriodAssignmentId,
      userId: userId,
    );

    _isLoading = false;
    if (result.success && result.resumeMessage != null) {
      _successMessage = result.resumeMessage;
    }
    notifyListeners();
    return result;
  }

  Future<bool> loadTakeExam(int id, int userId) async {
    _isLoading = true;
    notifyListeners();

    final result = await _service.getTakeExam(id, userId);
    _isLoading = false;

    if (result.redirectToDetails) {
      _successMessage = result.successMessage;
      notifyListeners();
      return false;
    }

    if (result.errorMessage != null) {
      _errorMessage = result.errorMessage;
      notifyListeners();
      return false;
    }

    _takeExamData = result.data;
    notifyListeners();
    return true;
  }
}

/// Controller màn làm bài — quản lý đáp án, timer, auto-save, vi phạm.
class TakeExamController extends ChangeNotifier {
  TakeExamController(this._service);

  final ExamSessionService _service;

  TakeExamData? _data;
  final Map<int, List<int>> _answers = {};
  int _currentQuestionIndex = 0;
  int _violationCount = 0;
  bool _isSubmitting = false;
  Timer? _autoSaveTimer;
  String? _errorMessage;

  TakeExamData? get data => _data;
  int get currentQuestionIndex => _currentQuestionIndex;
  int get violationCount => _violationCount;
  bool get isSubmitting => _isSubmitting;
  String? get errorMessage => _errorMessage;
  Map<int, List<int>> get answers => Map.unmodifiable(_answers);

  int get totalQuestions => _data?.session.questions.length ?? 0;

  void init(TakeExamData data) {
    _data = data;
    for (final q in data.session.questions) {
      _answers[q.questionId] = List<int>.from(q.selectedAnswerOptionIds);
    }
    notifyListeners();
  }

  void selectAnswer(int questionId, int optionId, {required bool isMultiple}) {
    final current = List<int>.from(_answers[questionId] ?? []);
    if (isMultiple) {
      if (current.contains(optionId)) {
        current.remove(optionId);
      } else {
        current.add(optionId);
      }
    } else {
      current
        ..clear()
        ..add(optionId);
    }
    _answers[questionId] = current;
    notifyListeners();
    _scheduleAutoSave();
  }

  void goToQuestion(int index) {
    if (index >= 0 && index < totalQuestions) {
      _currentQuestionIndex = index;
      notifyListeners();
    }
  }

  void nextQuestion() => goToQuestion(_currentQuestionIndex + 1);
  void previousQuestion() => goToQuestion(_currentQuestionIndex - 1);

  bool get allAnswered {
    if (_data == null) return false;
    for (final q in _data!.session.questions) {
      final selected = _answers[q.questionId] ?? [];
      if (selected.isEmpty) return false;
    }
    return true;
  }

  int get unansweredCount {
    if (_data == null) return 0;
    var count = 0;
    for (final q in _data!.session.questions) {
      if ((_answers[q.questionId] ?? []).isEmpty) count++;
    }
    return count;
  }

  void recordViolation() {
    _violationCount++;
    notifyListeners();
  }

  bool get isViolationLimitReached =>
      _violationCount >= AppConstants.maxExamViolations;

  void _scheduleAutoSave() {
    _autoSaveTimer?.cancel();
    _autoSaveTimer = Timer(const Duration(milliseconds: 1500), () {
      if (_data != null && !_isSubmitting) {
        _saveProgress(_data!.session.userId);
      }
    });
  }

  Future<void> _saveProgress(int userId) async {
    if (_data == null) return;
    await _service.saveProgress(
      examSessionId: _data!.session.id,
      answers: _answers,
      userId: userId,
    );
  }

  Future<bool> submit(int userId) async {
    if (_data == null) return false;
    _isSubmitting = true;
    _autoSaveTimer?.cancel();
    notifyListeners();

    final result = await _service.submitExam(
      examSessionId: _data!.session.id,
      answers: _answers,
      userId: userId,
    );

    _isSubmitting = false;
    if (!result.success) {
      _errorMessage = result.errorMessage;
      notifyListeners();
      return false;
    }
    return true;
  }

  Future<bool> cancelDueToViolation(int userId) async {
    if (_data == null) return false;
    _isSubmitting = true;
    notifyListeners();

    final result = await _service.cancelDueToViolation(
      examSessionId: _data!.session.id,
      answers: _answers,
      violationCount: _violationCount,
      userId: userId,
    );

    _isSubmitting = false;
    return result.success;
  }

  @override
  void dispose() {
    _autoSaveTimer?.cancel();
    super.dispose();
  }
}
