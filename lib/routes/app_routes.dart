/// Định nghĩa route paths — map 1-1 với controller/action WebApp.
class AppRoutes {
  static const login = '/login';
  static const register = '/register';
  static const home = '/';
  static const profile = '/profile';
  static const profileEdit = '/profile/edit';
  static const changePassword = '/profile/change-password';
  static const examSets = '/exam-sets';
  static const examSetDetail = '/exam-sets/:id';
  static const examInProgress = '/exam-sessions';
  static const examHistory = '/exam-sessions/history';
  static const examSessionDetail = '/exam-sessions/:id';
  static const takeExam = '/exam-sessions/:id/take';
}
