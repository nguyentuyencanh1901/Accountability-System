import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../core/theme/app_colors.dart';
import '../../l10n/app_strings.dart';
import '../../routes/app_routes.dart';

/// Bottom navigation — tương đương menu header WebApp.
class MainBottomNav extends StatelessWidget {
  const MainBottomNav({super.key, required this.currentIndex});

  final int currentIndex;

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);

    return NavigationBar(
      selectedIndex: currentIndex,
      onDestinationSelected: (index) {
        switch (index) {
          case 0:
            context.go(AppRoutes.home);
          case 1:
            context.go(AppRoutes.examSets);
          case 2:
            context.go(AppRoutes.examInProgress);
          case 3:
            context.go(AppRoutes.examHistory);
          case 4:
            context.go(AppRoutes.profile);
        }
      },
      destinations: [
        NavigationDestination(
          icon: const Icon(Icons.home_outlined),
          selectedIcon: const Icon(Icons.home),
          label: s.navHome,
        ),
        NavigationDestination(
          icon: const Icon(Icons.event_note_outlined),
          selectedIcon: const Icon(Icons.event_note),
          label: s.navExams,
        ),
        NavigationDestination(
          icon: const Icon(Icons.edit_note_outlined),
          selectedIcon: const Icon(Icons.edit_note),
          label: s.navInProgress,
        ),
        NavigationDestination(
          icon: const Icon(Icons.history_outlined),
          selectedIcon: const Icon(Icons.history),
          label: s.navHistory,
        ),
        NavigationDestination(
          icon: const Icon(Icons.person_outline),
          selectedIcon: const Icon(Icons.person),
          label: s.navProfile,
        ),
      ],
      indicatorColor: AppColors.blueSoft,
    );
  }
}
