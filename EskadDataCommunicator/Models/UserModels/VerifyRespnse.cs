using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.UserModels
{
    public class VerifyRespnse
    {
        /// <summary>
        /// Refresh token for JWT
        /// </summary>
        public string Refresh { get; set; }
        /// <summary>
        /// Access token for JWT
        /// </summary>
        public string Access { get; set; }
        /// <summary>
        /// User info
        /// </summary>
        public UserInfo User { get; set; }
    }
}
