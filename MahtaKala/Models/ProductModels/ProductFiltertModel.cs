using Kendo.Mvc.UI;
using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.ProductModels
{
    public class ProductFiltertModel
    {
        //[DataSourceRequest]
        //DataSourceRequest Request { get; set; }
        public int? StateFilter { get; set; }
        public string NameFilter { get; set; }
        public string CategoryFilter { get; set; }
        public string TagFilter { get; set; }

        public IQueryable<Product> Filter(IQueryable<Product> query)
        {
            if (!string.IsNullOrEmpty(NameFilter))
            {
                var parts = NameFilter.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in parts)
                {
                    var ss = s.Trim().ToLower();
                    query = query.Where(a => a.Title.ToLower().Contains(ss));
                }
            }
            if (CategoryFilter != null)
            {
                var parts = CategoryFilter.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in parts)
                {
                    var ss = s.Trim().ToLower();
                    query = query.Where(a => a.ProductCategories.Any(c => c.Category.Title.ToLower().Contains(ss)));
                }
            }
            if (TagFilter != null)
            {
                var parts = TagFilter.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in parts)
                {
                    var ss = s.Trim().ToLower();
                    query = query.Where(a => a.Tags.Any(c => c.Tag.Name.ToLower().Contains(ss)));
                }
            }
            if (StateFilter != null)
            {
                query = query.Where(a => a.Status == (ProductStatus)StateFilter);
            }

            return query;
        }

    }
}
