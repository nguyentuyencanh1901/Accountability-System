using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Swagger
{
    public class RemoveHeaderParamsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var parametersToHide = new List<string> { "CustomerId" }; // Danh sách các tham số tiêu đề muốn loại bỏ

            //foreach (var parameter in operation.Parameters.ToList())
            //{
            //    if (parameter.In == ParameterLocation.Header && parametersToRemove.Contains(parameter.Name))
            //    {
            //        operation.Parameters.Remove(parameter);
            //    }
            //}

            foreach (var parameter in operation.Parameters.ToList())
            {
                if (parameter.In == ParameterLocation.Header && parametersToHide.Contains(parameter.Name))
                {
                    parameter.Extensions.Add("x-internal-hidden", new OpenApiBoolean(true)); // Chuyển đổi giá trị bool thành đối tượng IOpenApiExtension hợp lệ
                }
            }
        }
    }
}
