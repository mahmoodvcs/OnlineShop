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
        public object Value { get; set; }
        public DataType Type { get; set; }
        public string Category { get; internal set; }
        public TypeCode TypeCode { get; internal set; }
    }
}