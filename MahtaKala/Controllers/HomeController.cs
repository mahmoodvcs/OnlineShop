﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var product = db.Products.Include(a=>a.Prices).FirstOrDefault(a=>a.Id==id);
            return View(product);
        }



        [HttpPost]
        public IActionResult AddToWishlists(int id)
        {
            if (UserId != 0)
            {
                var wishlists = db.Wishlists.Where(a => a.UserId == UserId && a.ProductId == id);
                if (!wishlists.Any())
                {
                    var p = new Wishlist();
                    p.UserId = UserId;
                    p.ProductId = id;
                    db.Wishlists.Add(p);
                    db.SaveChanges();
                }
                return Json(new { success = true });

            }
            return Json(new { success = false });
        }
    }
}
