import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../controllers/auth_controller.dart';
import '../../controllers/exam_session_controller.dart';
import '../../core/enums/app_enums.dart';
import '../../core/theme/app_colors.dart';
import '../../core/utils/display_helper.dart';
import '../../core/utils/vietnam_time_helper.dart';
import '../../models/exam/exam_session.dart';
import '../../l10n/app_strings.dart';
import '../../widgets/common/app_scaffold.dart';
import '../../widgets/common/message_banner.dart';
import '../../widgets/common/stat_card.dart';

/// Lịch sử thi — tương đương `Views/ExamSession/History.cshtml`.
class ExamSessionHistoryScreen extends StatefulWidget {
  const ExamSessionHistoryScreen({super.key});

  @override
  State<ExamSessionHistoryScreen> createState() =>
      _ExamSessionHistoryScreenState();
}

class _ExamSessionHistoryScreenState extends State<ExamSessionHistoryScreen> {
  int _pageIndex = 1;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  void _load() {
    final userId = context.read<AuthController>().storage.userId;
    context.read<ExamSessionController>().loadHistory(
          userId: userId,
          pageIndex: _pageIndex,
        );
  }

  Color _scoreColor(String level) => switch (level) {
        'high' => AppColors.teal,
        'mid' => AppColors.amber,
        _ => AppColors.error,
      };

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final controller = context.watch<ExamSessionController>();

    return AppScaffold(
      title: s.historyScreenTitle,
      body: Stack(
        children: [
          RefreshIndicator(
            onRefresh: () async => _load(),
            child: controller.sessions.isEmpty && !controller.isLoading
                ? ListView(
                    children: [
                      const SizedBox(height: 80),
                      Center(child: Text(s.noHistory)),
                    ],
                  )
                : ListView.builder(
                    padding: const EdgeInsets.all(12),
                    itemCount: controller.sessions.length,
                    itemBuilder: (context, index) {
                      final session = controller.sessions[index];
                      return _HistorySessionCard(
                        session: session,
                        scoreColor: _scoreColor(
                          DisplayHelper.scoreLevel(
                            session.totalScore,
                            session.maxScore,
                          ),
                        ),
                        onTap: () => context.push('/exam-sessions/${session.id}'),
                      );
                    },
                  ),
          ),
          if (controller.errorMessage != null)
            Positioned(
              top: 0,
              left: 16,
              right: 16,
              child: SafeArea(
                child: ErrorBanner(message: controller.errorMessage!),
              ),
            ),
          if (controller.isLoading) const LoadingOverlay(),
        ],
      ),
    );
  }
}

class _HistorySessionCard extends StatelessWidget {
  const _HistorySessionCard({
    required this.session,
    required this.scoreColor,
    required this.onTap,
  });

  final ExamSession session;
  final Color scoreColor;
  final VoidCallback onTap;

  String _statusLabel(int status) => switch (status) {
        ExamSessionStatus.expired => 'Hết giờ',
        ExamSessionStatus.cancelled => 'Bị hủy',
        ExamSessionStatus.completed => 'Hoàn thành',
        _ => ExamSessionStatus.label(status),
      };

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final percent = session.maxScore > 0
        ? (session.totalScore / session.maxScore).clamp(0.0, 1.0)
        : 0.0;
    final timeRange = session.finishedAt != null
        ? '${VietnamTimeHelper.formatInstant(session.startedAt)} - ${VietnamTimeHelper.formatInstant(session.finishedAt!)}'
        : VietnamTimeHelper.formatInstant(session.startedAt);

    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(8),
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      session.displayExamSetName,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.w700,
                        color: Color(0xFF111827),
                      ),
                    ),
                    const SizedBox(height: 6),
                    Wrap(
                      spacing: 6,
                      runSpacing: 4,
                      children: [
                        _Badge(
                          label: ExamType.label(session.examType),
                          bg: AppColors.blueSoft,
                          fg: AppColors.primaryLight,
                        ),
                        _Badge(
                          label: _statusLabel(session.status),
                          bg: session.status == ExamSessionStatus.cancelled
                              ? AppColors.error.withValues(alpha: 0.12)
                              : Colors.grey.shade200,
                          fg: session.status == ExamSessionStatus.cancelled
                              ? AppColors.error
                              : Colors.grey.shade700,
                        ),
                        if (session.violationCount > 0)
                          _Badge(
                            label: s.violationBadge(session.violationCount),
                            bg: AppColors.error.withValues(alpha: 0.12),
                            fg: AppColors.error,
                          ),
                      ],
                    ),
                    const SizedBox(height: 6),
                    Text(
                      timeRange,
                      style: const TextStyle(
                        fontSize: 11,
                        color: AppColors.textMuted,
                      ),
                    ),
                    const SizedBox(height: 8),
                    ClipRRect(
                      borderRadius: BorderRadius.circular(4),
                      child: LinearProgressIndicator(
                        value: percent,
                        minHeight: 5,
                        backgroundColor: Colors.grey.shade200,
                        color: scoreColor,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 10),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Text(
                    '${session.totalScore}/${session.maxScore}',
                    style: TextStyle(
                      fontSize: 15,
                      fontWeight: FontWeight.w800,
                      color: scoreColor,
                    ),
                  ),
                  const SizedBox(height: 8),
                  const Icon(
                    Icons.chevron_right,
                    size: 18,
                    color: AppColors.textMuted,
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Badge extends StatelessWidget {
  const _Badge({
    required this.label,
    required this.bg,
    required this.fg,
  });

  final String label;
  final Color bg;
  final Color fg;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(999),
      ),
      child: Text(
        label,
        style: TextStyle(
          fontSize: 10,
          fontWeight: FontWeight.w600,
          color: fg,
        ),
      ),
    );
  }
}
