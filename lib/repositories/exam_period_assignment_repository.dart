import '../../core/models/response_data.dart';
import '../../core/network/api_client.dart';
import '../../models/exam/exam_period_assignment.dart';

/// Repository phân công kỳ thi — tương đương `ExamPeriodAssignmentRepository`.
class ExamPeriodAssignmentRepository {
  ExamPeriodAssignmentRepository(this._api);

  final ApiClient _api;
  static const _controller = 'ExamPeriodAssignment';

  /// Danh sách phân công của thí sinh hiện tại (JWT).
  Future<ResponseData<List<ExamPeriodAssignment>>> getMyAssignments() {
    return _api.get<List<ExamPeriodAssignment>>(
      _api.buildPath(_controller, 'get-my-assignments'),
      fromJson: (json) => (json as List<dynamic>)
          .map((e) => ExamPeriodAssignment.fromJson(e as Map<String, dynamic>))
          .toList(),
    );
  }
}
