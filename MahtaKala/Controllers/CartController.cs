using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MahtaKala.Controllers
{

    public class CartController : SiteControllerBase<CartController>
    {
        public CartController(DataContext dataContext, ILogger<CartController> logger) : base(dataContext, logger)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}