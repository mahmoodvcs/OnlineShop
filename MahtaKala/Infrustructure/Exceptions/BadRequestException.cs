using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.Exceptions
{
    public class BadRequestException : ApiException
    {
        public BadRequestException(string message) : base(400, message)
        {
        }
    }
}
