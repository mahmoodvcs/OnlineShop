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

namespace MahtaKala.Controllers.Api
{
    [ApiController()]
    [Route("api/v{version:apiVersion}/provinces")]
    [ApiVersion("1")]
    [Authorize]
    public class ProvincesController : ApiControllerBase<ProvincesController>
    {
        public ProvincesController(DataContext context, ILogger<ProvincesController> logger)
            : base(context, logger)
        {
        }
        [HttpGet]
        public async Task<List<Province>> Index()
        {
            return await db.Provinces.ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] ProvinceModel provinceModel)
        {
            Province province = null;
            if (provinceModel.Id == 0)
            {
                province = new Province();
                db.Provinces.Add(province);
            }
            else
            {
                province = await db.Provinces.FirstOrDefaultAsync(c => c.Id == provinceModel.Id);
                if (province == null)
                    throw new Exception("Province Not Found.");
            }

            province.Name = provinceModel.Province;

            await db.SaveChangesAsync();
            return StatusCode(200);
        }
        [HttpDelete]
        public async Task<IActionResult> Index([FromBody] IdModel model)
        {
            var province = await db.Provinces.FirstOrDefaultAsync(c => c.Id == model.Id);
            if (province == null)
            {
                throw new Exception("Province Not Found.");
            }
            db.Provinces.Remove(province);
            await db.SaveChangesAsync();
            return StatusCode(200);
        }
    }
}
