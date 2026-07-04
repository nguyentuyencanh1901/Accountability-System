import 'paged_metadata.dart';

/// Envelope phản hồi chuẩn từ API — tương đương `ResponseData<T>` trong Example.Common.
///
/// Luôn kiểm tra [success] trước khi đọc [data] (giống pattern WebApp / API Client).
class ResponseData<T> {
  final T? data;
  final PagedMetaData? metaData;
  final String message;
  final bool success;
  final int statusCode;

  const ResponseData({
    this.data,
    this.metaData,
    this.message = '',
    this.success = false,
    this.statusCode = 0,
  });

  factory ResponseData.fromJson(
    Map<String, dynamic> json,
    T? Function(dynamic)? fromJsonT,
  ) {
    final rawData = json['data'];
    T? parsedData;
    if (fromJsonT != null && rawData != null) {
      parsedData = fromJsonT(rawData);
    }

    return ResponseData(
      data: parsedData,
      metaData: PagedMetaData.fromJson(
        json['metaData'] as Map<String, dynamic>?,
      ),
      message: json['message'] as String? ?? '',
      success: json['success'] as bool? ?? false,
      statusCode: json['statusCode'] as int? ?? 0,
    );
  }

  /// Tạo response lỗi khi không đọc được JSON từ API.
  factory ResponseData.error(String message, {int statusCode = 500}) {
    return ResponseData(
      message: message,
      success: false,
      statusCode: statusCode,
    );
  }
}
