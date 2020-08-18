using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.Exceptions
{
    public class ApiErrorResponse
    {
        public int Code { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }

        public ApiErrorResponse(int code, string type, string message)
        {
            Code = code;
            Type = type;
            Message = message;
        }
        public ApiErrorResponse(ApiException ex)
        {
            Code = ex.StatusCode;
            Type = ex.GetType().Name;
            Message = ex.Message;
        }
        public ApiErrorResponse(Exception ex, int code = 500)
        {
            Code = code;
            Type = ex.GetType().Name;
            Message = ex.Message;
        }
    }
}
