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
        private readonly CategoryService categoryService;
        private readonly ICategoryImageService categoryImageService;

        public CategoryListViewComponent(CategoryService categoryService, ICategoryImageService categoryImageService)
        {
            this.categoryService = categoryService;
            this.categoryImageService = categoryImageService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var data = await categoryService.Categories().ToListAsync();
            categoryImageService.FixImageUrls(data);
            return View(data);
        }
    }
}
