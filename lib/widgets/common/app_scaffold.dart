import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../core/constants/app_constants.dart';
import '../../l10n/app_strings.dart';
import '../../core/theme/app_colors.dart';
import '../../core/utils/nav_index_helper.dart';
import 'main_bottom_nav.dart';

/// Layout chính sau đăng nhập — tương đương `_AppLayout.cshtml`.
class AppScaffold extends StatelessWidget {
  const AppScaffold({
    super.key,
    required this.title,
    required this.body,
    this.actions,
    this.floatingActionButton,
    this.bottomNavigationBar,
    this.navIndex,
    this.showBottomNav = true,
  });

  final String title;
  final Widget body;
  final List<Widget>? actions;
  final Widget? floatingActionButton;
  final Widget? bottomNavigationBar;
  final int? navIndex;
  final bool showBottomNav;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final location = GoRouterState.of(context).matchedLocation;
    final index = navIndex ?? bottomNavIndexForLocation(location);

    return Scaffold(
      appBar: AppBar(
        title: Row(
          children: [
            const Icon(Icons.assignment_turned_in_outlined, size: 18),
            const SizedBox(width: 8),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    s.appName,
                    style: TextStyle(
                      fontSize: 10,
                      color: Colors.white.withValues(alpha: 0.75),
                      fontWeight: FontWeight.w400,
                    ),
                  ),
                  Text(
                    title,
                    style: const TextStyle(
                      fontSize: 15,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
        actions: actions,
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(2),
          child: Container(height: 2, color: AppColors.accentBlue),
        ),
      ),
      body: body,
      floatingActionButton: floatingActionButton,
      bottomNavigationBar: bottomNavigationBar ??
          (showBottomNav ? MainBottomNav(currentIndex: index) : null),
    );
  }
}

/// Footer ứng dụng.
class AppFooter extends StatelessWidget {
  const AppFooter({super.key});

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final year = DateTime.now().year;
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 12),
      child: Text(
        '© $year ${s.appName} · ${AppConstants.supportEmail}',
        textAlign: TextAlign.center,
        style: const TextStyle(fontSize: 11, color: AppColors.textMuted),
      ),
    );
  }
}
