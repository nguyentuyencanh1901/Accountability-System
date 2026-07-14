import '../config/api_config.dart';

/// Ghép URL ảnh công khai — port từ `EntityImageHelper.BuildPublicUrl` (API).
class EntityImageHelper {
  static const uploadRequestPath = '/uploads';

  static String? buildPublicUrl(String? relativePath, {String? apiBaseUrl}) {
    if (relativePath == null || relativePath.trim().isEmpty) return null;

    final path = '$uploadRequestPath/${relativePath.trim().replaceFirst(RegExp(r'^/+'), '')}';
    final base = (apiBaseUrl ?? ApiConfig.baseUrl).replaceAll(RegExp(r'/+$'), '');
    return '$base$path';
  }
}
