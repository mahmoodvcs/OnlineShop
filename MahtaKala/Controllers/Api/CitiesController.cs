using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Controllers
{
    [ApiController()]
    [Route("api/v{version:apiVersion}/cities")]
    [ApiVersion("1")]
    [Authorize]
    public class CitiesController : ControllerBase
    {
        private readonly DataContext db;

        public CitiesController(DataContext context)
        {
            db = context;
        }
        [HttpGet]
        public async Task<List<City>> Index(long province_id)
        {
            return await db.Cities.Where(a => a.ProvinceId == province_id).ToListAsync();
        }
    }
}
