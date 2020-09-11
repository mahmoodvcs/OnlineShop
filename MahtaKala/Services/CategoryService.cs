using EFSecondLevelCache.Core;
using MahtaKala.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class CategoryService
    {
        private readonly DataContext db;
        private readonly ICategoryImageService categoryImageService;

        public CategoryService(DataContext db, ICategoryImageService categoryImageService)
        {
            this.db = db;
            this.categoryImageService = categoryImageService;
        }

        public IQueryable<Category> Categories()
        {
            return db.Categories.Where(a => a.Published).OrderByDescending(a => a.Disabled ? 0 : 1).ThenBy(a => a.Order);
        }

        public async Task<List<Category>> AllCategories()
        {
            var list = await db.Categories.Cacheable().ToListAsync();
            List<Category> result = new List<Category>();
            CreateHierarchy(null, result, list.Where(c => c.ParentId == null).ToList());
            return result;
        }
        private void CreateHierarchy(long? parentId, IList<Category> result, IList<Category> categories)
        {
            if (categories == null)
                return;
            foreach (var c in categories)
            {
                var cp = new Category
                {
                    Id = c.Id,
                    Image = categoryImageService.GetImageUrl(c.Id, c.Image),
                    ParentId = parentId,
                    Title = c.Title,
                };
                result.Add(cp);
                cp.Children = new List<Category>();
                CreateHierarchy(c.Id, cp.Children, c.Children);
            }
        }

    }
}
