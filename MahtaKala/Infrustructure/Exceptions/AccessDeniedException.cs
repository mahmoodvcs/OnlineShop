using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.Exceptions
{
    public class AccessDeniedException : ApiException
    {
        public AccessDeniedException() : base(403, Messages.Messages.AccessDeined)
        {
        }
    }
}
