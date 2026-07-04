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

/// Sửa thông tin — tương đương `Views/Profile/Edit.cshtml`.
class EditProfileScreen extends StatefulWidget {
  const EditProfileScreen({super.key});

  @override
  State<EditProfileScreen> createState() => _EditProfileScreenState();
}

class _EditProfileScreenState extends State<EditProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  final _usernameController = TextEditingController();
  final _fullNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _phoneController = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _initForm());
  }

  Future<void> _initForm() async {
    final auth = context.read<AuthController>();
    final profile = context.read<ProfileController>();
    _usernameController.text = auth.storage.username;
    await profile.loadProfile(auth.storage.userId);
    final user = profile.user;
    if (user != null && mounted) {
      setState(() {
        if (user.username.isNotEmpty) {
          _usernameController.text = user.username;
        }
        _fullNameController.text = user.fullName;
        _emailController.text = user.email;
        _phoneController.text = user.phone;
      });
    }
  }

  @override
  void dispose() {
    _usernameController.dispose();
    _fullNameController.dispose();
    _emailController.dispose();
    _phoneController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    final auth = context.read<AuthController>();
    final profile = context.read<ProfileController>();

    final ok = await profile.updateProfile(
      userId: auth.storage.userId,
      fullName: _fullNameController.text,
      email: _emailController.text,
      phone: _phoneController.text,
    );

    if (ok && mounted) context.pop();
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final profile = context.watch<ProfileController>();

    return AppScaffold(
      title: s.editProfile,
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
                      controller: _usernameController,
                      readOnly: true,
                      enableInteractiveSelection: false,
                      decoration: InputDecoration(
                        labelText: s.username,
                        prefixIcon: const Icon(Icons.person_outline),
                        filled: true,
                        fillColor: const Color(0xFFF3F4F6),
                      ),
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _fullNameController,
                      decoration: InputDecoration(
                        labelText: s.fullName,
                        prefixIcon: const Icon(Icons.badge_outlined),
                      ),
                      validator: Validators.fullName,
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _emailController,
                      decoration: InputDecoration(
                        labelText: '${s.email} *',
                        prefixIcon: const Icon(Icons.email_outlined),
                      ),
                      validator: Validators.email,
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _phoneController,
                      decoration: InputDecoration(
                        labelText: s.phone,
                        prefixIcon: const Icon(Icons.phone_outlined),
                      ),
                    ),
                    const SizedBox(height: 22),
                    ElevatedButton(
                      onPressed: profile.isLoading ? null : _submit,
                      child: Text(s.save),
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
