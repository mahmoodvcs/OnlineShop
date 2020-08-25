using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.SharedServices.Exceptions
{
    public class AppException : Exception
    {
        public AppException(string message) : base(message) { }
        public AppException(string message, Exception inner) : base(message, inner) { }
        public AppException(string message, string details) : base(message)
        {
            Details = details;
        }

        public string Details { get; }
    }
}
