/// Metadata phân trang từ API — tương đương `PagedMetaData` trong Example.Common.
class PagedMetaData {
  final int currentPage;
  final int totalPages;
  final int pageSize;
  final int totalItems;
  final bool hasPrevious;
  final bool hasNext;

  const PagedMetaData({
    this.currentPage = 0,
    this.totalPages = 0,
    this.pageSize = 0,
    this.totalItems = 0,
    this.hasPrevious = false,
    this.hasNext = false,
  });

  factory PagedMetaData.fromJson(Map<String, dynamic>? json) {
    if (json == null) return const PagedMetaData();
    return PagedMetaData(
      currentPage: json['currentPage'] as int? ?? 0,
      totalPages: json['totalPages'] as int? ?? 0,
      pageSize: json['pageSize'] as int? ?? 0,
      totalItems: json['totalItems'] as int? ?? 0,
      hasPrevious: json['hasPrevious'] as bool? ?? false,
      hasNext: json['hasNext'] as bool? ?? false,
    );
  }
}
