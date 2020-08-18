using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.ProductModels
{
    public class UpdateCategoryRequest
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public long? ParentId { get; set; }
    }
}
