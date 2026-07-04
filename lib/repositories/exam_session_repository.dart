import '../../core/constants/app_constants.dart';
import '../../core/models/response_data.dart';
import '../../core/network/api_client.dart';
import '../../models/exam/exam_requests.dart';
import '../../models/exam/exam_session.dart';

/// Repository phiên thi — tương đương `ExamSessionRepository` + `ExamSessionClient`.
class ExamSessionRepository {
  ExamSessionRepository(this._api);

  final ApiClient _api;
  static const _controller = 'ExamSession';

  /// Danh sách phiên thi (lọc theo userId, status, examSetId...).
  Future<ResponseData<List<ExamSession>>> getList({
    required int pageIndex,
    int pageSize = AppConstants.defaultPageSize,
    int? userId,
    int? examSetId,
    int? examType,
    int? status,
  }) {
    final query = <String, dynamic>{
      'pageIndex': pageIndex,
      'pageSize': pageSize,
    };
    if (userId != null && userId > 0) query['userId'] = userId;
    if (examSetId != null && examSetId > 0) query['examSetId'] = examSetId;
    if (examType != null && examType > 0) query['examType'] = examType;
    if (status != null && status > 0) query['status'] = status;

    return _api.get<List<ExamSession>>(
      _api.buildPath(_controller, 'get-list-examSession'),
      queryParameters: query,
      fromJson: (json) => (json as List<dynamic>)
          .map((e) => ExamSession.fromJson(e as Map<String, dynamic>))
          .toList(),
    );
  }

  /// Lịch sử thi đã hoàn thành.
  Future<ResponseData<List<ExamSession>>> getHistory({
    required int pageIndex,
    required int userId,
    int pageSize = AppConstants.defaultPageSize,
  }) {
    return _api.get<List<ExamSession>>(
      _api.buildPath(_controller, 'get-history'),
      queryParameters: {
        'pageIndex': pageIndex,
        'pageSize': pageSize,
        'userId': userId,
      },
      fromJson: (json) => (json as List<dynamic>)
          .map((e) => ExamSession.fromJson(e as Map<String, dynamic>))
          .toList(),
    );
  }

  Future<ResponseData<ExamSession>> getById(int id) {
    return _api.get<ExamSession>(
      _api.buildPath(_controller, 'get-by-id'),
      queryParameters: {'id': id},
      fromJson: (json) => ExamSession.fromJson(json as Map<String, dynamic>),
    );
  }

  Future<ResponseData<ExamSession>> startExam(StartExamRequest request) {
    return _api.post<ExamSession>(
      _api.buildPath(_controller, 'start-exam'),
      data: request.toJson(),
      fromJson: (json) => ExamSession.fromJson(json as Map<String, dynamic>),
    );
  }

  Future<ResponseData<void>> saveExamProgress(SubmitExamRequest request) {
    return _api.post<void>(
      _api.buildPath(_controller, 'save-exam-progress'),
      data: request.toJson(),
      fromJson: (_) => null,
    );
  }

  Future<ResponseData<ExamSession>> submitExam(SubmitExamRequest request) {
    return _api.post<ExamSession>(
      _api.buildPath(_controller, 'submit-exam'),
      data: request.toJson(),
      fromJson: (json) => ExamSession.fromJson(json as Map<String, dynamic>),
    );
  }

  Future<ResponseData<void>> cancelExamDueToViolation(
    SubmitExamRequest request,
  ) {
    return _api.post<void>(
      _api.buildPath(_controller, 'cancel-exam-violation'),
      data: request.toJson(),
      fromJson: (_) => null,
    );
  }
}
