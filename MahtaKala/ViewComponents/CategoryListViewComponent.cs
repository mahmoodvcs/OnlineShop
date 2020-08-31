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
    public class CategoryListViewComponent : ViewComponent
    {
        private DataContext  _db;
        private readonly ICategoryImageService categoryImageService;

        public CategoryListViewComponent(DataContext db, ICategoryImageService categoryImageService)
        {
            _db = db;
            this.categoryImageService = categoryImageService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var data = await _db.Categories.ToListAsync();
            categoryImageService.FixImageUrls(data);
            return View(data);
        }
    }
}
