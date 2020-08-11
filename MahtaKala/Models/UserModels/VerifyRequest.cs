using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.UserModels
{
    public class VerifyRequest
    {
        public string Mobile { get; set; }
        public int Code { get; set; }
    }
}
