﻿using MahtaKala.Entities;
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
            var lstGroup = await categoryService.Categories().Where(a => a.ParentId == null && !a.Disabled).ToListAsync();
            foreach (var item in lstGroup)
            {
                List<long> lstg = new List<long>();
                lstg.Add(item.Id);
                lstg.AddRange(await categoryService.Categories().Where(a => a.ParentId == item.Id && !a.Disabled).Select(a => a.Id).ToListAsync());
                ProductGroupListVM vm = new ProductGroupListVM();
                vm.GroupName = item.Title;
                var lst = await productService.ProductsView(true, lstg.ToArray()).Take(10).ToListAsync();
                if (lst == null || lst.Count == 0)
                    continue;
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
