using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Controllers.Staff
{
    [Authorize(UserType.Admin)]
    [Route("~/Staff/BuyLimitation")]
    public class BuyLimitationController : SiteControllerBase<BuyLimitationController>
    {
        public BuyLimitationController(DataContext dataContext, ILogger<BuyLimitationController> logger) : base(dataContext, logger)
        {
        }

        public ActionResult Index()
        {
            return View("~/Views/Staff/BuyLimitation/BuyLimitationList.cshtml");
        }

        [Route("GetAll")]
        public async Task<IActionResult> GetAll([DataSourceRequest] DataSourceRequest req)
        {
            return KendoJson(await db.BuyLimitations.Include(a => a.City).Include(a => a.Province).ToDataSourceResultAsync(req));
        }

        [Route("Update")]
        public async Task<ActionResult> Update(BuyLimitation b)
        {
            if (b.Id > 0)
            {
                db.BuyLimitations.Attach(b);
                db.Entry(b).State = EntityState.Modified;
            }
            else
            {
                db.BuyLimitations.Add(b);
            }
            await db.SaveChangesAsync();
            return Json(b);
        }

        [Route("Remove")]
        public async Task<ActionResult> Remove(BuyLimitation b)
        {
            db.BuyLimitations.Attach(b);
            db.BuyLimitations.Remove(b);
            await db.SaveChangesAsync();
            return Json(null);
        }

    }
}
