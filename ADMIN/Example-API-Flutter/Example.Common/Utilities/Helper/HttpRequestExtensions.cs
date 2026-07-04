using Confluent.Kafka;
using Microsoft.AspNetCore.Http;

namespace Example.Common.Utilities.Helper
{
    public static class HttpRequestExtensions
    {
        public static string GetRawUrl(this HttpRequest request)
        {
            var httpContext = request.HttpContext;
            return $"{httpContext.Request.Path.Value}{httpContext.Request.QueryString}";
        }

        public static string GetDomain(this HttpRequest request)
        {
            var httpContext = request.HttpContext;
            return $"https://{httpContext.Request.Host}";
        }

        public static string GetFullPath(this HttpRequest request)
        {
            var httpContext = request.HttpContext;
            return $"https://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
        }

        public static string GetSubDomain(this HttpRequest request)
        {
            var subDomain = "";

            //var origin = request.Headers.Origin + "";
            //if (!string.IsNullOrWhiteSpace(origin) && origin.IndexOf(".example.io") != -1)
            //{
            //    origin = origin.Replace("https://", "").Replace("http://", "");
            //    var arr = origin.Split('.');
            //    if(arr.Length == 4)
            //    {
            //        //sub: vug.platform.example.io
            //        subDomain = origin.Split('.')[0];
            //    }
            //    else if(arr.Length == 3)
            //    {
            //        //platform.example.io
            //        subDomain = "platform";
            //    }
            //}

            //var referer = request.Headers.Referer + "";
            var origin = request.Headers.Origin + "";
            if (!string.IsNullOrWhiteSpace(origin))
            {
                origin = origin.Replace("https://", "").Replace("http://", "");
                //var arr = origin.Split('.');
                subDomain = origin.Split('.')[0];
            }

            return subDomain.Trim().ToLower();
        }

    }
}
