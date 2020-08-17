using MahtaKala.ActionFilter;
using MahtaKala.Entities;
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
    }
}
