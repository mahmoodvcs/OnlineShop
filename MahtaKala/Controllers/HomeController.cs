using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Models.ProductModels;
using MahtaKala.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Z.EntityFramework.Plus;

namespace MahtaKala.Controllers
{
    public class HomeController : SiteControllerBase<HomeController>
    {
        private readonly IProductImageService productImageService;

        public HomeController(
            DataContext dataContext, 
            ILogger<HomeController> logger,
            IProductImageService productImageService) : base(dataContext, logger)
        {
            this.productImageService = productImageService;
        }

        public IActionResult Index()
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
                    Prices = p.Prices.ToList(),
                    Images = p.ImageList
                }).FirstOrDefault();
            if (product == null)
                throw new EntityNotFoundException<Product>(id);
            product.Thubmnail = productImageService.GetImageUrl(product.Id, product.Thubmnail);
            product.Images = productImageService.GetImageUrls(product.Id, product.Images).ToList();
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

        [HttpPost]
        public IActionResult RemoveFromWishlists(int id)
        {
            if (UserId != 0)
            {
                db.Wishlists.Where(a => a.Id==id).Delete();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        
        public IActionResult Category(int? id, string term)
        {
            var page = 1;
            int pageSize;
            int recordsPerPage = 12;
            int totalItemCount;
            var vm = Search(page: 1, recordsPerPage: recordsPerPage, groupId: id, term: term, pageSize: out pageSize, totalItemCount: out totalItemCount);
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.TotalItemCount = totalItemCount;
            return View(vm);
        }

        [HttpGet]
        public ActionResult Search(int page = 1, string term = "", int? groupId = null)
        {
            int pageSize;
            int recordsPerPage = 12;
            int totalItemCount;
            var users = Search(page: page, recordsPerPage: recordsPerPage, groupId: groupId, term: term, pageSize: out pageSize, totalItemCount: out totalItemCount);
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.TotalItemCount = totalItemCount;
            return PartialView("_ProductList", users);
        }

        [NonAction]
        private IEnumerable<Entities.Product> Search(int page, int recordsPerPage, int? groupId, string term, out int pageSize, out int totalItemCount)
        {
            var queryable = db.Products.Include(a => a.Prices).AsQueryable();
            if (!string.IsNullOrEmpty(term))
            {
                queryable = queryable.Where(c => c.Title.Contains(term));

            }
            if (groupId.HasValue)
            {
                queryable = queryable.Where(c => c.ProductCategories.Any(pc => pc.CategoryId == groupId));
            }

            totalItemCount = queryable.Count();
            pageSize = (int)Math.Ceiling((double)totalItemCount / recordsPerPage);

            page = page > pageSize || page < 1 ? 1 : page;

            var skiped = (page - 1) * recordsPerPage;
            queryable = queryable.Skip(skiped).Take(recordsPerPage);


            var data = queryable.ToList();
            productImageService.FixImageUrls(data);
            return data;
        }

    }

}
