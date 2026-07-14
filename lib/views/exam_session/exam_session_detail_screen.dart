import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../controllers/auth_controller.dart';
import '../../controllers/exam_session_controller.dart';
import '../../core/enums/app_enums.dart';
import '../../core/theme/app_colors.dart';
import '../../core/utils/display_helper.dart';
import '../../models/exam/exam_session_question.dart';
import '../../widgets/common/app_scaffold.dart';
import '../../widgets/common/message_banner.dart';
import '../../widgets/common/question_image_view.dart';
import '../../widgets/common/stat_card.dart';

/// Chi tiết kết quả thi — tương đương `Views/ExamSession/Details.cshtml`.
class ExamSessionDetailScreen extends StatefulWidget {
  const ExamSessionDetailScreen({super.key, required this.sessionId});

  final int sessionId;

  @override
  State<ExamSessionDetailScreen> createState() =>
      _ExamSessionDetailScreenState();
}

class _ExamSessionDetailScreenState extends State<ExamSessionDetailScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  void _load() {
    final userId = context.read<AuthController>().storage.userId;
    context.read<ExamSessionController>().loadDetail(widget.sessionId, userId);
  }

  Color _scoreColor(String level) => switch (level) {
        'high' => AppColors.teal,
        'mid' => AppColors.amber,
        _ => AppColors.error,
      };

  @override
  Widget build(BuildContext context) {
    final controller = context.watch<ExamSessionController>();
    final session = controller.sessionDetail;

    return AppScaffold(
      title: 'Kết quả bài thi',
      body: Stack(
        children: [
          if (session != null)
            ListView(
              padding: const EdgeInsets.all(16),
              children: [
                if (controller.successMessage != null) ...[
                  SuccessBanner(message: controller.successMessage!),
                  const SizedBox(height: 12),
                ],
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(20),
                    child: Column(
                      children: [
                        Text(
                          session.examSetName,
                          style: const TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                        const SizedBox(height: 12),
                        Text(
                          '${session.totalScore} / ${session.maxScore}',
                          style: TextStyle(
                            fontSize: 32,
                            fontWeight: FontWeight.bold,
                            color: _scoreColor(
                              DisplayHelper.scoreLevel(
                                session.totalScore,
                                session.maxScore,
                              ),
                            ),
                          ),
                        ),
                        Text(
                          DisplayHelper.scorePercentLabel(
                            session.totalScore,
                            session.maxScore,
                          ),
                        ),
                        const SizedBox(height: 8),
                        Text(ExamSessionStatus.label(session.status)),
                        if (session.violationCount > 0)
                          Text(
                            'Vi phạm: ${session.violationCount} lần',
                            style: const TextStyle(color: AppColors.error),
                          ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 10),
                _ExamResultSummary(questions: session.questions),
                const SizedBox(height: 16),
                const Text(
                  'Chi tiết từng câu',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
                ),
                const SizedBox(height: 8),
                ...session.questions.map((q) => _QuestionReviewCard(question: q)),
              ],
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

class _QuestionReviewCard extends StatelessWidget {
  const _QuestionReviewCard({required this.question});

  final ExamSessionQuestion question;

  @override
  Widget build(BuildContext context) {
    final isMultiple = QuestionType.isMultiple(question.questionType);
    final selectedIds = question.selectedAnswerOptionIds.toSet();

    final isCorrect = question.isAnswerCorrect;
    final statusColor = isCorrect == true
        ? AppColors.success
        : isCorrect == false
            ? AppColors.error
            : AppColors.textMuted;
    final statusLabel = isCorrect == true
        ? 'Đúng'
        : isCorrect == false
            ? 'Sai'
            : 'Chưa trả lời';

    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Row(
                    children: [
                      Text(
                        'Câu ${question.sortOrder}',
                        style: const TextStyle(fontWeight: FontWeight.w700),
                      ),
                      const SizedBox(width: 8),
                      _MetaChip(
                        label: isMultiple ? 'Chọn nhiều' : 'Chọn một',
                        color: AppColors.blueSoft,
                        textColor: AppColors.primaryLight,
                      ),
                    ],
                  ),
                ),
                Text(statusLabel, style: TextStyle(color: statusColor)),
              ],
            ),
            const SizedBox(height: 6),
            Text(question.content),
            const SizedBox(height: 8),
            QuestionImageView(imageUrl: question.imageUrl),
            const SizedBox(height: 10),
            ...question.answerOptions.map((opt) {
              final selected = selectedIds.contains(opt.id);
              final correct = opt.isCorrect;
              final isMissingCorrect = correct && !selected;
              final isWrongSelected = selected && !correct;

              final leadingIcon = isMultiple
                  ? (selected
                      ? Icons.check_box_rounded
                      : Icons.check_box_outline_blank)
                  : (selected
                      ? Icons.radio_button_checked
                      : Icons.radio_button_off);

              final leadingColor = selected
                  ? (isWrongSelected ? AppColors.error : AppColors.primary)
                  : (isMissingCorrect ? AppColors.success : Colors.grey);

              return Padding(
                padding: const EdgeInsets.only(bottom: 4),
                child: Row(
                  children: [
                    Icon(
                      leadingIcon,
                      size: 16,
                      color: leadingColor,
                    ),
                    const SizedBox(width: 6),
                    Expanded(child: Text(opt.content)),
                    if (correct)
                      const Icon(
                        Icons.check,
                        size: 16,
                        color: AppColors.success,
                      )
                    else if (isWrongSelected)
                      const Icon(
                        Icons.close,
                        size: 16,
                        color: AppColors.error,
                      ),
                  ],
                ),
              );
            }),
            const SizedBox(height: 10),
            _AnswerSummaryNote(
              selectedLabels: question.answerOptions
                  .where((o) => selectedIds.contains(o.id))
                  .map((o) => o.content)
                  .toList(),
              correctLabels: question.answerOptions
                  .where((o) => o.isCorrect)
                  .map((o) => o.content)
                  .toList(),
              missingLabels: question.answerOptions
                  .where((o) => o.isCorrect && !selectedIds.contains(o.id))
                  .map((o) => o.content)
                  .toList(),
              wrongLabels: question.answerOptions
                  .where((o) => selectedIds.contains(o.id) && !o.isCorrect)
                  .map((o) => o.content)
                  .toList(),
            ),
          ],
        ),
      ),
    );
  }
}

class _ExamResultSummary extends StatelessWidget {
  const _ExamResultSummary({required this.questions});

  final List<ExamSessionQuestion> questions;

  @override
  Widget build(BuildContext context) {
    final total = questions.length;
    final unanswered = questions
        .where((q) => q.selectedAnswerOptionIds.isEmpty)
        .length;
    final correct = questions.where((q) => q.isAnswerCorrect == true).length;
    final wrong = total - unanswered - correct;

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Wrap(
          spacing: 8,
          runSpacing: 8,
          children: [
            _MetaChip(
              label: 'Tổng: $total',
              color: Colors.grey.shade200,
              textColor: Colors.grey.shade800,
            ),
            _MetaChip(
              label: 'Đúng: $correct',
              color: AppColors.tealSoft,
              textColor: AppColors.teal,
            ),
            _MetaChip(
              label: 'Sai: $wrong',
              color: AppColors.error.withValues(alpha: 0.12),
              textColor: AppColors.error,
            ),
            _MetaChip(
              label: 'Chưa chọn: $unanswered',
              color: AppColors.amberSoft,
              textColor: AppColors.amber,
            ),
          ],
        ),
      ),
    );
  }
}

class _MetaChip extends StatelessWidget {
  const _MetaChip({
    required this.label,
    required this.color,
    required this.textColor,
  });

  final String label;
  final Color color;
  final Color textColor;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: color,
        borderRadius: BorderRadius.circular(999),
      ),
      child: Text(
        label,
        style: TextStyle(
          fontSize: 11,
          fontWeight: FontWeight.w600,
          color: textColor,
        ),
      ),
    );
  }
}

class _AnswerSummaryNote extends StatelessWidget {
  const _AnswerSummaryNote({
    required this.selectedLabels,
    required this.correctLabels,
    required this.missingLabels,
    required this.wrongLabels,
  });

  final List<String> selectedLabels;
  final List<String> correctLabels;
  final List<String> missingLabels;
  final List<String> wrongLabels;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(10),
      decoration: BoxDecoration(
        color: AppColors.background,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: AppColors.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _noteLine(
            'Bạn chọn (${selectedLabels.length})',
            selectedLabels.isEmpty ? '—' : selectedLabels.join(', '),
            AppColors.primary,
          ),
          const SizedBox(height: 6),
          _noteLine(
            'Đáp án đúng (${correctLabels.length})',
            correctLabels.join(', '),
            AppColors.teal,
          ),
          if (missingLabels.isNotEmpty) ...[
            const SizedBox(height: 6),
            _noteLine(
              'Chưa chọn (${missingLabels.length})',
              missingLabels.join(', '),
              AppColors.amber,
            ),
          ],
          if (wrongLabels.isNotEmpty) ...[
            const SizedBox(height: 6),
            _noteLine(
              'Chọn sai (${wrongLabels.length})',
              wrongLabels.join(', '),
              AppColors.error,
            ),
          ],
        ],
      ),
    );
  }

  Widget _noteLine(String title, String value, Color color) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: TextStyle(
            fontSize: 11,
            fontWeight: FontWeight.w700,
            color: color,
          ),
        ),
        const SizedBox(height: 2),
        Text(
          value,
          style: const TextStyle(
            fontSize: 12,
            height: 1.35,
            color: Color(0xFF374151),
          ),
        ),
      ],
    );
  }
}
