using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.CategoryModels
{
    public class UpdateCategoryRequest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public int? ParentId { get; set; }
    }
}
