using Example.Common.Const;
using Microsoft.AspNetCore.Http;

namespace Example.Common.Utilities.Helper
{
    /// <summary>
    /// Helper lưu / xóa / hiển thị ảnh entity — dùng chung cho Question và các bảng khác sau này.
    /// Lưu file local theo cấu trúc: {module}/{entityId}/{fileName}
    /// </summary>
    public static class EntityImageHelper
    {
        public static class Modules
        {
            public const string Question = "questions";
        }

        public const string UploadRequestPath = "/uploads";
        public const long MaxFileSizeBytes = 5 * 1024 * 1024;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".heif", ".heic", ".webp" };

        public static string GetUploadRoot()
        {
            var configured = StaticVariable.CommonSetting?.MediaUploadPath;
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured.TrimEnd('/', '\\');
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        }

        public static (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return (true, null);
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, "Ảnh không được vượt quá 5MB");
            }

            if (!ImageFileValidatorHelper.IsValidImage(new List<IFormFile> { file }))
            {
                return (false, "File ảnh không hợp lệ (chỉ hỗ trợ JPG, PNG, HEIF, HEIC)");
            }

            return (true, null);
        }

        public static (bool IsValid, string? ErrorMessage) ValidateBase64(string? base64, string? fileName)
        {
            if (string.IsNullOrWhiteSpace(base64))
            {
                return (true, null);
            }

            var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                return (false, "Định dạng ảnh không được hỗ trợ");
            }

            try
            {
                var bytes = Convert.FromBase64String(StripDataUrlPrefix(base64));
                if (bytes.Length == 0)
                {
                    return (false, "Dữ liệu ảnh không hợp lệ");
                }

                if (bytes.Length > MaxFileSizeBytes)
                {
                    return (false, "Ảnh không được vượt quá 5MB");
                }
            }
            catch
            {
                return (false, "Dữ liệu ảnh không hợp lệ");
            }

            return (true, null);
        }

        /// <summary>Lưu ảnh mới từ IFormFile; xóa ảnh cũ nếu có.</summary>
        public static async Task<(string? RelativePath, string? ErrorMessage)> SaveAsync(
            IFormFile? file,
            string module,
            long entityId,
            string? currentRelativePath = null)
        {
            if (file == null || file.Length == 0)
            {
                return (currentRelativePath, null);
            }

            var validation = ValidateFile(file);
            if (!validation.IsValid)
            {
                return (null, validation.ErrorMessage);
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var relativePath = BuildRelativePath(module, entityId, $"{Guid.NewGuid():N}{extension}");
            var physicalPath = GetPhysicalPath(relativePath);

            EnsureDirectory(Path.GetDirectoryName(physicalPath)!);

            await using (var stream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(stream);
            }

            DeleteIfExists(currentRelativePath);
            return (relativePath, null);
        }

        /// <summary>Lưu ảnh từ base64 (WebAdmin gửi qua JSON API).</summary>
        public static async Task<(string? RelativePath, string? ErrorMessage)> SaveFromBase64Async(
            string? base64,
            string? fileName,
            string module,
            long entityId,
            string? currentRelativePath = null)
        {
            if (string.IsNullOrWhiteSpace(base64))
            {
                return (currentRelativePath, null);
            }

            var validation = ValidateBase64(base64, fileName);
            if (!validation.IsValid)
            {
                return (null, validation.ErrorMessage);
            }

            var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".jpg";
            }

            var bytes = Convert.FromBase64String(StripDataUrlPrefix(base64));
            var relativePath = BuildRelativePath(module, entityId, $"{Guid.NewGuid():N}{extension}");
            var physicalPath = GetPhysicalPath(relativePath);

            EnsureDirectory(Path.GetDirectoryName(physicalPath)!);
            await File.WriteAllBytesAsync(physicalPath, bytes);

            DeleteIfExists(currentRelativePath);
            return (relativePath, null);
        }

        /// <summary>Xử lý ảnh khi CUD: upload mới, giữ cũ hoặc xóa theo cờ RemoveImage.</summary>
        public static async Task<(string? RelativePath, string? ErrorMessage)> ProcessAsync(
            string? imageBase64,
            string? imageFileName,
            bool removeImage,
            string module,
            long entityId,
            string? currentRelativePath)
        {
            if (!string.IsNullOrWhiteSpace(imageBase64))
            {
                return await SaveFromBase64Async(imageBase64, imageFileName, module, entityId, currentRelativePath);
            }

            if (removeImage)
            {
                DeleteIfExists(currentRelativePath);
                return (null, null);
            }

            return (currentRelativePath, null);
        }

        public static void DeleteIfExists(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            var physicalPath = GetPhysicalPath(relativePath);
            FileHelper.DeleteFile(physicalPath);

            var directory = Path.GetDirectoryName(physicalPath);
            if (!string.IsNullOrEmpty(directory)
                && Directory.Exists(directory)
                && !Directory.EnumerateFileSystemEntries(directory).Any())
            {
                try
                {
                    Directory.Delete(directory);
                }
                catch
                {
                    // Bỏ qua nếu thư mục không xóa được
                }
            }
        }

        public static string GetPhysicalPath(string relativePath)
        {
            var normalized = relativePath.Replace('\\', '/').TrimStart('/');
            return Path.Combine(GetUploadRoot(), normalized.Replace('/', Path.DirectorySeparatorChar));
        }

        public static string BuildRelativePath(string module, long entityId, string fileName)
            => $"{module.Trim('/')}/{entityId}/{fileName}";

        public static string? BuildPublicUrl(string? relativePath, string? apiBaseUrl = null)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            var path = $"{UploadRequestPath}/{relativePath.TrimStart('/')}";
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                return $"{apiBaseUrl.TrimEnd('/')}{path}";
            }

            return path;
        }

        private static void EnsureDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string StripDataUrlPrefix(string base64)
        {
            var commaIndex = base64.IndexOf(',');
            return commaIndex >= 0 ? base64[(commaIndex + 1)..] : base64;
        }
    }
}
