import 'package:flutter/material.dart';
import 'app.dart';

/// Entry point — khởi tạo DI và chạy ứng dụng.
Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  final deps = await AppDependencies.create();
  runApp(MobileApp(deps: deps));
}
