using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.Exceptions
{
    public class ApiException : Exception
    {
        public ApiException(int statusCode, string message) : this(statusCode, message, null) { }
        public ApiException(int statusCode, Exception inner) : this(statusCode, inner.Message, inner) { }
        public ApiException(int statusCode, string message, Exception inner) : base(message, inner)
        {
            this.StatusCode = statusCode;
        }
        public virtual int StatusCode { get; set; } = 500;

    }
}
