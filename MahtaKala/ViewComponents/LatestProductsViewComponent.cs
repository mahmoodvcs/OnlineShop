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
    public class LatestProductsViewComponent : ViewComponent
    {
        private readonly ProductService productService;
        private readonly IProductImageService imageService;
        public LatestProductsViewComponent(ProductService productService, IProductImageService imageService)
        {
            this.productService = productService;
            this.imageService = imageService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var lst = await productService.ProductsView(true).Take(12).ToListAsync();
            foreach (var item in lst)
            {
                imageService.FixImageUrls(item);
            }
            return View(lst);
        }
    }
}
