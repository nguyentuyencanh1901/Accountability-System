namespace Example.Common.Utilities.Helper
{
    public class FileHelper
    {
        public static byte[] ReadFile(string filePath)
        {
            byte[] buffer;
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                int length = (int)fileStream.Length;  // get file length
                buffer = new byte[length];            // create buffer
                int count;                            // actual number of bytes read
                int sum = 0;                          // total number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading
            }
            finally
            {
                fileStream.Close();
            }
            return buffer;
        }

        public static string GetFileSSL()
        {
            string appDataPath = Path.Combine(AppContext.BaseDirectory, "SSL");
            string pemFilePath = Path.Combine(appDataPath, "client.truststore.example.pem");

            Console.WriteLine("pem Path: " + pemFilePath);
            return pemFilePath;
        }

        public static string KafkaSaveCaToFile(string content)
        {
            if(string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            string appDataPath = Path.Combine(AppContext.BaseDirectory, "SSL");
            string filePath = Path.Combine(appDataPath, "example-kafka.pem");

            try
            {
                // Lưu nội dung vào file
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error save file: {ex.Message}");
            }

            return filePath;
        }

        public static void DeleteFile(string filePath)
        {
            try
            {
                // Xóa tệp nếu tồn tại
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }
        }
    }
}
