using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Reflection;
using System.ComponentModel;
using System.Globalization;

namespace MahtaKala.Entities
{
    public class DynamicSetting
    {
        [Key]
        [StringLength(200)]
        [Required]
        [Display(Name = "نام")]
        public string Key { get; set; }

        [Display(Name = "عنوان فارسی")]
        [StringLength(256)]
        public string Title { get; set; }

        [Display(Name = "مقدار")]
        public string Vallue { get; set; }
    }
}