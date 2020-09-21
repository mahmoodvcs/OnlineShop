using MahtaKala.Entities;
using MahtaKala.Models;
using MahtaKala.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.ViewComponents
{
    public class SubCategoryViewComponent : ViewComponent
    {
        private readonly ProductService productService;
        private readonly CategoryService categoryService;
        private readonly ICategoryImageService categoryImageService;
        public SubCategoryViewComponent(DataContext db, ProductService productService, CategoryService categoryService, ICategoryImageService categoryImageService)
        {
            this.productService = productService;
            this.categoryService = categoryService;
            this.categoryImageService = categoryImageService;
        }

        public async Task<IViewComponentResult> InvokeAsync(long categoryId)
        {
            List<IdValModel> lst = new List<IdValModel>();
            var categories = await categoryService.Categories().Where(a => a.ParentId == categoryId).ToListAsync();
            categoryImageService.FixImageUrls(categories);
            return View(categories);
        }
    }
}
