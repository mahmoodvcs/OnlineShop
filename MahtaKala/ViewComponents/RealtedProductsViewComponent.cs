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
        private readonly ProductService productService;
        private readonly IProductImageService imageService;
        public RealtedProductsViewComponent(ProductService productService, IProductImageService imageService)
        {
            this.productService = productService;
            this.imageService = imageService;
        }

        public async Task<IViewComponentResult> InvokeAsync(long id, long categoryId)
        {

            var lst = await productService.Products().Include(a => a.Prices).Where(a => a.Id != id && a.ProductCategories.Any(c => c.CategoryId == categoryId)).Take(10).ToListAsync();
            foreach (var item in lst)
            {
                imageService.FixImageUrls(item);
            }
            return View(lst);
        }
    }
}
