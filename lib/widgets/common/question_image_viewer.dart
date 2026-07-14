import 'package:flutter/material.dart';

/// Xem ảnh câu hỏi phóng to — tương đương modal `_QuestionImageModal` trên WebApp.
class QuestionImageViewer {
  static Future<void> show(
    BuildContext context, {
    required String imageUrl,
    String? title,
  }) {
    return showDialog<void>(
      context: context,
      barrierColor: Colors.black.withValues(alpha: 0.92),
      builder: (context) => _QuestionImageViewerDialog(
        imageUrl: imageUrl,
        title: title ?? 'Ảnh câu hỏi',
      ),
    );
  }
}

class _QuestionImageViewerDialog extends StatefulWidget {
  const _QuestionImageViewerDialog({
    required this.imageUrl,
    required this.title,
  });

  final String imageUrl;
  final String title;

  @override
  State<_QuestionImageViewerDialog> createState() =>
      _QuestionImageViewerDialogState();
}

class _QuestionImageViewerDialogState extends State<_QuestionImageViewerDialog> {
  final TransformationController _transformController =
      TransformationController();
  double _scale = 1;

  @override
  void dispose() {
    _transformController.dispose();
    super.dispose();
  }

  void _resetZoom() {
    _transformController.value = Matrix4.identity();
    setState(() => _scale = 1);
  }

  void _zoomBy(double delta) {
    final next = (_scale + delta).clamp(0.5, 5.0);
    final scaleFactor = next / _scale;
    _scale = next;

    final matrix = _transformController.value.clone()
      ..translateByDouble(0, 0, 0, 1)
      ..scaleByDouble(scaleFactor, scaleFactor, 1, 1);
    _transformController.value = matrix;
    setState(() {});
  }

  void _panBy(double dx) {
    final matrix = _transformController.value.clone()
      ..translateByDouble(dx, 0, 0, 1);
    _transformController.value = matrix;
  }

  @override
  Widget build(BuildContext context) {
    return Material(
      type: MaterialType.transparency,
      child: SafeArea(
        child: Stack(
          children: [
            Positioned.fill(
              child: InteractiveViewer(
                transformationController: _transformController,
                minScale: 0.5,
                maxScale: 5,
                panEnabled: true,
                scaleEnabled: true,
                onInteractionEnd: (_) {
                  setState(() {
                    _scale = _transformController.value.getMaxScaleOnAxis();
                  });
                },
                child: Center(
                  child: Image.network(
                    widget.imageUrl,
                    fit: BoxFit.contain,
                    loadingBuilder: (context, child, progress) {
                      if (progress == null) return child;
                      return const SizedBox(
                        width: 36,
                        height: 36,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: Colors.white70,
                        ),
                      );
                    },
                    errorBuilder: (_, __, ___) => const Padding(
                      padding: EdgeInsets.all(24),
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            Icons.broken_image_outlined,
                            color: Colors.white54,
                            size: 48,
                          ),
                          SizedBox(height: 12),
                          Text(
                            'Không thể tải ảnh',
                            style: TextStyle(color: Colors.white70),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
            ),
            Positioned(
              top: 8,
              left: 12,
              right: 12,
              child: Row(
                children: [
                  Expanded(
                    child: Text(
                      widget.title,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 15,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                  IconButton(
                    onPressed: () => Navigator.of(context).pop(),
                    icon: const Icon(Icons.close, color: Colors.white),
                    tooltip: 'Đóng',
                  ),
                ],
              ),
            ),
            Positioned(
              left: 0,
              right: 0,
              bottom: 12,
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    'Chụm để phóng to · Kéo để xem',
                    style: TextStyle(
                      color: Colors.white.withValues(alpha: 0.75),
                      fontSize: 12,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      _ViewerToolButton(
                        icon: Icons.remove,
                        tooltip: 'Thu nhỏ',
                        onPressed: () => _zoomBy(-0.25),
                      ),
                      const SizedBox(width: 6),
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 10,
                          vertical: 6,
                        ),
                        decoration: BoxDecoration(
                          color: Colors.black.withValues(alpha: 0.45),
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: Text(
                          '${(_scale * 100).round()}%',
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                      const SizedBox(width: 6),
                      _ViewerToolButton(
                        icon: Icons.add,
                        tooltip: 'Phóng to',
                        onPressed: () => _zoomBy(0.25),
                      ),
                      const SizedBox(width: 6),
                      _ViewerToolButton(
                        icon: Icons.chevron_left,
                        tooltip: 'Kéo trái',
                        onPressed: () => _panBy(80),
                      ),
                      const SizedBox(width: 6),
                      _ViewerToolButton(
                        icon: Icons.chevron_right,
                        tooltip: 'Kéo phải',
                        onPressed: () => _panBy(-80),
                      ),
                      const SizedBox(width: 6),
                      _ViewerToolButton(
                        icon: Icons.refresh,
                        tooltip: 'Đặt lại',
                        onPressed: _resetZoom,
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _ViewerToolButton extends StatelessWidget {
  const _ViewerToolButton({
    required this.icon,
    required this.tooltip,
    required this.onPressed,
  });

  final IconData icon;
  final String tooltip;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.black.withValues(alpha: 0.45),
      shape: const CircleBorder(),
      clipBehavior: Clip.antiAlias,
      child: IconButton(
        onPressed: onPressed,
        icon: Icon(icon, color: Colors.white, size: 20),
        tooltip: tooltip,
        visualDensity: VisualDensity.compact,
        constraints: const BoxConstraints(minWidth: 36, minHeight: 36),
        padding: EdgeInsets.zero,
      ),
    );
  }
}
