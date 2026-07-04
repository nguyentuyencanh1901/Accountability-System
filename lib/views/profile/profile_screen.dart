import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../controllers/auth_controller.dart';
import '../../controllers/profile_controller.dart';
import '../../l10n/app_strings.dart';
import '../../models/user/app_user.dart';
import '../../routes/app_routes.dart';
import '../../widgets/common/app_scaffold.dart';
import '../../widgets/common/message_banner.dart';
import '../../widgets/common/stat_card.dart';

/// Thông tin cá nhân — tương đương `Views/Profile/Index.cshtml`.
class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  void _load() {
    final userId = context.read<AuthController>().storage.userId;
    context.read<ProfileController>().loadProfile(userId);
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final profile = context.watch<ProfileController>();
    final auth = context.watch<AuthController>();
    final user = profile.user ??
        (auth.isLoggedIn
            ? AppUser.fromAuthStorage(
                userId: auth.storage.userId,
                username: auth.storage.username,
                fullName: auth.storage.fullName,
                email: auth.storage.email,
                userType: auth.storage.userType,
              )
            : null);

    return AppScaffold(
      title: s.profileTitle,
      body: Stack(
        children: [
          RefreshIndicator(
            onRefresh: () async => _load(),
            child: ListView(
              padding: const EdgeInsets.all(12),
              children: [
                if (profile.successMessage != null) ...[
                  SuccessBanner(message: profile.successMessage!),
                  const SizedBox(height: 8),
                ],
                if (profile.errorMessage != null) ...[
                  ErrorBanner(message: profile.errorMessage!),
                  const SizedBox(height: 8),
                ],
                if (user != null) ...[
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(14),
                      child: Column(
                        children: [
                          CircleAvatar(
                            radius: 28,
                            backgroundColor: Colors.blue.shade100,
                            child: Text(
                              user.fullName.isNotEmpty
                                  ? user.fullName[0].toUpperCase()
                                  : '?',
                              style: const TextStyle(
                                fontSize: 22,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ),
                          const SizedBox(height: 8),
                          Text(
                            user.fullName,
                            style: const TextStyle(
                              fontSize: 17,
                              fontWeight: FontWeight.w700,
                            ),
                          ),
                          Text(
                            '@${user.username}',
                            style: const TextStyle(
                              fontSize: 12,
                              color: Colors.grey,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 8),
                  _InfoTile(label: s.email, value: user.email),
                  _InfoTile(label: s.phone, value: user.phone),
                  _InfoTile(label: s.username, value: user.username),
                  const SizedBox(height: 10),
                  ElevatedButton.icon(
                    onPressed: () => context.push(AppRoutes.profileEdit),
                    icon: const Icon(Icons.edit_outlined, size: 18),
                    label: Text(s.editProfile),
                  ),
                  const SizedBox(height: 8),
                  OutlinedButton.icon(
                    onPressed: () => context.push(AppRoutes.changePassword),
                    icon: const Icon(Icons.lock_reset, size: 18),
                    label: Text(s.changePassword),
                  ),
                ],
              ],
            ),
          ),
          if (profile.isLoading) const LoadingOverlay(),
        ],
      ),
    );
  }
}

class _InfoTile extends StatelessWidget {
  const _InfoTile({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final display = value.trim().isEmpty ? '—' : value.trim();

    return Card(
      margin: const EdgeInsets.only(bottom: 6),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              label,
              style: const TextStyle(fontSize: 11, color: Colors.grey),
            ),
            const SizedBox(height: 4),
            Text(
              display,
              style: const TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.w600,
                color: Color(0xFF111827),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
