using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Example.Common.Filter
{
    /// <summary>
    /// Check header
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class CheckHeaderAttribute : Attribute, IResourceFilter
    {
        private readonly string[] _headers;

        public CheckHeaderAttribute(params string[] headers)
        {
            _headers = headers;
        }
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (_headers == null)
            {
                return;
            }

            if (!_headers.All(h => context.HttpContext.Request.Headers.ContainsKey(h)))
            {
                //throw new Exception("HTTP headers not contains!")

                context.Result = new ContentResult()
                {
                    Content = "HTTP headers not contains!",
                    StatusCode = HttpStatusCode.Unauthorized.GetHashCode()
                };
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            //todo
        }
    }
}
