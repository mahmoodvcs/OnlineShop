using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.ProductModels
{
    public class CategoryWithProductsModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public long? ParentId { get; set; }
        public bool Disabled { get; set; }
        public CategoryModel.ColorModel Color { get; set; }

        public IList<CategoryWithProductsModel> Children { get; set; }
        public IList<ProductConciseModel> Products { get; set; }
    }
}
