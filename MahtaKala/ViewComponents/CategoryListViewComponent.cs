using MahtaKala.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.ViewComponents
{
    public class CategoryListViewComponent : ViewComponent
    {
        private DataContext  _db;
        public CategoryListViewComponent(DataContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(await _db.Categories.ToListAsync());
        }
    }
}
