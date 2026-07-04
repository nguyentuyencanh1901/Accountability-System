import 'package:go_router/go_router.dart';

import '../controllers/auth_controller.dart';
import '../views/auth/login_screen.dart';
import '../views/auth/register_screen.dart';
import '../views/exam_set/exam_set_detail_screen.dart';
import '../views/exam_set/exam_set_list_screen.dart';
import '../views/exam_session/exam_session_detail_screen.dart';
import '../views/exam_session/exam_session_history_screen.dart';
import '../views/exam_session/exam_session_list_screen.dart';
import '../views/exam_session/take_exam_screen.dart';
import '../views/home/home_screen.dart';
import '../views/profile/change_password_screen.dart';
import '../views/profile/edit_profile_screen.dart';
import '../views/profile/profile_screen.dart';
import 'app_routes.dart';

/// Cấu hình GoRouter — redirect theo trạng thái đăng nhập (giống `[Authorize]` WebApp).
GoRouter createAppRouter(AuthController authController) {
  return GoRouter(
    initialLocation: AppRoutes.login,
    refreshListenable: authController,
    redirect: (context, state) {
      final loggedIn = authController.isLoggedIn;
      final isAuthRoute = state.matchedLocation == AppRoutes.login ||
          state.matchedLocation == AppRoutes.register;

      if (!loggedIn && !isAuthRoute) return AppRoutes.login;
      if (loggedIn && isAuthRoute) return AppRoutes.home;
      return null;
    },
    routes: [
      GoRoute(
        path: AppRoutes.login,
        builder: (_, __) => const LoginScreen(),
      ),
      GoRoute(
        path: AppRoutes.register,
        builder: (_, __) => const RegisterScreen(),
      ),
      GoRoute(
        path: AppRoutes.home,
        builder: (_, __) => const HomeScreen(),
      ),
      GoRoute(
        path: AppRoutes.profile,
        builder: (_, __) => const ProfileScreen(),
      ),
      GoRoute(
        path: AppRoutes.profileEdit,
        builder: (_, __) => const EditProfileScreen(),
      ),
      GoRoute(
        path: AppRoutes.changePassword,
        builder: (_, __) => const ChangePasswordScreen(),
      ),
      GoRoute(
        path: AppRoutes.examSets,
        builder: (_, __) => const ExamSetListScreen(),
      ),
      GoRoute(
        path: AppRoutes.examSetDetail,
        builder: (context, state) {
          final id = int.parse(state.pathParameters['id']!);
          return ExamSetDetailScreen(assignmentId: id);
        },
      ),
      GoRoute(
        path: AppRoutes.examInProgress,
        builder: (_, __) => const ExamSessionListScreen(),
      ),
      GoRoute(
        path: AppRoutes.examHistory,
        builder: (_, __) => const ExamSessionHistoryScreen(),
      ),
      GoRoute(
        path: AppRoutes.examSessionDetail,
        builder: (context, state) {
          final id = int.parse(state.pathParameters['id']!);
          return ExamSessionDetailScreen(sessionId: id);
        },
      ),
      GoRoute(
        path: AppRoutes.takeExam,
        builder: (context, state) {
          final id = int.parse(state.pathParameters['id']!);
          return TakeExamScreen(sessionId: id);
        },
      ),
    ],
  );
}
