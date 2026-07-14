import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../controllers/auth_controller.dart';
import '../../controllers/home_controller.dart';
import '../../core/constants/app_constants.dart';
import '../../core/enums/app_enums.dart';
import '../../core/theme/app_colors.dart';
import '../../core/utils/display_helper.dart';
import '../../core/utils/vietnam_time_helper.dart';
import '../../l10n/app_strings.dart';
import '../../routes/app_routes.dart';
import '../../widgets/common/app_scaffold.dart';
import '../../widgets/common/dashboard_widgets.dart';
import '../../widgets/common/message_banner.dart';
import '../../widgets/common/stat_card.dart';

/// Trang chủ dashboard — tương đương `Views/Home/Index.cshtml`.
class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  void _load() {
    final auth = context.read<AuthController>();
    context.read<HomeController>().loadDashboard(
          userId: auth.storage.userId,
          fullName: auth.fullName,
          username: auth.username,
        );
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final auth = context.watch<AuthController>();
    final home = context.watch<HomeController>();
    final dashboard = home.dashboard;
    final displayName = (dashboard?.fullName ?? auth.fullName).isNotEmpty
        ? (dashboard?.fullName ?? auth.fullName)
        : (dashboard?.username ?? auth.username);

    return AppScaffold(
      title: s.homeTitle,
      body: Stack(
        children: [
          RefreshIndicator(
            onRefresh: () async => _load(),
            child: ListView(
              padding: EdgeInsets.zero,
              children: [
                _DashboardHero(displayName: displayName, s: s),
                Padding(
                  padding: const EdgeInsets.fromLTRB(12, 0, 12, 12),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      if (dashboard?.errorMessage != null) ...[
                        const SizedBox(height: 8),
                        ErrorBanner(message: dashboard!.errorMessage!),
                      ],
                      const SizedBox(height: 10),
                      Row(
                        children: [
                          DashboardStatTile(
                            label: s.statAssigned,
                            value: '${dashboard?.totalExamSets ?? 0}',
                            icon: Icons.layers_outlined,
                            color: AppColors.primaryLight,
                            softColor: AppColors.blueSoft,
                            onTap: () => context.go(AppRoutes.examSets),
                          ),
                          const SizedBox(width: 6),
                          DashboardStatTile(
                            label: s.statInProgress,
                            value: '${dashboard?.inProgressSessions ?? 0}',
                            icon: Icons.edit_note_outlined,
                            color: AppColors.amber,
                            softColor: AppColors.amberSoft,
                            onTap: () => context.go(AppRoutes.examInProgress),
                          ),
                          const SizedBox(width: 6),
                          DashboardStatTile(
                            label: s.statCompleted,
                            value: '${dashboard?.completedSessions ?? 0}',
                            icon: Icons.task_alt,
                            color: AppColors.teal,
                            softColor: AppColors.tealSoft,
                            onTap: () => context.go(AppRoutes.examHistory),
                          ),
                        ],
                      ),
                      const SizedBox(height: 8),
                      DashboardActionCard(
                        title: s.startExamTitle,
                        description: s.startExamDesc,
                        icon: Icons.menu_book_rounded,
                        accentColor: AppColors.primaryLight,
                        buttonLabel: s.viewExams,
                        onPressed: () => context.go(AppRoutes.examSets),
                      ),
                      const SizedBox(height: 8),
                      DashboardActionCard(
                        title: s.continueExamTitle,
                        description: s.continueExamDesc,
                        icon: Icons.play_circle_outline,
                        accentColor: AppColors.amber,
                        outlinedButton: true,
                        buttonLabel: s.allCount(
                            dashboard?.inProgressSessions ?? 0),
                        onPressed: () => context.go(AppRoutes.examInProgress),
                        child: dashboard != null &&
                                dashboard.recentInProgress.isNotEmpty
                            ? Column(
                                children: dashboard.recentInProgress
                                    .take(3)
                                    .map(
                                      (s) => Padding(
                                        padding:
                                            const EdgeInsets.only(bottom: 6),
                                        child: DashboardMiniListItem(
                                          title: s.examSetName,
                                          subtitle:
                                              '${ExamType.label(s.examType)} · ${VietnamTimeHelper.formatInstant(s.startedAt, pattern: 'dd/MM HH:mm')}',
                                          accentColor: AppColors.amber,
                                          onTap: () => context.push(
                                            '/exam-sessions/${s.id}/take',
                                          ),
                                        ),
                                      ),
                                    )
                                    .toList(),
                              )
                            : Container(
                                width: double.infinity,
                                padding: const EdgeInsets.all(10),
                                decoration: BoxDecoration(
                                  color: AppColors.amberSoft,
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                child: Text(
                                  s.noInProgressExams,
                                  style: TextStyle(
                                    fontSize: 12,
                                    color: AppColors.amber,
                                  ),
                                ),
                              ),
                      ),
                      const SizedBox(height: 8),
                      DashboardActionCard(
                        title: s.historyTitle,
                        description: s.historyDesc,
                        icon: Icons.history_rounded,
                        accentColor: AppColors.teal,
                        outlinedButton: true,
                        buttonLabel: s.viewHistory,
                        onPressed: () => context.go(AppRoutes.examHistory),
                        child: dashboard != null &&
                                dashboard.recentCompleted.isNotEmpty
                            ? Column(
                                children: dashboard.recentCompleted
                                    .take(AppConstants.defaultPageSize)
                                    .map(
                                      (s) => Padding(
                                        padding:
                                            const EdgeInsets.only(bottom: 6),
                                        child: DashboardMiniListItem(
                                          title: s.examSetName,
                                          subtitle: s.finishedAt != null
                                              ? VietnamTimeHelper.formatInstant(
                                                  s.finishedAt!,
                                                )
                                              : ExamSessionStatus.label(
                                                  s.status,
                                                ),
                                          accentColor: AppColors.teal,
                                          trailing: Text(
                                            '${s.totalScore.toStringAsFixed(0)}/${s.maxScore.toStringAsFixed(0)}',
                                            style: TextStyle(
                                              fontWeight: FontWeight.w700,
                                              color: _scoreColor(
                                                DisplayHelper.scoreLevel(
                                                  s.totalScore,
                                                  s.maxScore,
                                                ),
                                              ),
                                            ),
                                          ),
                                          onTap: () => context.push(
                                            '/exam-sessions/${s.id}',
                                          ),
                                        ),
                                      ),
                                    )
                                    .toList(),
                              )
                            : Container(
                                width: double.infinity,
                                padding: const EdgeInsets.all(10),
                                decoration: BoxDecoration(
                                  color: AppColors.tealSoft,
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                child: Text(
                                  s.noResults,
                                  style: TextStyle(
                                    fontSize: 12,
                                    color: AppColors.teal,
                                  ),
                                ),
                              ),
                      ),
                      const SizedBox(height: 16),
                      const AppFooter(),
                    ],
                  ),
                ),
              ],
            ),
          ),
          if (home.isLoading) const LoadingOverlay(),
        ],
      ),
    );
  }

  Color _scoreColor(String level) => switch (level) {
        'high' => AppColors.teal,
        'mid' => AppColors.amber,
        _ => AppColors.error,
      };
}

/// Hero banner trang chủ — tương đương `.dashboard-hero` WebApp.
class _DashboardHero extends StatelessWidget {
  const _DashboardHero({required this.displayName, required this.s});

  final String displayName;
  final S s;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [AppColors.headerDark, AppColors.headerMid, Color(0xFF243B55)],
        ),
      ),
      child: Stack(
        children: [
          Positioned(
            right: -30,
            top: -20,
            child: Icon(
              Icons.assignment_turned_in_outlined,
              size: 140,
              color: Colors.white.withValues(alpha: 0.06),
            ),
          ),
          Positioned(
            left: -20,
            bottom: -30,
            child: Icon(
              Icons.school_outlined,
              size: 100,
              color: Colors.white.withValues(alpha: 0.05),
            ),
          ),
          Padding(
            padding: const EdgeInsets.fromLTRB(14, 16, 14, 16),
            child: Row(
              children: [
                Container(
                  width: 44,
                  height: 44,
                  decoration: BoxDecoration(
                    color: Colors.white.withValues(alpha: 0.15),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(
                      color: Colors.white.withValues(alpha: 0.25),
                    ),
                  ),
                  child: Center(
                    child: Text(
                      displayName.isNotEmpty
                          ? displayName[0].toUpperCase()
                          : 'T',
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        s.hello,
                        style: TextStyle(
                          color: Colors.white.withValues(alpha: 0.8),
                          fontSize: 11,
                        ),
                      ),
                      Text(
                        displayName,
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 18,
                          fontWeight: FontWeight.w800,
                        ),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        s.homeSubtitle,
                        style: TextStyle(
                          color: Colors.white.withValues(alpha: 0.82),
                          fontSize: 11,
                          height: 1.3,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
          Positioned(
            bottom: 0,
            left: 0,
            right: 0,
            child: Container(height: 3, color: AppColors.accentBlue),
          ),
        ],
      ),
    );
  }
}
