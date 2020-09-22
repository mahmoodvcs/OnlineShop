using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using MahtaKala.Entities;
using MahtaKala.Infrustructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MahtaKala.Controllers
{
    public class RepoController : MahtaControllerBase<RepoController>
    {
        public RepoController(DataContext context, ILogger<RepoController> logger)
            :base(context, logger)
        {
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

        public IActionResult Users([DataSourceRequest] DataSourceRequest req)
        {
            return KendoJson(db.Users.Where(u => u.Type == UserType.Seller).Select(u => new
            {
                Name = u.FirstName + " " + u.LastName + " (" + u.Username + ")",
                Id = u.Id
            }).ToDataSourceResult(req));
        }
        public IActionResult Sellers([DataSourceRequest] DataSourceRequest req)
        {
            return KendoJson(db.Sellers.Select(u => new
            {
                u.Id,
                u.Name
            }).ToList());
        }
    }
}