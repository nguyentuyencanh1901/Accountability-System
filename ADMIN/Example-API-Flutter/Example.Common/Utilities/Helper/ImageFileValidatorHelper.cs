using Microsoft.AspNetCore.Http;

namespace Example.Common.Utilities.Helper
{
    public static class ImageFileValidatorHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".heif", ".heic", ".webp" };

        // Các content-type hợp lệ
        private static readonly string[] AllowedContentTypes = {
            "image/jpeg",
            "image/png",
            "image/heif",
            "image/heic",
            "image/webp"
        };

        /// <summary>
        /// Kiểm tra file ảnh hợp lệ dựa trên đuôi file và content-type
        /// </summary>
        /// <param name="files">Danh sách IFormFile cần kiểm tra</param>
        /// <returns>True nếu hợp lệ, ngược lại là False</returns>
        public static bool IsValidImage(List<IFormFile>? files)
        {
            if (files == null || !files.Any())
            {
                return true;
            }
            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                {
                    return false;
                }

                // Kiểm tra đuôi file
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    return false;
                }

                // Kiểm tra content type
                if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                {
                    return false;
                }

            }

            return true;
        }
    }
}
