using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.StaffModels
{
    public class ProductChangeCategoryModel
    {
        public long[] ProductIds { get; set; }
        public long CategoryId { get; set; }
    }
}
