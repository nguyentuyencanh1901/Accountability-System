import '../../l10n/app_strings.dart';

/// Bộ validator form — ánh xạ quy tắc `[Required]`, `[EmailAddress]`, `[MinLength]` trong WebApp ViewModels.
class Validators {
  static String? required(String? value, {String? message}) {
    if (value == null || value.trim().isEmpty) {
      return message ?? S.current.requiredFullName;
    }
    return null;
  }

  static String? email(String? value) {
    if (value == null || value.trim().isEmpty) {
      return S.current.requiredEmail;
    }
    final pattern = RegExp(r'^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$');
    if (!pattern.hasMatch(value.trim())) return S.current.invalidEmail;
    return null;
  }

  static String? password(String? value, {int minLength = 6}) {
    if (value == null || value.isEmpty) return S.current.requiredPassword;
    if (value.length < minLength) {
      return S.current.passwordMinLength(minLength);
    }
    return null;
  }

  static String? confirmPassword(String? value, String password) {
    if (value == null || value.isEmpty) {
      return S.current.requiredConfirmPassword;
    }
    if (value != password) return S.current.passwordMismatch;
    return null;
  }

  static String? username(String? value) =>
      required(value, message: S.current.requiredUsername);

  static String? fullName(String? value) =>
      required(value, message: S.current.requiredFullName);
}
