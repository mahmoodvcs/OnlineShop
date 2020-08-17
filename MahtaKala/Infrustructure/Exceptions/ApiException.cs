using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.Exceptions
{
    public class ApiException : Exception
    {
        public ApiException(int statusCode, string message) : base(message)
        {
            this.StatusCode = statusCode;
        }
        public virtual int StatusCode { get; set; } = 500;

    }
}
