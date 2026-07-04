import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../controllers/auth_controller.dart';
import '../../controllers/profile_controller.dart';
import '../../l10n/app_strings.dart';
import '../../core/utils/validators.dart';
import '../../widgets/common/app_scaffold.dart';
import '../../widgets/common/message_banner.dart';
import '../../widgets/common/stat_card.dart';

/// Đổi mật khẩu — tương đương `Views/Profile/ChangePassword.cshtml`.
class ChangePasswordScreen extends StatefulWidget {
  const ChangePasswordScreen({super.key});

  @override
  State<ChangePasswordScreen> createState() => _ChangePasswordScreenState();
}

class _ChangePasswordScreenState extends State<ChangePasswordScreen> {
  final _formKey = GlobalKey<FormState>();
  final _currentController = TextEditingController();
  final _newController = TextEditingController();
  final _confirmController = TextEditingController();

  @override
  void dispose() {
    _currentController.dispose();
    _newController.dispose();
    _confirmController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    final auth = context.read<AuthController>();
    final profile = context.read<ProfileController>();

    final ok = await profile.changePassword(
      userId: auth.storage.userId,
      currentPassword: _currentController.text,
      newPassword: _newController.text,
      confirmPassword: _confirmController.text,
    );

    if (ok && mounted) context.pop();
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final profile = context.watch<ProfileController>();

    return AppScaffold(
      title: s.changePassword,
      body: Stack(
        children: [
          ListView(
            padding: const EdgeInsets.all(16),
            children: [
              if (profile.errorMessage != null) ...[
                ErrorBanner(message: profile.errorMessage!),
                const SizedBox(height: 12),
              ],
              Form(
                key: _formKey,
                child: Column(
                  children: [
                    TextFormField(
                      controller: _currentController,
                      obscureText: true,
                      decoration: InputDecoration(
                        labelText: s.currentPassword,
                        prefixIcon: const Icon(Icons.lock_outline),
                      ),
                      validator: (v) => Validators.required(
                        v,
                        message: s.requiredCurrentPassword,
                      ),
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _newController,
                      obscureText: true,
                      decoration: InputDecoration(
                        labelText: s.newPassword,
                        prefixIcon: const Icon(Icons.lock_reset),
                      ),
                      validator: (v) {
                        if (v == null || v.isEmpty) {
                          return s.requiredNewPassword;
                        }
                        return Validators.password(v);
                      },
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _confirmController,
                      obscureText: true,
                      decoration: InputDecoration(
                        labelText: s.confirmNewPassword,
                        prefixIcon: const Icon(Icons.lock_reset),
                      ),
                      validator: (v) {
                        if (v == null || v.isEmpty) {
                          return s.requiredConfirmNewPassword;
                        }
                        return Validators.confirmPassword(v, _newController.text);
                      },
                    ),
                    const SizedBox(height: 22),
                    ElevatedButton(
                      onPressed: profile.isLoading ? null : _submit,
                      child: Text(s.changePassword),
                    ),
                  ],
                ),
              ),
            ],
          ),
          if (profile.isLoading) const LoadingOverlay(),
        ],
      ),
    );
  }
}
