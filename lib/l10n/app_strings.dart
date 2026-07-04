import 'package:flutter/widgets.dart';
import 'package:provider/provider.dart';

import '../controllers/locale_controller.dart';
import '../core/enums/app_enums.dart';

/// Chuỗi giao diện tĩnh — tiếng Việt / tiếng Anh.
class S {
  S._(this._lang);

  static String languageCode = 'vi';

  final String _lang;
  bool get _en => _lang == 'en';
  bool get isEnglish => _en;

  static S of(BuildContext context) {
    try {
      final code = context.watch<LocaleController>().languageCode;
      return S._(code);
    } catch (_) {
      return S._(languageCode);
    }
  }

  static S get current => S._(languageCode);

  // --- App ---
  String get appName => _en ? 'Assessment System' : 'Hệ thống đánh giá';

  // --- Nav ---
  String get navHome => _en ? 'Home' : 'Trang chủ';
  String get navExams => _en ? 'Exams' : 'Kỳ thi';
  String get navInProgress => _en ? 'In progress' : 'Đang làm';
  String get navHistory => _en ? 'History' : 'Lịch sử';
  String get navProfile => _en ? 'Profile' : 'Cá nhân';

  // --- Common ---
  String get cancel => _en ? 'Cancel' : 'Hủy';
  String get save => _en ? 'Save changes' : 'Lưu thay đổi';
  String get continueLabel => _en ? 'Continue' : 'Tiếp tục';
  String get logout => _en ? 'Log out' : 'Đăng xuất';
  String get logoutConfirmTitle => logout;
  String get logoutConfirmMessage =>
      _en ? 'Do you want to log out?' : 'Bạn có muốn đăng xuất không?';
  String get language => _en ? 'Language' : 'Ngôn ngữ';
  String get vietnamese => 'Tiếng Việt';
  String get english => 'English';
  String get loading => _en ? 'Loading...' : 'Đang tải...';
  String get understood => _en ? 'Got it' : 'Đã hiểu';
  String get previous => _en ? 'Previous' : 'Trước';
  String get next => _en ? 'Next' : 'Tiếp';
  String get all => _en ? 'All' : 'Tất cả';
  String get email => 'Email';
  String get phone => _en ? 'Phone number' : 'Số điện thoại';
  String get username => _en ? 'Username' : 'Tên đăng nhập';
  String get fullName => _en ? 'Full name *' : 'Họ và tên *';
  String get password => _en ? 'Password *' : 'Mật khẩu *';
  String get currentPassword => _en ? 'Current password *' : 'Mật khẩu hiện tại *';
  String get newPassword => _en ? 'New password *' : 'Mật khẩu mới *';
  String get confirmNewPassword =>
      _en ? 'Confirm new password *' : 'Xác nhận mật khẩu mới *';
  String get requiredNewPassword =>
      _en ? 'Please enter new password' : 'Vui lòng nhập mật khẩu mới';
  String get requiredConfirmNewPassword => _en
      ? 'Please confirm new password'
      : 'Vui lòng xác nhận mật khẩu mới';

  String allCount(int count) => '$all ($count)';
  String minutesQuestions(int minutes, int count) =>
      _en ? '$minutes min · $count questions' : '$minutes phút · $count câu';
  String questionsProgress(int answered, int total) =>
      _en ? '$answered/$total questions' : '$answered/$total câu';
  String startedAt(String time) =>
      _en ? 'Started: $time' : 'Bắt đầu: $time';
  String timeWindow(String start, String end) =>
      _en ? 'Window: $start - $end' : 'Khung giờ: $start - $end';
  String examTypeLine(String type) => _en ? 'Exam type: $type' : 'Loại thi: $type';
  String statusLine(String status) => _en ? 'Status: $status' : 'Trạng thái: $status';
  String durationLine(int minutes, int count) => _en
      ? 'Duration: $minutes min · $count questions'
      : 'Thời gian: $minutes phút · $count câu';
  String violations(int count) =>
      _en ? 'Violations: $count' : 'Vi phạm: $count lần';
  String violationBadge(int count) =>
      _en ? 'Violations $count' : 'Vi phạm $count';
  String questionNumber(int order) => _en ? 'Question $order' : 'Câu $order';
  String examSetFallback(int id) => _en ? 'Exam set #$id' : 'Bộ đề #$id';

  // --- Home ---
  String get homeTitle => navHome;
  String get hello => _en ? 'Hello' : 'Xin chào';
  String get homeSubtitle => _en
      ? 'Overview of your exams and progress.'
      : 'Tổng quan kỳ thi và tiến độ làm bài của bạn.';
  String get statAssigned => _en ? 'Assigned exams' : 'Kỳ thi được phân';
  String get statInProgress => _en ? 'In progress' : 'Đang làm dở';
  String get statCompleted => _en ? 'Completed' : 'Đã hoàn thành';
  String get startExamTitle => _en ? 'Start an exam' : 'Bắt đầu làm bài';
  String get startExamDesc => _en
      ? 'View assigned exams and start when the time comes.'
      : 'Xem danh sách kỳ thi được phân công và bắt đầu khi đến thời gian.';
  String get viewExams => _en ? 'View exams' : 'Xem kỳ thi';
  String get continueExamTitle => _en ? 'Continue exam' : 'Tiếp tục làm bài';
  String get continueExamDesc => _en
      ? 'Unsubmitted exams can be resumed from saved progress.'
      : 'Các bài thi chưa nộp, có thể tiếp tục từ phần đã lưu.';
  String get noInProgressExams =>
      _en ? 'No exams in progress' : 'Không có bài thi đang làm dở';
  String get historyTitle => _en ? 'Result history' : 'Lịch sử kết quả';
  String get historyDesc => _en
      ? 'Review scores and details of submitted exams.'
      : 'Xem lại điểm và chi tiết các bài đã nộp.';
  String get viewHistory => _en ? 'View history' : 'Xem lịch sử';
  String get noResults => _en ? 'No results yet' : 'Chưa có kết quả nào';

  // --- Exams ---
  String get examsTitle => navExams;
  String get noAssignedExams => _en
      ? 'No exams have been assigned yet.'
      : 'Chưa có kỳ thi nào được phân công.';
  String get startExam => _en ? 'Start exam' : 'Bắt đầu thi';
  String get continueExam => _en ? 'Continue exam' : 'Tiếp tục thi';
  String get viewDetails => _en ? 'View details' : 'Xem chi tiết';
  String get examDetails => _en ? 'Exam details' : 'Chi tiết kỳ thi';
  String get continueExamSession => _en ? 'Continue exam' : 'Tiếp tục bài thi';
  String get cannotStartExam => _en
      ? 'Cannot start or continue this exam right now.'
      : 'Hiện không thể bắt đầu hoặc tiếp tục bài thi.';

  // --- In progress list ---
  String get inProgressTitle => navInProgress;
  String get noInProgressSessions =>
      _en ? 'No exams in progress.' : 'Không có bài thi đang làm.';

  // --- History ---
  String get historyScreenTitle => navHistory;
  String get noHistory => _en ? 'No exam history yet.' : 'Chưa có lịch sử thi.';

  // --- Exam result ---
  String get examResult => _en ? 'Exam result' : 'Kết quả bài thi';
  String get questionDetails => _en ? 'Question details' : 'Chi tiết từng câu';
  String get correct => _en ? 'Correct' : 'Đúng';
  String get incorrect => _en ? 'Incorrect' : 'Sai';
  String get unanswered => _en ? 'Unanswered' : 'Chưa trả lời';
  String get totalLabel => _en ? 'Total' : 'Tổng';
  String get correctLabel => _en ? 'Correct' : 'Đúng';
  String get unansweredLabel => _en ? 'Unselected' : 'Chưa chọn';
  String get youSelected => _en ? 'Your answer' : 'Bạn chọn';
  String get correctAnswers => _en ? 'Correct answer(s)' : 'Đáp án đúng';
  String get notSelected => _en ? 'Not selected' : 'Chưa chọn';
  String get wrongSelected => _en ? 'Wrong selection' : 'Chọn sai';
  String get chooseMultiple => _en ? 'Multiple choice' : 'Chọn nhiều';
  String get chooseOne => _en ? 'Single choice' : 'Chọn một';

  // --- Take exam ---
  String get takeExam => _en ? 'Take exam' : 'Làm bài thi';
  String get questionList => _en ? 'Question list' : 'Danh sách câu hỏi';
  String get submitExam => _en ? 'Submit' : 'Nộp bài';
  String get submitConfirmTitle => submitExam;
  String get submitConfirmMessage =>
      _en ? 'Are you sure you want to submit?' : 'Bạn chắc chắn muốn nộp bài?';
  String get examRules => _en ? 'Exam rules' : 'Quy chế thi';
  String examRulesBody(int maxViolations) => _en
      ? 'Do not leave the app during the exam.\n'
          'More than $maxViolations violations → exam cancelled, no retake.'
      : 'Không rời ứng dụng khi đang thi.\n'
          'Vi phạm quá $maxViolations lần → hủy bài, không được thi lại.';
  String examRulesWarning(int maxViolations) => _en
      ? 'Do not leave the app during the exam. More than $maxViolations violations will cancel the exam.'
      : 'Không rời ứng dụng khi đang thi. Vi phạm quá $maxViolations lần sẽ hủy bài.';
  String unansweredWarning(int count) => _en
      ? '$count question(s) still unanswered.'
      : 'Còn thiếu $count câu chưa trả lời.';
  String get selected => _en ? 'selected' : 'đã chọn';
  String get legendInProgress => navInProgress;
  String get legendSelected => _en ? 'Selected ✓' : 'Đã chọn ✓';
  String get legendNotDone => _en ? 'Not done' : 'Chưa làm';
  String get legendInProgressLong => _en ? 'Navy = in progress' : 'Navy = đang làm';
  String get legendSelectedLong =>
      _en ? 'Green + ✓ = selected' : 'Xanh lá + ✓ = đã chọn';
  String get legendNotDoneLong => _en ? 'White = not done' : 'Trắng = chưa làm';
  String get questionWord => _en ? 'Q' : 'Câu';

  // --- Profile ---
  String get profileTitle => _en ? 'Personal info' : 'Thông tin cá nhân';
  String get editProfile => _en ? 'Edit profile' : 'Sửa thông tin';
  String get changePassword => _en ? 'Change password' : 'Đổi mật khẩu';
  String get updateSuccess =>
      _en ? 'Profile updated successfully.' : 'Cập nhật thông tin thành công.';
  String get changePasswordSuccess =>
      _en ? 'Password changed successfully.' : 'Đổi mật khẩu thành công.';

  // --- Auth ---
  String get login => _en ? 'Log in' : 'Đăng nhập';
  String get cannotStartExamError => _en
      ? 'Unable to start the exam.'
      : 'Không thể bắt đầu bài thi.';
  String get register => _en ? 'Sign up' : 'Đăng ký';
  String get registerTitle => register;
  String get noAccount => _en ? "Don't have an account? " : 'Chưa có tài khoản? ';
  String get hasAccount => _en ? 'Already have an account? ' : 'Đã có tài khoản? ';
  String get loginNow => _en ? 'Log in' : 'Đăng nhập ngay';

  // --- Validators ---
  String get requiredEmail => _en ? 'Please enter email' : 'Vui lòng nhập email';
  String get invalidEmail => _en ? 'Invalid email' : 'Email không hợp lệ';
  String get requiredPassword =>
      _en ? 'Please enter password' : 'Vui lòng nhập mật khẩu';
  String passwordMinLength(int min) => _en
      ? 'Password must be at least $min characters'
      : 'Mật khẩu tối thiểu $min ký tự';
  String get requiredConfirmPassword =>
      _en ? 'Please confirm password' : 'Vui lòng xác nhận mật khẩu';
  String get passwordMismatch =>
      _en ? 'Passwords do not match' : 'Mật khẩu xác nhận không khớp';
  String get requiredUsername =>
      _en ? 'Please enter username' : 'Vui lòng nhập tên đăng nhập';
  String get requiredFullName =>
      _en ? 'Please enter full name' : 'Vui lòng nhập họ tên';
  String get requiredCurrentPassword => _en
      ? 'Please enter current password'
      : 'Vui lòng nhập mật khẩu hiện tại';

  // --- Enum labels ---
  String examSessionStatus(int status) => switch (status) {
        ExamSessionStatus.inProgress => _en ? 'In progress' : 'Đang thi',
        ExamSessionStatus.completed => _en ? 'Completed' : 'Hoàn thành',
        ExamSessionStatus.expired => _en ? 'Expired' : 'Hết giờ',
        ExamSessionStatus.cancelled => _en ? 'Cancelled' : 'Bị hủy',
        _ => _en ? 'Unknown' : 'Không xác định',
      };

  String examPeriodAssignmentStatus(int status) => switch (status) {
        ExamPeriodAssignmentStatus.assigned =>
          _en ? 'Assigned' : 'Đã phân công',
        ExamPeriodAssignmentStatus.inProgress => _en ? 'In progress' : 'Đang thi',
        ExamPeriodAssignmentStatus.completed => _en ? 'Completed' : 'Hoàn thành',
        ExamPeriodAssignmentStatus.absent => _en ? 'Absent' : 'Vắng thi',
        ExamPeriodAssignmentStatus.cancelled =>
          _en ? 'Cancelled' : 'Bị hủy bỏ',
        _ => _en ? 'Unknown' : 'Không xác định',
      };

  String examType(int type) => switch (type) {
        ExamType.trial => _en ? 'Practice' : 'Thi thử',
        ExamType.real => _en ? 'Official' : 'Thi thật',
        _ => _en ? 'Unknown' : 'Không xác định',
      };

  String questionType(int type) => switch (type) {
        QuestionType.multipleChoice =>
          _en ? 'Multiple answers' : 'Chọn nhiều đáp án',
        QuestionType.singleChoice =>
          _en ? 'Single answer' : 'Chọn một đáp án',
        _ => _en ? 'Single answer' : 'Chọn một đáp án',
      };

  String questionTypeShort(int type) => switch (type) {
        QuestionType.multipleChoice => _en ? 'Multi' : 'Nhiều',
        QuestionType.singleChoice => _en ? 'One' : 'Một',
        _ => _en ? 'One' : 'Một',
      };

  String get examPeriodPublished => _en ? 'Published' : 'Đã công bố';
  String get examPeriodClosed => _en ? 'Closed' : 'Đã đóng';
  String get examPeriodEnded => _en ? 'Ended' : 'Kết thúc';
  String get examPeriodNotStarted =>
      _en ? 'Not yet open' : 'Chưa đến giờ thi';

  String displayStatusName(int assignmentStatus, ExamPeriodAvailability availability) {
    if (availability == ExamPeriodAvailability.ended) return examPeriodEnded;
    if (availability == ExamPeriodAvailability.notYetStarted) {
      return examPeriodNotStarted;
    }
    return examPeriodAssignmentStatus(assignmentStatus);
  }
}

extension SContext on BuildContext {
  S get s => S.of(this);
}
