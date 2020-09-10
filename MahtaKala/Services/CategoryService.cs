using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class CategoryService
    {
        private readonly DataContext db;

        public CategoryService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<Category> Categories()
        {
            return db.Categories.Where(a => a.Published).OrderByDescending(a => a.Disabled ? 0 : 1).ThenBy(a => a.Order);
        }
    }
}
