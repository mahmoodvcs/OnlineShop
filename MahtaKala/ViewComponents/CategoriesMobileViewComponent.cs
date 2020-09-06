using MahtaKala.Entities;
using MahtaKala.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.ViewComponents
{
    public class CategoriesMobileViewComponent : ViewComponent
    {
        private DataContext _db;
        private readonly ICategoryImageService categoryImageService;

        public CategoriesMobileViewComponent(DataContext db, ICategoryImageService categoryImageService)
        {
            _db = db;
            this.categoryImageService = categoryImageService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var data = await _db.Categories.ToListAsync();
            return View(data);
        }
    }
}
