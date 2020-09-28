using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MahtaKala.Models
{
    public class SettingModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
        public DataType Type { get; set; }
    }
}