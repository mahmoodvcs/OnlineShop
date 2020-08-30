using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Models.ProductModels;
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

        public IActionResult Category(int? id)
        {
            return View();
        }

        public IActionResult Product(int id)
        {
            var product = db.Products.Where(a => a.Id == id)
                .Select(p => new ProductHomeModel
                {
                    Id = p.Id,
                    Category = p.ProductCategories.FirstOrDefault().Category.Title,
                    CategoryId = p.ProductCategories.FirstOrDefault().CategoryId,
                    Brand = p.Brand.Name,
                    Description = p.Description,
                    Thubmnail = p.Thubmnail,
                    Title = p.Title,
                    Prices = p.Prices.ToList()
                }).FirstOrDefault();
            if (product == null)
                throw new EntityNotFoundException<Product>(id);
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
