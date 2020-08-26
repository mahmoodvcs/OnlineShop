using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MahtaKala.Controllers
{
    public class RepoController : Controller
    {
        private readonly DataContext db;
        public RepoController(DataContext context)
        {
            db = context;
        }

        public JsonResult GetCategories()
        {
            var p = db.Categories.Select(a=>new {  a.Id,a.Title }).ToList();
            return Json(p);
        }

        public JsonResult GetBrands()
        {
            var p = db.Brands.Select(a => new { a.Id, a.Name }).ToList();
            return Json(p);
        }
    }
}