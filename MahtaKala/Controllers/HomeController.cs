﻿using System;
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

namespace MahtaKala.Controllers
{
    public class HomeController : SiteControllerBase<HomeController>
    {
        private readonly IProductImageService imageService;
        public HomeController(DataContext dataContext, ILogger<HomeController> logger,
            IProductImageService imageService) : base(dataContext, logger)
        {
            this.imageService = imageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Product(int id)
        {
            var p = db.Products.Include(a=> a.ProductCategories).Include(a=> a.Prices).Include(a=>a.Brand).FirstOrDefault(a => a.Id == id);
            if (p == null)
                throw new EntityNotFoundException<Product>(id);
            imageService.FixImageUrls(p);
            var product = new ProductHomeModel();
            product.Id = p.Id;
            product.Category = p.ProductCategories.FirstOrDefault().Category !=null? p.ProductCategories.FirstOrDefault().Category.Title:"";
            product.CategoryId = p.ProductCategories.FirstOrDefault() != null ? p.ProductCategories.FirstOrDefault().CategoryId:0;
            product.Brand = p.Brand.Name;
            product.Description = p.Description;
            product.Thubmnail = p.Thubmnail;
            product.Title = p.Title;
            product.Prices = p.Prices.ToList();
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
                // queryable = queryable.Where(c => c.CategoryId == groupId);
            }

            totalItemCount = queryable.Count();
            pageSize = (int)Math.Ceiling((double)totalItemCount / recordsPerPage);

            page = page > pageSize || page < 1 ? 1 : page;

            var skiped = (page - 1) * recordsPerPage;
            queryable = queryable.Skip(skiped).Take(recordsPerPage);
            foreach (var item in queryable)
            {
                imageService.FixImageUrls(item);
            }

            return queryable.ToList();
        }

    }

}
