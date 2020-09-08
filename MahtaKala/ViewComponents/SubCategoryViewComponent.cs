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
        private DataContext _db;
        private readonly ProductService productService;
        private readonly CategoryService categoryService;

        public SubCategoryViewComponent(DataContext db, ProductService productService, CategoryService categoryService)
        {
            _db = db;
            this.productService = productService;
            this.categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync(long categoryId)
        {
            List<IdValModel> lst = new List<IdValModel>();
            var categories = await categoryService.Categories().Where(a => a.ParentId == categoryId).ToListAsync();
            foreach (var item in categories)
            {
                IdValModel vm = new IdValModel();
                vm.Title = item.Title;
                vm.Id = item.Id;
                lst.Add(vm);
            }
            return View(lst);
        }
    }
}
