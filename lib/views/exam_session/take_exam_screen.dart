import 'dart:async';

import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../controllers/auth_controller.dart';
import '../../controllers/exam_session_controller.dart';
import '../../core/constants/app_constants.dart';
import '../../core/enums/app_enums.dart';
import '../../l10n/app_strings.dart';
import '../../core/theme/app_colors.dart';
import '../../models/exam/exam_session_question.dart';
import '../../core/utils/nav_index_helper.dart';
import '../../widgets/common/main_bottom_nav.dart';
import '../../widgets/common/app_global_actions.dart';
import '../../widgets/common/question_image_view.dart';

/// Màn làm bài thi — layout gọn, ưu tiên hiển thị đủ câu hỏi và đáp án.
class TakeExamScreen extends StatefulWidget {
  const TakeExamScreen({super.key, required this.sessionId});

  final int sessionId;

  @override
  State<TakeExamScreen> createState() => _TakeExamScreenState();
}

class _TakeExamScreenState extends State<TakeExamScreen>
    with WidgetsBindingObserver {
  Timer? _timer;
  Duration _remaining = Duration.zero;
  bool _initialized = false;
  bool _warningShown = false;
  final ScrollController _sideNavScroll = ScrollController();
  int _lastNavIndex = -1;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _timer?.cancel();
    _sideNavScroll.dispose();
    super.dispose();
  }

  /// Cuộn ô chọn câu bên phải tới câu đang làm.
  void _scrollSideNavTo(int index) {
    if (!_sideNavScroll.hasClients) return;
    const itemExtent = 32.0;
    final offset = (index * itemExtent)
        .clamp(0.0, _sideNavScroll.position.maxScrollExtent);
    _sideNavScroll.animateTo(
      offset,
      duration: const Duration(milliseconds: 200),
      curve: Curves.easeOut,
    );
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (state == AppLifecycleState.paused ||
        state == AppLifecycleState.inactive) {
      _recordViolation('Bạn đã rời ứng dụng trong lúc thi.');
    }
  }

  Future<void> _load() async {
    final auth = context.read<AuthController>();
    final sessionCtrl = context.read<ExamSessionController>();
    final takeCtrl = context.read<TakeExamController>();

    final ok = await sessionCtrl.loadTakeExam(
      widget.sessionId,
      auth.storage.userId,
    );

    if (!mounted) return;

    if (!ok) {
      if (sessionCtrl.successMessage != null) {
        context.replace('/exam-sessions/${widget.sessionId}');
      }
      return;
    }

    final data = sessionCtrl.takeExamData!;
    takeCtrl.init(data);
    _startTimer(data.endTime);
    setState(() => _initialized = true);
    _showExamRulesOnce();
  }

  void _showExamRulesOnce() {
    if (_warningShown) return;
    _warningShown = true;
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          duration: const Duration(seconds: 4),
          behavior: SnackBarBehavior.floating,
          margin: const EdgeInsets.fromLTRB(12, 0, 12, 88),
          content: Text(
            'Không rời ứng dụng khi đang thi. Vi phạm quá ${AppConstants.maxExamViolations} lần sẽ hủy bài.',
          ),
        ),
      );
    });
  }

  void _startTimer(DateTime endTime) {
    _timer?.cancel();
    void tick() {
      final diff = endTime.toUtc().difference(DateTime.now().toUtc());
      if (diff.isNegative) {
        _timer?.cancel();
        _autoSubmit();
        return;
      }
      setState(() => _remaining = diff);
    }

    tick();
    _timer = Timer.periodic(const Duration(seconds: 1), (_) => tick());
  }

  Future<void> _recordViolation(String message) async {
    final takeCtrl = context.read<TakeExamController>();
    if (takeCtrl.isSubmitting) return;

    takeCtrl.recordViolation();
    if (!mounted) return;

    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));

    if (takeCtrl.isViolationLimitReached) {
      await _cancelDueToViolation();
    }
  }

  Future<void> _cancelDueToViolation() async {
    final auth = context.read<AuthController>();
    final takeCtrl = context.read<TakeExamController>();
    await takeCtrl.cancelDueToViolation(auth.storage.userId);
    if (mounted) context.replace('/exam-sessions/${widget.sessionId}');
  }

  Future<void> _autoSubmit() async {
    final takeCtrl = context.read<TakeExamController>();
    final auth = context.read<AuthController>();
    await takeCtrl.submit(auth.storage.userId);
    if (mounted) context.replace('/exam-sessions/${widget.sessionId}');
  }

  Future<void> _submit() async {
    final takeCtrl = context.read<TakeExamController>();
    if (!takeCtrl.allAnswered) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            'Còn thiếu ${takeCtrl.unansweredCount} câu chưa trả lời.',
          ),
        ),
      );
      return;
    }

    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Nộp bài'),
        content: const Text('Bạn chắc chắn muốn nộp bài?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Hủy'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Nộp bài'),
          ),
        ],
      ),
    );

    if (confirm != true) return;

    final auth = context.read<AuthController>();
    final ok = await takeCtrl.submit(auth.storage.userId);
    if (!mounted) return;

    if (!ok) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(takeCtrl.errorMessage ?? 'Nộp bài thất bại.')),
      );
      return;
    }

    context.replace('/exam-sessions/${widget.sessionId}');
  }

  void _openQuestionSheet(
    List<ExamSessionQuestion> questions,
    TakeExamController takeCtrl,
  ) {
    showModalBottomSheet<void>(
      context: context,
      showDragHandle: true,
      builder: (ctx) {
        return SafeArea(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'Danh sách câu hỏi',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.w700),
                ),
                const SizedBox(height: 8),
                const _QuestionNavLegend(compact: false),
                const SizedBox(height: 12),
                Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  children: List.generate(questions.length, (index) {
                    final q = questions[index];
                    final answered =
                        (takeCtrl.answers[q.questionId] ?? []).isNotEmpty;
                    final isCurrent = index == takeCtrl.currentQuestionIndex;

                    return _QuestionNavTile(
                      index: index,
                      isCurrent: isCurrent,
                      isAnswered: answered,
                      isMultiple: QuestionType.isMultiple(q.questionType),
                      size: 48,
                      onTap: () {
                        takeCtrl.goToQuestion(index);
                        Navigator.pop(ctx);
                      },
                    );
                  }),
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  String _formatDuration(Duration d) {
    final h = d.inHours;
    final m = d.inMinutes.remainder(60).toString().padLeft(2, '0');
    final s = d.inSeconds.remainder(60).toString().padLeft(2, '0');
    return h > 0 ? '$h:$m:$s' : '$m:$s';
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final takeCtrl = context.watch<TakeExamController>();
    final sessionCtrl = context.watch<ExamSessionController>();

    if (!_initialized || takeCtrl.data == null) {
      return Scaffold(
        appBar: AppBar(
          title: Text(s.takeExam),
          actions: AppGlobalActions.buttons(context),
        ),
        body: sessionCtrl.isLoading
            ? const Center(child: CircularProgressIndicator())
            : Center(child: Text(sessionCtrl.errorMessage ?? 'Đang tải...')),
        bottomNavigationBar: MainBottomNav(
          currentIndex: bottomNavIndexForLocation('/exam-sessions/${widget.sessionId}/take'),
        ),
      );
    }

    final questions = takeCtrl.data!.session.questions;
    final current = questions[takeCtrl.currentQuestionIndex];
    final answeredCount = questions
        .where((q) => (takeCtrl.answers[q.questionId] ?? []).isNotEmpty)
        .length;
    final progress = questions.isEmpty ? 0.0 : answeredCount / questions.length;
    final isUrgent = _remaining.inMinutes < 5;
    final isMultiple = QuestionType.isMultiple(current.questionType);
    final typeColor = isMultiple ? AppColors.primaryLight : AppColors.teal;

    if (_lastNavIndex != takeCtrl.currentQuestionIndex) {
      _lastNavIndex = takeCtrl.currentQuestionIndex;
      WidgetsBinding.instance.addPostFrameCallback((_) {
        _scrollSideNavTo(takeCtrl.currentQuestionIndex);
      });
    }

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        toolbarHeight: 44,
        automaticallyImplyLeading: false,
        titleSpacing: 8,
        title: Text(
          takeCtrl.data!.session.examSetName,
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          style: const TextStyle(fontSize: 14),
        ),
        actions: [
          IconButton(
            visualDensity: VisualDensity.compact,
            padding: EdgeInsets.zero,
            constraints: const BoxConstraints(minWidth: 32, minHeight: 32),
            icon: const Icon(Icons.info_outline, size: 18),
            tooltip: 'Quy chế thi',
            onPressed: () {
              showDialog<void>(
                context: context,
                builder: (ctx) => AlertDialog(
                  title: const Text('Quy chế thi'),
                  content: Text(
                    'Không rời ứng dụng khi đang thi.\n'
                    'Vi phạm quá ${AppConstants.maxExamViolations} lần → hủy bài, không được thi lại.',
                  ),
                  actions: [
                    TextButton(
                      onPressed: () => Navigator.pop(ctx),
                      child: const Text('Đã hiểu'),
                    ),
                  ],
                ),
              );
            },
          ),
          _CompactPill(
            icon: Icons.timer_outlined,
            label: _formatDuration(_remaining),
            color: isUrgent ? AppColors.error : Colors.white24,
            textColor: Colors.white,
          ),
          _CompactPill(
            label: s.violationBadge(takeCtrl.violationCount),
            color: takeCtrl.violationCount > 0
                ? AppColors.error.withValues(alpha: 0.3)
                : Colors.white12,
            textColor: Colors.white,
          ),
          Padding(
            padding: const EdgeInsets.only(right: 6),
            child: _CompactPill(
              label: '${takeCtrl.currentQuestionIndex + 1}/${questions.length}',
              color: Colors.white12,
              textColor: Colors.white,
            ),
          ),
          ...AppGlobalActions.buttons(context),
        ],
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(3),
          child: LinearProgressIndicator(
            value: progress,
            minHeight: 3,
            backgroundColor: Colors.white12,
            color: AppColors.teal,
          ),
        ),
      ),
      body: Column(
        children: [
          Expanded(
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Expanded(
                  child: ListView(
                    padding: const EdgeInsets.fromLTRB(10, 6, 4, 2),
                    children: [
                      _CompactQuestionHeader(
                        question: current,
                        isMultiple: isMultiple,
                        typeColor: typeColor,
                      ),
                      Padding(
                        padding: const EdgeInsets.only(bottom: 8),
                        child: QuestionImageView(
                          imageUrl: current.imageUrl,
                          height: 80,
                          viewerTitle: 'Ảnh câu hỏi ${current.sortOrder}',
                        ),
                      ),
                      ...current.answerOptions.map((opt) {
                        final selected =
                            (takeCtrl.answers[current.questionId] ?? [])
                                .contains(opt.id);
                        return _CompactAnswerTile(
                          content: opt.content,
                          selected: selected,
                          isMultiple: isMultiple,
                          typeColor: typeColor,
                          onTap: () => takeCtrl.selectAnswer(
                            current.questionId,
                            opt.id,
                            isMultiple: isMultiple,
                          ),
                        );
                      }),
                    ],
                  ),
                ),
                _SideQuestionNavigator(
                  scrollController: _sideNavScroll,
                  questions: questions,
                  currentIndex: takeCtrl.currentQuestionIndex,
                  answers: takeCtrl.answers,
                  onTap: takeCtrl.goToQuestion,
                ),
              ],
            ),
          ),
          Material(
            elevation: 4,
            color: Colors.white,
            child: SafeArea(
              top: false,
              bottom: false,
              child: Padding(
                padding: const EdgeInsets.fromLTRB(8, 6, 8, 6),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const _QuestionNavLegend(horizontal: true),
                    const SizedBox(height: 4),
                    Row(
                      children: [
                        Expanded(
                          child: OutlinedButton.icon(
                            onPressed: takeCtrl.currentQuestionIndex > 0
                                ? takeCtrl.previousQuestion
                                : null,
                            icon: const Icon(Icons.arrow_back, size: 16),
                            label: const Text('Trước'),
                            style: OutlinedButton.styleFrom(
                              minimumSize: const Size(0, 34),
                              padding:
                                  const EdgeInsets.symmetric(horizontal: 6),
                              tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                              visualDensity: VisualDensity.compact,
                              textStyle: const TextStyle(
                                fontSize: 12,
                                fontWeight: FontWeight.w600,
                              ),
                              foregroundColor: AppColors.headerDark,
                              side: const BorderSide(color: AppColors.headerDark),
                            ),
                          ),
                        ),
                        const SizedBox(width: 6),
                        InkWell(
                          onTap: () => _openQuestionSheet(questions, takeCtrl),
                          borderRadius: BorderRadius.circular(6),
                          child: Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 8,
                              vertical: 6,
                            ),
                            decoration: BoxDecoration(
                              color: AppColors.blueSoft,
                              borderRadius: BorderRadius.circular(6),
                              border: Border.all(color: AppColors.primaryLight),
                            ),
                            child: Column(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Text(
                                  '$answeredCount/${questions.length}',
                                  style: const TextStyle(
                                    fontSize: 12,
                                    fontWeight: FontWeight.w800,
                                    color: AppColors.primaryLight,
                                  ),
                                ),
                                const Text(
                                  'đã chọn',
                                  style: TextStyle(
                                    fontSize: 9,
                                    color: AppColors.primaryLight,
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ),
                        const SizedBox(width: 6),
                        Expanded(
                          child: FilledButton.icon(
                            onPressed: takeCtrl.currentQuestionIndex <
                                    questions.length - 1
                                ? takeCtrl.nextQuestion
                                : null,
                            icon: const Icon(Icons.arrow_forward, size: 16),
                            label: const Text('Tiếp'),
                            style: FilledButton.styleFrom(
                              minimumSize: const Size(0, 34),
                              padding:
                                  const EdgeInsets.symmetric(horizontal: 6),
                              tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                              visualDensity: VisualDensity.compact,
                              textStyle: const TextStyle(
                                fontSize: 12,
                                fontWeight: FontWeight.w600,
                              ),
                              backgroundColor: AppColors.primaryLight,
                            ),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 6),
                    SizedBox(
                      width: double.infinity,
                      child: FilledButton.icon(
                        onPressed: takeCtrl.isSubmitting ? null : _submit,
                        style: FilledButton.styleFrom(
                          backgroundColor: AppColors.teal,
                          minimumSize: const Size(double.infinity, 36),
                          padding: const EdgeInsets.symmetric(horizontal: 12),
                          tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                          visualDensity: VisualDensity.compact,
                        ),
                        icon: takeCtrl.isSubmitting
                            ? const SizedBox(
                                width: 16,
                                height: 16,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                  color: Colors.white,
                                ),
                              )
                            : const Icon(Icons.send_rounded, size: 16),
                        label: const Text(
                          'Nộp bài',
                          style: TextStyle(
                            fontSize: 13,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
      bottomNavigationBar: MainBottomNav(
        currentIndex: bottomNavIndexForLocation(
          '/exam-sessions/${widget.sessionId}/take',
        ),
      ),
    );
  }
}

/// Ô chọn câu dùng chung — side nav và bottom sheet.
class _QuestionNavTile extends StatelessWidget {
  const _QuestionNavTile({
    required this.index,
    required this.isCurrent,
    required this.isAnswered,
    required this.isMultiple,
    required this.onTap,
    this.size = 40,
  });

  final int index;
  final bool isCurrent;
  final bool isAnswered;
  final bool isMultiple;
  final VoidCallback onTap;
  final double size;

  @override
  Widget build(BuildContext context) {
    final typeColor = isMultiple ? AppColors.primaryLight : AppColors.teal;

    Color bg;
    Color borderColor;
    Color textColor;
    double borderWidth;

    if (isCurrent) {
      bg = AppColors.headerDark;
      borderColor = AppColors.headerDark;
      textColor = Colors.white;
      borderWidth = 2;
    } else if (isAnswered) {
      bg = AppColors.teal;
      borderColor = AppColors.teal;
      textColor = Colors.white;
      borderWidth = 2;
    } else {
      bg = Colors.white;
      borderColor = AppColors.border;
      textColor = AppColors.textMuted;
      borderWidth = 1;
    }

    return Material(
      color: bg,
      borderRadius: BorderRadius.circular(size > 34 ? 8 : 6),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(size > 34 ? 8 : 6),
        child: Container(
          width: size,
          height: size,
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(size > 34 ? 8 : 6),
            border: Border.all(color: borderColor, width: borderWidth),
          ),
          child: Stack(
            alignment: Alignment.center,
            children: [
              Text(
                '${index + 1}',
                style: TextStyle(
                  fontSize: size > 44 ? 16 : (size > 34 ? 13 : 11),
                  fontWeight: FontWeight.w800,
                  color: textColor,
                ),
              ),
              if (isAnswered && !isCurrent)
                Positioned(
                  right: 1,
                  top: 1,
                  child: Icon(
                    Icons.check_circle,
                    size: size > 44 ? 14 : (size > 34 ? 11 : 9),
                    color: Colors.white.withValues(alpha: 0.9),
                  ),
                ),
              if (size >= 34 && !isCurrent && !isAnswered)
                Positioned(
                  bottom: 3,
                  child: Container(
                    width: 6,
                    height: 6,
                    decoration: BoxDecoration(
                      color: typeColor.withValues(alpha: 0.5),
                      shape: BoxShape.circle,
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

/// Chú thích màu ô chọn câu.
class _QuestionNavLegend extends StatelessWidget {
  const _QuestionNavLegend({
    this.compact = true,
    this.iconOnly = false,
    this.horizontal = false,
  });

  final bool compact;
  final bool iconOnly;
  final bool horizontal;

  @override
  Widget build(BuildContext context) {
    if (horizontal) {
      return Wrap(
        alignment: WrapAlignment.center,
        spacing: 8,
        runSpacing: 2,
        children: const [
          _LegendRow(color: AppColors.headerDark, label: 'Đang làm'),
          _LegendRow(color: AppColors.teal, label: 'Đã chọn ✓'),
          _LegendRow(
            color: AppColors.border,
            label: 'Chưa làm',
            filled: false,
          ),
        ],
      );
    }

    if (iconOnly) {
      return const Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          _LegendRow(color: AppColors.headerDark, label: '', iconOnly: true),
          SizedBox(height: 5),
          _LegendRow(
            color: AppColors.teal,
            label: '',
            iconOnly: true,
            showCheck: true,
          ),
          SizedBox(height: 5),
          _LegendRow(
            color: AppColors.border,
            label: '',
            filled: false,
            iconOnly: true,
          ),
        ],
      );
    }

    if (compact) {
      return const Column(
        children: [
          _LegendRow(
            color: AppColors.headerDark,
            label: 'Đang làm',
          ),
          SizedBox(height: 3),
          _LegendRow(
            color: AppColors.teal,
            label: 'Đã chọn ✓',
          ),
          SizedBox(height: 3),
          _LegendRow(
            color: AppColors.border,
            label: 'Chưa làm',
            filled: false,
          ),
        ],
      );
    }

    return Wrap(
      spacing: 12,
      runSpacing: 4,
      children: const [
        _LegendRow(color: AppColors.headerDark, label: 'Navy = đang làm'),
        _LegendRow(color: AppColors.teal, label: 'Xanh lá + ✓ = đã chọn'),
        _LegendRow(
          color: AppColors.border,
          label: 'Trắng = chưa làm',
          filled: false,
        ),
      ],
    );
  }
}

class _LegendRow extends StatelessWidget {
  const _LegendRow({
    required this.color,
    required this.label,
    this.filled = true,
    this.iconOnly = false,
    this.showCheck = false,
  });

  final Color color;
  final String label;
  final bool filled;
  final bool iconOnly;
  final bool showCheck;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Container(
          width: 10,
          height: 10,
          alignment: Alignment.center,
          decoration: BoxDecoration(
            color: filled ? color : Colors.white,
            border: Border.all(color: color, width: 1.5),
            borderRadius: BorderRadius.circular(3),
          ),
          child: showCheck
              ? Icon(
                  Icons.check,
                  size: 8,
                  color: Colors.white.withValues(alpha: 0.95),
                )
              : null,
        ),
        if (!iconOnly) ...[
          const SizedBox(width: 5),
          Text(
            label,
            style: const TextStyle(fontSize: 9, color: AppColors.textMuted),
            overflow: TextOverflow.ellipsis,
            maxLines: 1,
          ),
        ],
      ],
    );
  }
}

/// Thanh chọn câu cố định bên phải — nhảy nhanh giữa các câu hỏi.
class _SideQuestionNavigator extends StatelessWidget {
  const _SideQuestionNavigator({
    required this.scrollController,
    required this.questions,
    required this.currentIndex,
    required this.answers,
    required this.onTap,
  });

  final ScrollController scrollController;
  final List<ExamSessionQuestion> questions;
  final int currentIndex;
  final Map<int, List<int>> answers;
  final ValueChanged<int> onTap;

  @override
  Widget build(BuildContext context) {
    final tileSize = _sideTileSize(questions.length);
    const tileGap = 2.0;
    final panelWidth = tileSize + 8;

    return ClipRRect(
      borderRadius: BorderRadius.circular(8),
      child: Container(
        width: panelWidth,
        margin: const EdgeInsets.fromLTRB(0, 8, 4, 4),
        decoration: BoxDecoration(
          color: Colors.white,
          border: Border.all(color: AppColors.border),
        ),
        child: Column(
          children: [
            Container(
              width: double.infinity,
              padding: const EdgeInsets.symmetric(vertical: 4),
              color: AppColors.blueSoft,
              child: const Text(
                'Câu',
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontSize: 9,
                  fontWeight: FontWeight.w700,
                  color: AppColors.primaryLight,
                ),
              ),
            ),
            Expanded(
              child: ListView.builder(
                controller: scrollController,
                padding: const EdgeInsets.symmetric(vertical: 2, horizontal: 2),
                itemCount: questions.length,
                itemBuilder: (context, index) {
                  final q = questions[index];
                  final answered = (answers[q.questionId] ?? []).isNotEmpty;
                  final isCurrent = index == currentIndex;

                  return Padding(
                    padding: const EdgeInsets.only(bottom: tileGap),
                    child: Center(
                      child: _QuestionNavTile(
                        index: index,
                        isCurrent: isCurrent,
                        isAnswered: answered,
                        isMultiple: QuestionType.isMultiple(q.questionType),
                        size: tileSize,
                        onTap: () => onTap(index),
                      ),
                    ),
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }

  static double _sideTileSize(int count) {
    if (count > 10) return 26;
    if (count > 8) return 28;
    if (count > 5) return 30;
    return 32;
  }
}

/// Header câu hỏi gọn: số câu + điểm + loại trên một dòng.
class _CompactQuestionHeader extends StatelessWidget {
  const _CompactQuestionHeader({
    required this.question,
    required this.isMultiple,
    required this.typeColor,
  });

  final ExamSessionQuestion question;
  final bool isMultiple;
  final Color typeColor;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 6),
      padding: const EdgeInsets.all(10),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: AppColors.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                width: 24,
                height: 24,
                alignment: Alignment.center,
                decoration: BoxDecoration(
                  color: AppColors.headerDark,
                  borderRadius: BorderRadius.circular(5),
                ),
                child: Text(
                  '${question.sortOrder}',
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.bold,
                    fontSize: 12,
                  ),
                ),
              ),
              const SizedBox(width: 6),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                decoration: BoxDecoration(
                  color: AppColors.amberSoft,
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Text(
                  '${question.points}đ',
                  style: const TextStyle(
                    fontSize: 10,
                    fontWeight: FontWeight.w600,
                    color: AppColors.amber,
                  ),
                ),
              ),
              const SizedBox(width: 4),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                decoration: BoxDecoration(
                  color: isMultiple ? AppColors.blueSoft : AppColors.tealSoft,
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      QuestionType.icon(question.questionType),
                      size: 11,
                      color: typeColor,
                    ),
                    const SizedBox(width: 3),
                    Text(
                      isMultiple ? 'Chọn nhiều' : 'Chọn một',
                      style: TextStyle(
                        fontSize: 10,
                        fontWeight: FontWeight.w600,
                        color: typeColor,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            question.content,
            style: const TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w500,
              height: 1.3,
            ),
          ),
        ],
      ),
    );
  }
}

/// Đáp án dạng tile gọn — giảm padding để hiện nhiều đáp án hơn.
class _CompactAnswerTile extends StatelessWidget {
  const _CompactAnswerTile({
    required this.content,
    required this.selected,
    required this.isMultiple,
    required this.typeColor,
    required this.onTap,
  });

  final String content;
  final bool selected;
  final bool isMultiple;
  final Color typeColor;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 4),
      child: Material(
        color: selected
            ? (isMultiple ? AppColors.blueSoft : AppColors.tealSoft)
            : Colors.white,
        borderRadius: BorderRadius.circular(6),
        child: InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(6),
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(6),
              border: Border.all(
                color: selected ? typeColor : AppColors.border,
                width: selected ? 1.5 : 1,
              ),
            ),
            child: Row(
              children: [
                Icon(
                  isMultiple
                      ? (selected
                          ? Icons.check_box_rounded
                          : Icons.check_box_outline_blank)
                      : (selected
                          ? Icons.radio_button_checked
                          : Icons.radio_button_off),
                  size: 18,
                  color: selected ? typeColor : AppColors.textMuted,
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    content,
                    style: TextStyle(
                      fontSize: 13,
                      height: 1.25,
                      fontWeight: selected ? FontWeight.w600 : FontWeight.w400,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _CompactPill extends StatelessWidget {
  const _CompactPill({
    required this.label,
    this.icon,
    this.color = Colors.white12,
    this.textColor = Colors.white,
  });

  final String label;
  final IconData? icon;
  final Color color;
  final Color textColor;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.symmetric(vertical: 8, horizontal: 2),
      padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 3),
      decoration: BoxDecoration(
        color: color,
        borderRadius: BorderRadius.circular(5),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          if (icon != null) ...[
            Icon(icon, size: 12, color: textColor),
            const SizedBox(width: 3),
          ],
          Text(
            label,
            style: TextStyle(
              color: textColor,
              fontSize: 11,
              fontWeight: FontWeight.w600,
              fontFeatures: const [FontFeature.tabularFigures()],
            ),
          ),
        ],
      ),
    );
  }
}
