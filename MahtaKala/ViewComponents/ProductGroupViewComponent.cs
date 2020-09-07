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
    public class ProductGroupViewComponent : ViewComponent
    {
        private readonly IProductImageService imageService;
        private readonly ProductService productService;
        private readonly CategoryService categoryService;
        public ProductGroupViewComponent(IProductImageService imageService,
            ProductService productService,
            CategoryService categoryService)
        {
            this.imageService = imageService;
            this.productService = productService;
            this.categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<ProductGroupListVM> productGroupList = new List<ProductGroupListVM>();
            var lstGroup = await categoryService.Categories().Where(a => a.ParentId == null).ToListAsync();
            foreach (var item in lstGroup)
            {
                ProductGroupListVM vm = new ProductGroupListVM();
                vm.GroupName = item.Title;
                var lst = await productService.Products().Include(a => a.Prices).Where(c=> c.ProductCategories.Any(pc => pc.CategoryId == item.Id)).Take(10).ToListAsync();
                foreach (var itemx in lst)
                {
                    imageService.FixImageUrls(itemx);
                }
                vm.Products = lst;
                productGroupList.Add(vm);
            }
          
            return View(productGroupList);
        }
    }
}
