import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../controllers/locale_controller.dart';
import '../../l10n/app_strings.dart';

/// Nút chọn ngôn ngữ Vi / En.
class LocaleToggleButton extends StatelessWidget {
  const LocaleToggleButton({super.key});

  @override
  Widget build(BuildContext context) {
    final locale = context.watch<LocaleController>();
    final s = S.of(context);

    return PopupMenuButton<String>(
      tooltip: s.language,
      offset: const Offset(0, 40),
      onSelected: (code) => locale.setLanguage(code),
      itemBuilder: (_) => [
        CheckedPopupMenuItem(
          value: 'vi',
          checked: !locale.isEnglish,
          child: Text(s.vietnamese),
        ),
        CheckedPopupMenuItem(
          value: 'en',
          checked: locale.isEnglish,
          child: Text(s.english),
        ),
      ],
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 6),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.language, size: 18),
            const SizedBox(width: 2),
            Text(
              locale.isEnglish ? 'EN' : 'VI',
              style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w700),
            ),
          ],
        ),
      ),
    );
  }
}
