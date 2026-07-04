import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../controllers/exam_set_controller.dart';
import '../../core/enums/app_enums.dart';
import '../../core/theme/app_colors.dart';
import '../../core/utils/display_helper.dart';
import '../../core/utils/exam_period_time_helper.dart';
import '../../core/utils/vietnam_time_helper.dart';
import '../../l10n/app_strings.dart';
import '../../models/exam/exam_period_assignment.dart';
import '../../services/exam_set_service.dart';
import '../../widgets/common/app_scaffold.dart';
import '../../widgets/common/message_banner.dart';
import '../../widgets/common/stat_card.dart';

/// Danh sách kỳ thi — tương đương `Views/ExamSet/Index.cshtml`.
class ExamSetListScreen extends StatefulWidget {
  const ExamSetListScreen({super.key});

  @override
  State<ExamSetListScreen> createState() => _ExamSetListScreenState();
}

class _ExamSetListScreenState extends State<ExamSetListScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<ExamSetController>().loadIndex();
    });
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final controller = context.watch<ExamSetController>();

    return AppScaffold(
      title: s.examsTitle,
      body: Stack(
        children: [
          RefreshIndicator(
            onRefresh: () => controller.loadIndex(),
            child: controller.items.isEmpty && !controller.isLoading
                ? ListView(
                    children: [
                      const SizedBox(height: 80),
                      Center(child: Text(s.noAssignedExams)),
                    ],
                  )
                : ListView.builder(
                    padding: const EdgeInsets.all(12),
                    itemCount: controller.items.length,
                    itemBuilder: (context, index) {
                      final item = controller.items[index];
                      return _AssignedExamCard(item: item, s: s);
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

class _AssignedExamCard extends StatelessWidget {
  const _AssignedExamCard({required this.item, required this.s});

  final AssignedExamItem item;
  final S s;

  bool get _isCompleted =>
      ExamSetService.isAssignmentCompleted(item.assignment);

  void _openDetail(BuildContext context) {
    context.push('/exam-sets/${item.assignment.id}');
  }

  @override
  Widget build(BuildContext context) {
    final a = item.assignment;
    final examSet = item.examSet;
    final dateFormat = (DateTime value) =>
        VietnamTimeHelper.formatInstant(value);

    final content = Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          a.examPeriodName,
          style: const TextStyle(
            fontSize: 14,
            fontWeight: FontWeight.w700,
          ),
        ),
        const SizedBox(height: 4),
        Text(
          examSet?.name ?? a.examSetName,
          style: const TextStyle(
            fontSize: 12,
            color: AppColors.textMuted,
          ),
        ),
        if (examSet != null) ...[
          const SizedBox(height: 4),
          Text(
            s.minutesQuestions(examSet.durationMinutes, examSet.questionCount),
            style: const TextStyle(fontSize: 11, color: AppColors.textMuted),
          ),
        ],
        const SizedBox(height: 6),
        Wrap(
          spacing: 6,
          runSpacing: 4,
          children: [
            _Chip(
              DisplayHelper.examTypeName(a.examType),
              AppColors.blueSoft,
              AppColors.primaryLight,
            ),
            _Chip(
              ExamPeriodTimeHelper.displayStatusName(a),
              AppColors.amberSoft,
              AppColors.amber,
            ),
          ],
        ),
        if (a.examPeriodStartAt != null) ...[
          const SizedBox(height: 6),
          Text(
            '${dateFormat(a.examPeriodStartAt!)} - ${a.examPeriodEndAt != null ? dateFormat(a.examPeriodEndAt!) : '—'}',
            style: const TextStyle(fontSize: 11),
          ),
        ],
        if (!_isCompleted) ...[
          const SizedBox(height: 10),
          SizedBox(
            width: double.infinity,
            child: OutlinedButton(
              onPressed: () => _openDetail(context),
              style: OutlinedButton.styleFrom(
                foregroundColor: AppColors.primaryLight,
                side: const BorderSide(color: AppColors.primaryLight),
                padding: const EdgeInsets.symmetric(vertical: 8),
              ),
              child: Text(_actionLabel(a)),
            ),
          ),
        ],
      ],
    );

    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: _isCompleted
          ? Padding(padding: const EdgeInsets.all(12), child: content)
          : InkWell(
              onTap: () => _openDetail(context),
              borderRadius: BorderRadius.circular(10),
              child: Padding(padding: const EdgeInsets.all(12), child: content),
            ),
    );
  }

  String _actionLabel(ExamPeriodAssignment a) {
    if (a.status == ExamPeriodAssignmentStatus.inProgress &&
        ExamPeriodTimeHelper.canContinueExam(a)) {
      return s.continueExam;
    }
    if (ExamPeriodTimeHelper.canStartExam(a)) {
      return s.startExam;
    }
    return s.viewDetails;
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
