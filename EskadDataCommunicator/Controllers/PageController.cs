using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MahtaKala.Controllers
{
    public class PageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Aboutus()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult ReturnGuarantee()
        {
            return View();
        }
        public IActionResult Delivery()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult GuaranteeOriginality()
        {
            return View();
        }

        public IActionResult ReturnCommodityRules()
        {
            return View();
        }
      

    }
}