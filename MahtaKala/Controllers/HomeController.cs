using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MahtaKala.Controllers
{
    public class HomeController : SiteControllerBase<HomeController>
    {
        public HomeController(DataContext dataContext, ILogger<HomeController> logger) : base(dataContext, logger)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Category(int id)
        {
            return View();
        }

        public IActionResult Product(int id)
        {
            return View();
        }

    }
}
