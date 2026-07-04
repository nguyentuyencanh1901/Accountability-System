import 'dart:convert';

import 'package:dio/dio.dart';
import '../config/api_config.dart';
import '../models/response_data.dart';
import '../storage/auth_storage.dart';

typedef SessionExpiredCallback = void Function();

/// HTTP client cơ sở — tương đương `ApiClientBase` trong Example.API.Client.
///
/// - Tự gắn Bearer token từ [AuthStorage].
/// - Deserialize `ResponseData<T>` với camelCase.
/// - Không throw exception khi API trả `success: false` (giống .NET client).
class ApiClient {
  ApiClient(this._authStorage, {SessionExpiredCallback? onSessionExpired})
      : _onSessionExpired = onSessionExpired {
    _dio = Dio(
      BaseOptions(
        baseUrl: '${ApiConfig.baseUrl}/',
        connectTimeout: ApiConfig.connectTimeout,
        receiveTimeout: ApiConfig.receiveTimeout,
        headers: {'Accept': 'application/json'},
        responseType: ResponseType.plain,
      ),
    );

    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) {
          final token = _authStorage.token;
          if (token != null && token.isNotEmpty) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          handler.next(options);
        },
      ),
    );
  }

  final AuthStorage _authStorage;
  final SessionExpiredCallback? _onSessionExpired;
  late final Dio _dio;

  /// Build path theo pattern: `api/v{version}/{controller}/{route}`.
  String buildPath(String controller, String route) {
    return 'api/v${ApiConfig.apiVersion}/$controller/$route';
  }

  Future<ResponseData<T>> get<T>(
    String path, {
    Map<String, dynamic>? queryParameters,
    required T? Function(dynamic json) fromJson,
  }) {
    return _send(
      () => _dio.get<String>(path, queryParameters: queryParameters),
      fromJson,
    );
  }

  Future<ResponseData<T>> post<T>(
    String path, {
    Object? data,
    required T? Function(dynamic json) fromJson,
  }) {
    return _send(() => _dio.post<String>(path, data: data), fromJson);
  }

  Future<ResponseData<T>> put<T>(
    String path, {
    Object? data,
    required T? Function(dynamic json) fromJson,
  }) {
    return _send(() => _dio.put<String>(path, data: data), fromJson);
  }

  Future<ResponseData<T>> _send<T>(
    Future<Response<String>> Function() request,
    T? Function(dynamic json) fromJson,
  ) async {
    try {
      final response = await request();
      final content = response.data?.trim() ?? '';

      if (content.isEmpty) {
        final ok = response.statusCode != null &&
            response.statusCode! >= 200 &&
            response.statusCode! < 300;
        return ResponseData(success: ok, statusCode: response.statusCode ?? 0);
      }

      if (!content.startsWith('{')) {
        return ResponseData.error(
          content,
          statusCode: response.statusCode ?? 500,
        );
      }

      final map = jsonDecode(content) as Map<String, dynamic>;
      return ResponseData.fromJson(map, fromJson);
    } on DioException catch (e) {
      return _handleDioException(e);
    } catch (_) {
      return ResponseData.error('Không thể đọc phản hồi từ API');
    }
  }

  Future<ResponseData<T>> _handleDioException<T>(DioException e) async {
    final statusCode = e.response?.statusCode ?? 0;

    if (statusCode == 401) {
      await _authStorage.clear();
      _onSessionExpired?.call();
      return ResponseData.error(
        'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.',
        statusCode: 401,
      );
    }

    final body = e.response?.data;
    if (body is String && body.trim().startsWith('{')) {
      try {
        final map = jsonDecode(body) as Map<String, dynamic>;
        final message = map['message'] as String?;
        if (message != null && message.isNotEmpty) {
          return ResponseData.error(message, statusCode: statusCode);
        }
      } catch (_) {}
    }

    return ResponseData.error(
      _friendlyDioMessage(e),
      statusCode: statusCode,
    );
  }

  String _friendlyDioMessage(DioException e) {
    switch (e.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
        return 'Kết nối máy chủ quá lâu. Vui lòng thử lại.';
      case DioExceptionType.connectionError:
        return 'Không thể kết nối đến máy chủ. Kiểm tra mạng và API.';
      default:
        return 'Không thể kết nối đến máy chủ.';
    }
  }
}

/// Encode object sang JSON Map (dùng cho request body).
Map<String, dynamic> toJsonMap(Object obj) {
  if (obj is Map<String, dynamic>) return obj;
  if (obj is Map) return Map<String, dynamic>.from(obj);
  throw ArgumentError('Object must implement toJson or be a Map');
}
