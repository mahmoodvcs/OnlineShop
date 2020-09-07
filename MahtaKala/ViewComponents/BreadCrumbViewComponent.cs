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

    public class BreadCrumbViewComponent : ViewComponent
    {
        private DataContext _db;
        private readonly ProductService productService;
        private readonly CategoryService categoryService;

        public BreadCrumbViewComponent(DataContext db, ProductService productService, CategoryService categoryService)
        {
            _db = db;
            this.productService = productService;
            this.categoryService = categoryService;
        }

        private void GetGroup(long id, ref List<IdValModel> lst)
        {
            var p = categoryService.Categories().FirstOrDefaultAsync(a => a.Id == id).Result;
            if (p != null)
            {
                IdValModel v = new IdValModel();
                v.Id = p.Id;
                v.Title = p.Title;
                lst.Add(v);
                if (p.ParentId.HasValue)
                    GetGroup(p.ParentId.Value, ref lst);
            }
        }


        public async Task<IViewComponentResult> InvokeAsync(long categoryId)
        {
            List<IdValModel> lst = new List<IdValModel>();
            GetGroup(categoryId, ref lst);
            lst.Reverse();
            return View(lst);
        }
    }
}
