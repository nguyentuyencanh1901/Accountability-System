/// Cấu hình kết nối API — tương đương `ApiClientOptions` trong Example.API.Client.
class ApiConfig {
  /// Base URL của UserService.API.
  ///
  /// - Android Emulator: dùng `http://10.0.2.2:5000` (10.0.2.2 = localhost máy host).
  /// - Thiết bị thật / iOS Simulator: dùng IP LAN máy chạy API, ví dụ `http://192.168.1.10:5000`.
  /// - WebApp dùng: `http://localhost:5000` trong appsettings.json.
  static const String baseUrl = 'http://10.0.2.2:5000';

  /// WebApp dùng ApiVersion = "100" (endpoint V100 cho thí sinh).
  static const String apiVersion = '100';

  /// Timeout mặc định cho mọi request HTTP.
  static const Duration connectTimeout = Duration(seconds: 30);
  static const Duration receiveTimeout = Duration(seconds: 30);
}
