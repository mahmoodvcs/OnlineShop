using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MahtaKala.Controllers
{
    public class HomeController : Controller
    {
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
