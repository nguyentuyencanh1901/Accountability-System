import 'dart:ui';

import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../core/constants/app_constants.dart';
import '../l10n/app_strings.dart';

/// Quản lý ngôn ngữ giao diện (mặc định tiếng Việt).
class LocaleController extends ChangeNotifier {
  LocaleController(this._prefs) {
    final saved = _prefs.getString(AppConstants.keyLocale);
    if (saved == 'en' || saved == 'vi') {
      _languageCode = saved!;
    }
    S.languageCode = _languageCode;
  }

  final SharedPreferences _prefs;
  String _languageCode = 'vi';

  Locale get locale => Locale(_languageCode);
  String get languageCode => _languageCode;
  bool get isEnglish => _languageCode == 'en';

  Future<void> setLanguage(String code) async {
    if (code != 'vi' && code != 'en') return;
    if (_languageCode == code) return;
    _languageCode = code;
    S.languageCode = code;
    await _prefs.setString(AppConstants.keyLocale, code);
    notifyListeners();
  }
}
