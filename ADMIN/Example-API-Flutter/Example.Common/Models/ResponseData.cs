using Example.Common.Enums;
using Example.Common.Utilities;
using System.Net;

namespace Example.Common.Models
{
    public class ResponseData<T> where T : class
    {
        public ResponseData()
        {
            this.Success = false;
            this.Message = "";
            this.Data = null;
            this.StatusCode = 0;
        }
        public ResponseData(string exception)
        {
            this.Success = false;
            this.Message = exception;
            this.Data = null;
            this.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        public ResponseData(ErrorCodeAPI errorCodes)
        {
            Success = false;
            StatusCode = (int)errorCodes;
            Message = StringUtils.GetEnumDescription(errorCodes);
            Data = default(T);
        }

        public ResponseData(ErrorCodeAPI errorCodes, T data)
        {
            Success = false;
            StatusCode = (int)errorCodes;
            Message = StringUtils.GetEnumDescription(errorCodes);
            Data = data;
        }

        // successfully then respone data
        public ResponseData(bool isSuccess, T data = null, PagedMetaData metaData = null)
        {
            this.Success = isSuccess;
            this.Data = data;
            this.MetaData = metaData;
            this.StatusCode = (int)ErrorCodeAPI.OK;
            this.Message = StringUtils.GetEnumDescription(ErrorCodeAPI.OK);
        }

        public T? Data { get; set; }

        public PagedMetaData MetaData { get; set; }

        public string Message { get; set; }

        public bool Success { get; set; }

        public int StatusCode { get; set; }
    }
}
