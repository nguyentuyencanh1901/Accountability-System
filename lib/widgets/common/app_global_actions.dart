import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../controllers/auth_controller.dart';
import '../../l10n/app_strings.dart';
import '../../routes/app_routes.dart';
import 'locale_toggle.dart';

/// Nút đăng xuất + chọn ngôn ngữ — hiển thị trên mọi trang sau đăng nhập.
class AppGlobalActions {
  static Future<void> confirmLogout(BuildContext context) async {
    final s = S.of(context);
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(s.logoutConfirmTitle),
        content: Text(s.logoutConfirmMessage),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: Text(s.cancel),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: Text(s.logout),
          ),
        ],
      ),
    );

    if (confirmed != true || !context.mounted) return;

    await context.read<AuthController>().logout();
    if (context.mounted) context.go(AppRoutes.login);
  }

  static List<Widget> buttons(BuildContext context) => [
        const LocaleToggleButton(),
        IconButton(
          visualDensity: VisualDensity.compact,
          icon: const Icon(Icons.logout, size: 20),
          tooltip: S.of(context).logout,
          onPressed: () => confirmLogout(context),
        ),
      ];
}
