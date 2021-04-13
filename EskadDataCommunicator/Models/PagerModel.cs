using MahtaKala.Entities.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models
{
    public class PagerModel : IPagingData
    {
        public int Offset { get; set; }
        public int Page { get; set; }
    }

    //public class SortOrder
    //{
    //    public string Field { get; set; }
    //    public bool Asc { get; set; } = true;
    //}
}
