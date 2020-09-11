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
    public class HotDealsViewComponent : ViewComponent
    {
        private readonly IProductImageService imageService;
        private readonly ProductService productService;
        public HotDealsViewComponent(IProductImageService imageService,
            ProductService productService)
        {
            this.imageService = imageService;
            this.productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var lst = await productService.ProductsView().Include(a => a.Prices).Take(10).ToListAsync();
            foreach (var item in lst)
            {
                imageService.FixImageUrls(item);
            }
            return View(lst);
        }
    }
}
