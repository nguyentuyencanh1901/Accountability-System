/// Xác định tab bottom nav theo route hiện tại.
int bottomNavIndexForLocation(String location) {
  if (location.startsWith('/profile')) return 4;
  if (location.startsWith('/exam-sets')) return 1;
  if (location.contains('/take')) return 2;
  if (location == '/exam-sessions/history') return 3;
  if (RegExp(r'^/exam-sessions/\d+$').hasMatch(location)) return 3;
  if (location.startsWith('/exam-sessions')) return 2;
  return 0;
}
