using MahtaKala.ActionFilter;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Controllers
{
    [ApiController()]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1")]
    public class HomeController : Controller
    {
        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            return StatusCode(200);
        }
    }
}
