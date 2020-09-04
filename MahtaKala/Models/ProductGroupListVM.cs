using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models
{
    public class ProductGroupListVM
    {
        public string GroupName { get; set; }
        public List<Product> Products { get; set; }
    }
}
