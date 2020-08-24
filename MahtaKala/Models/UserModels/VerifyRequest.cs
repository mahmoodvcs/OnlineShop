using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.UserModels
{
    public class VerifyRequest
    {
        public string Mobile { get; set; }
        /// <summary>
        /// OTP code
        /// </summary>
        public int Code { get; set; }
        public int Id { get; set; }
    }
}
