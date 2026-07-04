import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../controllers/auth_controller.dart';
import '../../controllers/exam_session_controller.dart';
import '../../controllers/exam_set_controller.dart';
import '../../core/utils/display_helper.dart';
import '../../l10n/app_strings.dart';
import '../../widgets/common/app_scaffold.dart';
import '../../widgets/common/message_banner.dart';
import '../../widgets/common/stat_card.dart';

/// Chi tiết kỳ thi — tương đương `Views/ExamSet/Details.cshtml`.
class ExamSetDetailScreen extends StatefulWidget {
  const ExamSetDetailScreen({super.key, required this.assignmentId});

  final int assignmentId;

  @override
  State<ExamSetDetailScreen> createState() => _ExamSetDetailScreenState();
}

class _ExamSetDetailScreenState extends State<ExamSetDetailScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  void _load() {
    final userId = context.read<AuthController>().storage.userId;
    context.read<ExamSetController>().loadDetails(
          assignmentId: widget.assignmentId,
          userId: userId,
        );
  }

  Future<void> _startOrContinue() async {
    final details = context.read<ExamSetController>().details;
    if (details == null) return;

    final auth = context.read<AuthController>();
    final sessionCtrl = context.read<ExamSessionController>();
    final a = details.assignment;

    if (details.inProgressSessions.isNotEmpty) {
      final sessionId = details.inProgressSessions.first.id;
      if (mounted) context.push('/exam-sessions/$sessionId/take');
      return;
    }

    final result = await sessionCtrl.startExam(
      examSetId: a.examSetId ?? 0,
      examType: a.examType,
      examPeriodAssignmentId: a.id,
      userId: auth.storage.userId,
    );

    if (!mounted) return;

    if (!result.success) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            result.errorMessage ?? S.of(context).cannotStartExamError,
          ),
        ),
      );
      return;
    }

    if (result.resumeMessage != null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(result.resumeMessage!)),
      );
    }

    context.push('/exam-sessions/${result.sessionId}/take');
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final controller = context.watch<ExamSetController>();
    final details = controller.details;

    return AppScaffold(
      title: s.examDetails,
      body: Stack(
        children: [
          if (details != null)
            ListView(
              padding: const EdgeInsets.all(12),
              children: [
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(14),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          details.assignment.examPeriodName,
                          style: const TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          details.examSet?.name ??
                              details.assignment.examSetName,
                          style: const TextStyle(fontSize: 13),
                        ),
                        const SizedBox(height: 8),
                        Text(
                          s.examTypeLine(
                            DisplayHelper.examTypeName(
                              details.assignment.examType,
                            ),
                          ),
                          style: const TextStyle(fontSize: 13),
                        ),
                        Text(
                          s.statusLine(details.displayStatusName),
                          style: const TextStyle(fontSize: 13),
                        ),
                        if (details.examSet != null)
                          Text(
                            s.durationLine(
                              details.examSet!.durationMinutes,
                              details.examSet!.questionCount,
                            ),
                            style: const TextStyle(fontSize: 13),
                          ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 12),
                if (details.canContinueExam &&
                    details.inProgressSessions.isNotEmpty)
                  ElevatedButton.icon(
                    onPressed: _startOrContinue,
                    icon: const Icon(Icons.play_arrow, size: 18),
                    label: Text(s.continueExamSession),
                  )
                else if (details.canStartExam)
                  ElevatedButton.icon(
                    onPressed: _startOrContinue,
                    icon: const Icon(Icons.play_circle_outline, size: 18),
                    label: Text(s.startExam),
                  )
                else
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(12),
                      child: Text(
                        s.cannotStartExam,
                        style: const TextStyle(fontSize: 13),
                      ),
                    ),
                  ),
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
