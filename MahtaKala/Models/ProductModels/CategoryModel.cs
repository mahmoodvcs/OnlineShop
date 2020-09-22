using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.ProductModels
{
    public class CategoryModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public long? ParentId { get; set; }
        public bool Disabled { get; set; }
        public bool Published { get; set; }
        public int Order { get; set; }
        public new ColorModel Color { get; set; }
        public class ColorModel
        {
            public int R { get; set; }
            public int G { get; set; }
            public int B { get; set; }
            public float A { get; set; }
        }
    }
}
