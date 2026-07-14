import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/utils/entity_image_helper.dart';
import 'question_image_viewer.dart';

/// Ảnh câu hỏi — có ảnh thì hiển thị; không có hoặc lỗi tải thì placeholder + icon.
class QuestionImageView extends StatelessWidget {
  const QuestionImageView({
    super.key,
    required this.imageUrl,
    this.height = 120,
    this.emptyLabel = 'Không có ảnh câu hỏi',
    this.viewerTitle,
  });

  final String? imageUrl;
  final double height;
  final String emptyLabel;
  final String? viewerTitle;

  bool get _hasImage => imageUrl != null && imageUrl!.trim().isNotEmpty;

  String? get _resolvedUrl {
    final url = imageUrl!.trim();
    if (url.startsWith('http://') || url.startsWith('https://')) return url;
    return EntityImageHelper.buildPublicUrl(url);
  }

  void _openViewer(BuildContext context, String src) {
    QuestionImageViewer.show(
      context,
      imageUrl: src,
      title: viewerTitle ?? 'Ảnh câu hỏi',
    );
  }

  @override
  Widget build(BuildContext context) {
    if (!_hasImage) {
      return _QuestionImagePlaceholder(height: height, label: emptyLabel);
    }

    final src = _resolvedUrl;
    if (src == null) {
      return _QuestionImagePlaceholder(height: height, label: emptyLabel);
    }

    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: () => _openViewer(context, src),
        borderRadius: BorderRadius.circular(8),
        child: Stack(
          alignment: Alignment.center,
          children: [
            ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: Image.network(
                src,
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
            ),
            Positioned(
              right: 8,
              bottom: 8,
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(
                  color: Colors.black.withValues(alpha: 0.55),
                  borderRadius: BorderRadius.circular(16),
                ),
                child: const Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(Icons.zoom_in, size: 14, color: Colors.white),
                    SizedBox(width: 4),
                    Text(
                      'Xem ảnh',
                      style: TextStyle(color: Colors.white, fontSize: 11),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
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
