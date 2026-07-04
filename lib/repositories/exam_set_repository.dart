import '../../core/models/response_data.dart';
import '../../core/network/api_client.dart';
import '../../models/exam/exam_set.dart';

/// Repository bộ đề — tương đương `ExamSetRepository`.
class ExamSetRepository {
  ExamSetRepository(this._api);

  final ApiClient _api;
  static const _controller = 'ExamSet';

  Future<ResponseData<ExamSet>> getById(int id) {
    return _api.get<ExamSet>(
      _api.buildPath(_controller, 'get-by-id'),
      queryParameters: {'id': id},
      fromJson: (json) => ExamSet.fromJson(json as Map<String, dynamic>),
    );
  }
}
