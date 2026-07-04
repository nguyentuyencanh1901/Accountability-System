import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../controllers/auth_controller.dart';
import '../../controllers/exam_session_controller.dart';
import '../../core/enums/app_enums.dart';
import '../../core/theme/app_colors.dart';
import '../../core/utils/display_helper.dart';
import '../../core/utils/vietnam_time_helper.dart';
import '../../l10n/app_strings.dart';
import '../../services/exam_session_service.dart';
import '../../widgets/common/app_scaffold.dart';
import '../../widgets/common/message_banner.dart';
import '../../widgets/common/stat_card.dart';

/// Danh sách bài đang làm — tương đương `Views/ExamSession/Index.cshtml`.
class ExamSessionListScreen extends StatefulWidget {
  const ExamSessionListScreen({super.key});

  @override
  State<ExamSessionListScreen> createState() => _ExamSessionListScreenState();
}

class _ExamSessionListScreenState extends State<ExamSessionListScreen> {
  int _pageIndex = 1;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  void _load() {
    final userId = context.read<AuthController>().storage.userId;
    context.read<ExamSessionController>().loadInProgress(
          userId: userId,
          pageIndex: _pageIndex,
        );
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final controller = context.watch<ExamSessionController>();

    return AppScaffold(
      title: s.inProgressTitle,
      body: Stack(
        children: [
          RefreshIndicator(
            onRefresh: () async => _load(),
            child: controller.inProgressItems.isEmpty && !controller.isLoading
                ? ListView(
                    children: [
                      const SizedBox(height: 80),
                      Center(child: Text(s.noInProgressSessions)),
                    ],
                  )
                : ListView.builder(
                    padding: const EdgeInsets.all(12),
                    itemCount: controller.inProgressItems.length,
                    itemBuilder: (context, index) {
                      return _InProgressExamCard(
                        item: controller.inProgressItems[index],
                        s: s,
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

class _InProgressExamCard extends StatelessWidget {
  const _InProgressExamCard({required this.item, required this.s});

  final InProgressExamItem item;
  final S s;

  void _continueExam(BuildContext context) {
    context.push('/exam-sessions/${item.session.id}/take');
  }

  @override
  Widget build(BuildContext context) {
    final session = item.session;
    final assignment = item.assignment;
    final examSet = item.examSet;
    final dateFormat = (DateTime value) =>
        VietnamTimeHelper.formatInstant(value);

    final title = assignment?.examPeriodName.isNotEmpty == true
        ? assignment!.examPeriodName
        : session.displayExamSetName;
    final subtitle = assignment?.examPeriodName.isNotEmpty == true
        ? session.displayExamSetName
        : (examSet?.name ?? assignment?.examSetName ?? '');

    final questionCount = examSet?.questionCount ?? session.questions.length;
    final answeredCount = session.answers
        .where((a) => a.selectedAnswerOptionIds.isNotEmpty)
        .length;

    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: InkWell(
        onTap: () => _continueExam(context),
        borderRadius: BorderRadius.circular(10),
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: const TextStyle(
                  fontSize: 14,
                  fontWeight: FontWeight.w700,
                ),
              ),
              if (subtitle.isNotEmpty) ...[
                const SizedBox(height: 4),
                Text(
                  subtitle,
                  style: const TextStyle(
                    fontSize: 12,
                    color: AppColors.textMuted,
                  ),
                ),
              ],
              if (examSet != null) ...[
                const SizedBox(height: 4),
                Text(
                  s.minutesQuestions(examSet.durationMinutes, examSet.questionCount),
                  style: const TextStyle(
                    fontSize: 11,
                    color: AppColors.textMuted,
                  ),
                ),
              ],
              const SizedBox(height: 6),
              Wrap(
                spacing: 6,
                runSpacing: 4,
                children: [
                  _Chip(
                    DisplayHelper.examTypeName(session.examType),
                    AppColors.blueSoft,
                    AppColors.primaryLight,
                  ),
                  _Chip(
                    s.examSessionStatus(ExamSessionStatus.inProgress),
                    AppColors.amberSoft,
                    AppColors.amber,
                  ),
                  if (session.violationCount > 0)
                    _Chip(
                      s.violationBadge(session.violationCount),
                      AppColors.error.withValues(alpha: 0.12),
                      AppColors.error,
                    ),
                ],
              ),
              const SizedBox(height: 6),
              Text(
                s.startedAt(dateFormat(session.startedAt)),
                style: const TextStyle(fontSize: 11),
              ),
              if (assignment?.examPeriodStartAt != null) ...[
                const SizedBox(height: 4),
                Text(
                  s.timeWindow(
                    dateFormat(assignment!.examPeriodStartAt!),
                    assignment.examPeriodEndAt != null
                        ? dateFormat(assignment.examPeriodEndAt!)
                        : '—',
                  ),
                  style: const TextStyle(
                    fontSize: 11,
                    color: AppColors.textMuted,
                  ),
                ),
              ],
              if (questionCount > 0) ...[
                const SizedBox(height: 8),
                Row(
                  children: [
                    Expanded(
                      child: ClipRRect(
                        borderRadius: BorderRadius.circular(4),
                        child: LinearProgressIndicator(
                          value: (answeredCount / questionCount).clamp(0.0, 1.0),
                          minHeight: 5,
                          backgroundColor: Colors.grey.shade200,
                          color: AppColors.primaryLight,
                        ),
                      ),
                    ),
                    const SizedBox(width: 8),
                    Text(
                      s.questionsProgress(answeredCount, questionCount),
                      style: const TextStyle(
                        fontSize: 11,
                        color: AppColors.textMuted,
                      ),
                    ),
                  ],
                ),
              ],
              const SizedBox(height: 10),
              SizedBox(
                width: double.infinity,
                child: FilledButton.icon(
                  onPressed: () => _continueExam(context),
                  icon: const Icon(Icons.play_arrow, size: 18),
                  label: Text(s.continueLabel),
                  style: FilledButton.styleFrom(
                    backgroundColor: AppColors.primaryLight,
                    padding: const EdgeInsets.symmetric(vertical: 10),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Chip extends StatelessWidget {
  const _Chip(this.label, this.bg, this.fg);
  final String label;
  final Color bg;
  final Color fg;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Text(label, style: TextStyle(fontSize: 11, color: fg)),
    );
  }
}
