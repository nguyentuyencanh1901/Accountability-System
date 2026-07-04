import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../controllers/locale_controller.dart';
import '../controllers/auth_controller.dart';
import '../controllers/exam_session_controller.dart';
import '../controllers/exam_set_controller.dart';
import '../controllers/home_controller.dart';
import '../controllers/profile_controller.dart';
import '../l10n/app_strings.dart';
import '../core/network/api_client.dart';
import '../core/storage/auth_storage.dart';
import '../core/theme/app_theme.dart';
import '../repositories/app_user_repository.dart';
import '../repositories/auth_repository.dart';
import '../repositories/exam_period_assignment_repository.dart';
import '../repositories/exam_session_repository.dart';
import '../repositories/exam_set_repository.dart';
import '../routes/app_router.dart';
import '../services/auth_service.dart';
import '../services/exam_session_service.dart';
import '../services/exam_set_service.dart';
import '../services/home_service.dart';
import '../services/profile_service.dart';

/// Container DI — tương đương `ServicesRegister.cs` trong WebApp.
class AppDependencies {
  AppDependencies({
    required this.authController,
    required this.profileController,
    required this.homeController,
    required this.examSetController,
    required this.examSessionController,
    required this.takeExamController,
    required this.localeController,
    required this.router,
  });

  final AuthController authController;
  final ProfileController profileController;
  final HomeController homeController;
  final ExamSetController examSetController;
  final ExamSessionController examSessionController;
  final TakeExamController takeExamController;
  final LocaleController localeController;
  final GoRouter router;

  static Future<AppDependencies> create() async {
    final prefs = await SharedPreferences.getInstance();
    final storage = AuthStorage(prefs);

    AuthController? authControllerRef;
    final api = ApiClient(
      storage,
      onSessionExpired: () => authControllerRef?.logout(),
    );

    final authRepo = AuthRepository(api, storage);
    final appUserRepo = AppUserRepository(api);
    final assignmentRepo = ExamPeriodAssignmentRepository(api);
    final examSetRepo = ExamSetRepository(api);
    final sessionRepo = ExamSessionRepository(api);

    final authService = AuthService(authRepo, storage);
    final profileService = ProfileService(appUserRepo, authRepo, storage);
    final homeService = HomeService(assignmentRepo, sessionRepo);
    final examSetService =
        ExamSetService(examSetRepo, sessionRepo, assignmentRepo);
    final examSessionService =
        ExamSessionService(sessionRepo, examSetRepo, assignmentRepo);

    final authController = AuthController(authService, storage);
    authControllerRef = authController;
    final router = createAppRouter(authController);

    final localeController = LocaleController(prefs);

    return AppDependencies(
      authController: authController,
      profileController: ProfileController(profileService),
      homeController: HomeController(homeService),
      examSetController: ExamSetController(examSetService),
      examSessionController: ExamSessionController(examSessionService),
      takeExamController: TakeExamController(examSessionService),
      localeController: localeController,
      router: router,
    );
  }
}

/// Widget gốc ứng dụng Flutter.
class MobileApp extends StatelessWidget {
  const MobileApp({super.key, required this.deps});

  final AppDependencies deps;

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider<LocaleController>.value(
          value: deps.localeController,
        ),
        ChangeNotifierProvider<AuthController>.value(
          value: deps.authController,
        ),
        ChangeNotifierProvider<ProfileController>.value(
          value: deps.profileController,
        ),
        ChangeNotifierProvider<HomeController>.value(
          value: deps.homeController,
        ),
        ChangeNotifierProvider<ExamSetController>.value(
          value: deps.examSetController,
        ),
        ChangeNotifierProvider<ExamSessionController>.value(
          value: deps.examSessionController,
        ),
        ChangeNotifierProvider<TakeExamController>.value(
          value: deps.takeExamController,
        ),
      ],
      child: Consumer<LocaleController>(
        builder: (context, locale, _) {
          return MaterialApp.router(
            title: S.current.appName,
            debugShowCheckedModeBanner: false,
            theme: AppTheme.light,
            locale: locale.locale,
            supportedLocales: const [
              Locale('vi'),
              Locale('en'),
            ],
            localizationsDelegates: const [
              GlobalMaterialLocalizations.delegate,
              GlobalWidgetsLocalizations.delegate,
              GlobalCupertinoLocalizations.delegate,
            ],
            routerConfig: deps.router,
          );
        },
      ),
    );
  }
}
