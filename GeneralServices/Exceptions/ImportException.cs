using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.GeneralServices.Exceptions
{
    public class ImportException : Exception
    {
        public ImportException(string msg) : base(msg) { }
    }
}
