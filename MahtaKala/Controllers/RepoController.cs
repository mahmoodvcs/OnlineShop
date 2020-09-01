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
            var p = db.Categories.Select(a => new { a.Id, a.Title }).ToList();
            return Json(p);
        }

        public JsonResult GetBrands()
        {
            var p = db.Brands.Select(a => new { a.Id, a.Name }).ToList();
            return Json(p);
        }

        public JsonResult GetProvince()
        {
            return Json(db.Provinces.Select(a => new { a.Id, a.Name }).ToList());
        }

        public JsonResult GetCity(int? provinceId, string name)
        {
            var xp = db.Cities.Select(a => new { a.Id, a.Name, a.ProvinceId }).AsQueryable();
            if (provinceId != null)
                xp = xp.Where(p => p.ProvinceId == provinceId);
            int id;
            if (!string.IsNullOrEmpty(name) && !int.TryParse(name, out id))
            {
                xp = xp.Where(p => p.Name.Contains(name));
            }
            return Json(xp.ToList());
        }
    }
}