using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class ProvincesController :ControllerBase
    {
        private readonly DataContext db;

        public ProvincesController(DataContext context)
        {
            db = context;
        }
        [HttpGet]
        public async Task<List<Province>> Index()
        {
            return await db.Provinces.ToListAsync();
        }
    }
}
