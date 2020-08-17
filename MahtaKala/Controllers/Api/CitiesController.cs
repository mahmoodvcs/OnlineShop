using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    public class CitiesController : ApiControllerBase<CitiesController>
    {
        public CitiesController(DataContext context, ILogger<CitiesController> logger)
            : base(context, logger)
        {
        }
        [HttpGet]
        public async Task<List<City>> Index(long province_id)
        {
            return await db.Cities.Where(a => a.ProvinceId == province_id).ToListAsync();
        }
    }
}
