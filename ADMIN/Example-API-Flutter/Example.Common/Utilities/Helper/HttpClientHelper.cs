using Example.Common.Const;
using Example.Common.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Example.Common.Utilities.Helper
{
    public class HttpClientHelper
    {
        /// Tải về trang web và trả về chuỗi nội dung
        public static async Task<string> GetAsync(string requestUri)
        {
            // Khởi tạo http client
            //using var httpClient = new HttpClient();
            using (HttpClient httpClient = new HttpClient())
            {
                // Thiết lập các Header nếu cần
                //httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml+json");
                try
                {
                    // Thực hiện truy vấn GET
                    HttpResponseMessage response = await httpClient.GetAsync(requestUri);

                    // Hiện thị thông tin header trả về
                    //ShowHeaders(response.Headers);

                    // Phát sinh Exception nếu mã trạng thái trả về là lỗi
                    response.EnsureSuccessStatusCode();

                    Console.WriteLine($"Tải thành công - statusCode {(int)response.StatusCode} {response.ReasonPhrase}");

                    // Đọc nội dung content trả về - ĐỌC CHUỖI NỘI DUNG
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;

                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        public static async Task<string> SendAsync(string requestUri)
        {
            // Khởi tạo http client
            using (HttpClient httpClient = new HttpClient())
            {
                // Thiết lập các Header nếu cần
                //httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml+json");
                try
                {
                    var httpRequestMessage = new HttpRequestMessage();
                    httpRequestMessage.Method = HttpMethod.Post;
                    httpRequestMessage.RequestUri = new Uri(requestUri);

                    // Thực hiện Post
                    var response = await httpClient.SendAsync(httpRequestMessage);

                    // Phát sinh Exception nếu mã trạng thái trả về là lỗi
                    response.EnsureSuccessStatusCode();

                    Console.WriteLine($"Tải thành công - statusCode {(int)response.StatusCode} {response.ReasonPhrase}");

                    // Đọc nội dung content trả về - ĐỌC CHUỖI NỘI DUNG
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;

                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        /// In ra thông tin các Header của HTTP Response
        private static void ShowHeaders(HttpHeaders headers)
        {
            Console.WriteLine("CÁC HEADER:");
            foreach (var header in headers)
            {
                foreach (var value in header.Value)
                {
                    Console.WriteLine($"{header.Key,25} : {value}");

                }
            }
            Console.WriteLine();
        }

        /// Tải từ url, trả về stream để đọc dữ liệu (xem bài về stream)
        public static async Task DownloadDataStream(string requestUri, string filename)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(requestUri);
                    response.EnsureSuccessStatusCode();

                    // Lấy Stream để đọc content
                    using var stream = await response.Content.ReadAsStreamAsync();

                    // THỰC HIỆN ĐỌC Content
                    int SIZEBUFFER = 500;
                    using var streamwrite = File.OpenWrite(filename);  // Mở stream để lưu file
                    byte[] buffer = new byte[SIZEBUFFER];               // tạo bộ nhớ đệm lưu dữ liệu khi đọc stream

                    bool endread = false;
                    do                                                  // thực hiện đọc các byte từ stream và lưu ra streamwrite
                    {
                        int numberRead = await stream.ReadAsync(buffer, 0, SIZEBUFFER);
                        Console.WriteLine(numberRead);
                        if (numberRead == 0)
                        {
                            endread = true;
                        }
                        else
                        {
                            await streamwrite.WriteAsync(buffer, 0, numberRead);
                        }

                    } while (!endread);
                    Console.WriteLine("Download success");

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw ex;
                }
            }
        }

        public static async Task<string> PostAsJsonAsync(string requestUri, object value, Dictionary<string, string>? headers = null)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    if(headers != null)
                    {
                        httpClient.DefaultRequestHeaders.Clear();
                        foreach (var item in headers)
                        {
                            httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                    }

                    var httpClientJsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };

                    //Gửi yêu cầu POST đến URL của API
                    HttpResponseMessage response = await httpClient.PostAsJsonAsync(requestUri, value, httpClientJsonOptions);

                    // Kiểm tra xem yêu cầu có thành công không
                    if (response != null)
                    {
                        // Đọc nội dung phản hồi
                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            return responseContent;
                        }
                        else
                        {
                            // Handle non-successful response
                            throw new Exception($"Request failed with status code: {response.StatusCode}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return null;
        }

        public static async Task<string> PutAsJsonAsync(string requestUri, object value)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    var httpClientJsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };

                    //Gửi yêu cầu PUT đến URL của API
                    HttpResponseMessage response = await httpClient.PutAsJsonAsync(requestUri, value, httpClientJsonOptions);

                    // Kiểm tra xem yêu cầu có thành công không
                    if (response != null)
                    {
                        // Đọc nội dung phản hồi
                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            return responseContent;
                        }
                        else
                        {
                            // Handle non-successful response
                            throw new Exception($"Request failed with status code: {response.StatusCode}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return null;
        }


        //public static async Task<ResponseData<object>> SyncDataAI(SyncDataAIModel<object> model)
        //{
        //    try
        //    {
        //        var jsonSerializerOptions = new JsonSerializerOptions
        //        {
        //            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //            PropertyNameCaseInsensitive = true
        //        };
        //        string jsonData = JsonSerializer.Serialize(model, jsonSerializerOptions);
        //        string requestUri = $"{StaticVariable.apiAI}/crud_gateway";
        //        string responseContent = await HttpClientHelper.PostAsJsonAsync(requestUri, model);
        //        if (!string.IsNullOrEmpty(responseContent))
        //        {
        //            string content = $"SyncDataAI: {requestUri} ==> {jsonData} {Environment.NewLine} {responseContent}";
        //            Console.WriteLine(content);
        //            return new ResponseData<object>(true, content);
        //        }

        //        return new ResponseData<object>(false, $"SyncDataAI response null : {requestUri} ==> {jsonData}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"SyncDataAI: {ex.Message}");
        //        return new ResponseData<object>(false, ex.Message);
        //    }
        //}
    }
}
