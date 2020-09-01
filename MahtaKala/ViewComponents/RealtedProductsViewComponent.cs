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
    public class RealtedProductsViewComponent : ViewComponent
    {
        private readonly IProductImageService imageService;
        private DataContext _db;
        public RealtedProductsViewComponent(DataContext db, IProductImageService imageService)
        {
            this.imageService = imageService;
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync(long id, long categoryId)
        {

            var lst = await _db.Products.Include(a => a.Prices).Where(a => a.Id != id && a.ProductCategories.Any(c => c.CategoryId == categoryId)).Take(10).ToListAsync();
            foreach (var item in lst)
            {
                imageService.FixImageUrls(item);
            }
            return View(lst);
        }
    }
}
