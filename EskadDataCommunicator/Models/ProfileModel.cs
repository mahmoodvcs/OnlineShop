using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models
{
    public class ProfileModel
    {
        [StringLength(255)]
        public string Name { get; set; }
        [StringLength(255)]
        public string Family { get; set; }
        public string National_Code { get; set; }
        /// <summary>
        /// Not used
        /// </summary>
        public string Personel_Code { get; set; }
        [EmailAddress(ErrorMessage ="آدرس ایمیل معتبر نیست")]
        public string EMail { get; set; }
    }
}
