import 'package:flutter/material.dart';

import '../../core/config/api_config.dart';
import '../../core/theme/app_colors.dart';

/// Ảnh câu hỏi — có ảnh thì hiển thị; không có hoặc lỗi tải thì placeholder + icon.
class QuestionImageView extends StatelessWidget {
  const QuestionImageView({
    super.key,
    required this.imageUrl,
    this.height = 120,
    this.emptyLabel = 'Không có ảnh câu hỏi',
  });

  final String? imageUrl;
  final double height;
  final String emptyLabel;

  bool get _hasImage => imageUrl != null && imageUrl!.trim().isNotEmpty;

  String get _resolvedUrl {
    final url = imageUrl!.trim();
    if (url.startsWith('http://') || url.startsWith('https://')) return url;
    return '${ApiConfig.baseUrl}$url';
  }

  @override
  Widget build(BuildContext context) {
    if (!_hasImage) {
      return _QuestionImagePlaceholder(height: height, label: emptyLabel);
    }

    return ClipRRect(
      borderRadius: BorderRadius.circular(8),
      child: Image.network(
        _resolvedUrl,
        height: height,
        width: double.infinity,
        fit: BoxFit.contain,
        loadingBuilder: (context, child, loadingProgress) {
          if (loadingProgress == null) return child;
          return SizedBox(
            height: height,
            child: const Center(
              child: SizedBox(
                width: 24,
                height: 24,
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
            ),
          );
        },
        errorBuilder: (_, __, ___) =>
            _QuestionImagePlaceholder(height: height, label: emptyLabel),
      ),
    );
  }
}

class _QuestionImagePlaceholder extends StatelessWidget {
  const _QuestionImagePlaceholder({
    required this.height,
    required this.label,
  });

  final double height;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      height: height,
      width: double.infinity,
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: AppColors.background,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: AppColors.border),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.hide_image_outlined,
            size: 22,
            color: Colors.grey.shade500,
          ),
          const SizedBox(width: 8),
          Flexible(
            child: Text(
              label,
              textAlign: TextAlign.center,
              style: TextStyle(
                fontSize: 12,
                color: Colors.grey.shade600,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
