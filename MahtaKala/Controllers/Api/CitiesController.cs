using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Models;
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

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] CityModel cityModel)
        {
            City city = null;
            if (cityModel.Id == 0)
            {
                city = new City();
                db.Cities.Add(city);
            }
            else
            {
                city = await db.Cities.FirstOrDefaultAsync(c => c.Id == cityModel.Id);
                if (city == null)
                    throw new EntityNotFoundException<City>(cityModel.Id);
            }

            city.Name = cityModel.City;
            city.ProvinceId = cityModel.Province_Id;

            await db.SaveChangesAsync();
            return StatusCode(200);
        }
        [HttpDelete]
        public async Task<IActionResult> Index([FromBody] IdModel model)
        {
            var city = await db.Cities.FirstOrDefaultAsync(c => c.Id == model.Id);
            if (city == null)
            {
                throw new EntityNotFoundException<City>(model.Id);
            }
            db.Cities.Remove(city);
            await db.SaveChangesAsync();
            return StatusCode(200);
        }
    }
}
