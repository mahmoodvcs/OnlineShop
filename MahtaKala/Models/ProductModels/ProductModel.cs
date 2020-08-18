﻿using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.ProductModels
{
    public class ProductModel
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public long Category_Id { get; set; }
        public long Brand_Id { get; set; }
        public IList<Characteristic> Characteristics { get; set; }
        public IList<string> ImageList { get; set; }
        public string Thubmnail { get; set; }
    }
}