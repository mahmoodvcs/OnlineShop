using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using MahtaKala.Entities;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Models;
using MahtaKala.Models.ProductModels;
using MahtaKala.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using P.Pager;
using Z.EntityFramework.Plus;

namespace MahtaKala.Controllers
{
    public class HomeController : SiteControllerBase<HomeController>
    {
        private readonly IProductImageService productImageService;
        private readonly ProductService productService;

        public HomeController(
            DataContext dataContext,
            ILogger<HomeController> logger,
            IProductImageService productImageService,
            ProductService productService,
            IHttpContextAccessor contextAccessor) : base(dataContext, logger)
        {
            this.productImageService = productImageService;
            this.productService = productService;
            this.contextAccessor = contextAccessor;
        }
        private readonly IHttpContextAccessor contextAccessor;
        public IActionResult Index()
        {
            ViewBag.EditRequired = false;
            if (User != null)
            {
                if (string.IsNullOrEmpty(User.FirstName) ||
                    string.IsNullOrEmpty(User.LastName) ||
                    string.IsNullOrEmpty(User.MobileNumber) ||
                    //string.IsNullOrEmpty(User.EmailAddress) ||
                    string.IsNullOrEmpty(User.NationalCode))
                {
                    ViewBag.EditRequired = true;
                }
				else
				{
                    int usersActiveAddresses = db.Addresses.Count(x => 
                        x.UserId == User.Id 
                        && !x.Disabled 
                        && !string.IsNullOrWhiteSpace(x.Details));
                    if (usersActiveAddresses == 0)
                        ViewBag.EditRequired = true;
				}
            }
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
                    Status = p.Status,
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
        public async Task<IActionResult> AddToWishlists(int id)
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
                int count = await db.Wishlists.Where(a => a.UserId == UserId).CountAsync();
                return Json(new { success = true, count });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult RemoveFromWishlists(int id)
        {
            if (UserId != 0)
            {
                db.Wishlists.Where(a => a.Id == id).Delete();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        public async Task<IActionResult> Compare(long firstId, long secondId)
        {
            List<long> ids = new List<long>
            {
                firstId, secondId
            };
            ProductCompareModel model = new ProductCompareModel
            {
                Products = await db.Products.Include(a => a.Prices).Where(a => ids.Contains(a.Id)).ToListAsync()
            };
            foreach (var product in model.Products)
            {
                productImageService.FixImageUrls(product);
            }

            return View(model);
        }

        //public IActionResult Category(long? id, string term)
        //{
        //    var page = 1;
        //    int pageSize;
        //    int recordsPerPage = 12;
        //    int totalItemCount;
        //    string txtSearch = "جستجو";
        //    if (!string.IsNullOrEmpty(term))
        //        txtSearch = "جستجو برای " + term;

        //    var isParentCategories = false;
        //    if (id.HasValue)
        //    {
        //        var c = db.Categories.FirstOrDefault(a => a.Id == id);
        //        if (c != null)
        //        {
        //            txtSearch = "جستجو در دسته بندی " + c.Title;
        //            isParentCategories = db.Categories.Any(a => a.ParentId == id);
        //        }
        //    }
        //    ViewData["Title"] = txtSearch;
        //    var vm = Search(page: 1, recordsPerPage: recordsPerPage, groupId: id, term: term, pageSize: out pageSize, totalItemCount: out totalItemCount);
        //    ViewBag.PageSize = pageSize;
        //    ViewBag.CurrentPage = page;
        //    ViewBag.TotalItemCount = totalItemCount;
        //    ViewBag.groupId = id;
        //    ViewBag.IsShowAlert = (isParentCategories == false && totalItemCount <= 0);
        //    return View(vm);
        //}


        //[HttpGet]
        //public ActionResult Search(int page = 1, string term = "", int? id = null)
        //{
        //    int pageSize;
        //    int recordsPerPage = 12;
        //    int totalItemCount;
        //    var users = Search(page: page, recordsPerPage: recordsPerPage, groupId: id, term: term, pageSize: out pageSize, totalItemCount: out totalItemCount);
        //    ViewBag.PageSize = pageSize;
        //    ViewBag.CurrentPage = page;
        //    ViewBag.TotalItemCount = totalItemCount;
        //    return PartialView("_ProductList", users);
        //}

        //[NonAction]
        //private IEnumerable<Entities.Product> Search(int page, int recordsPerPage, long? groupId, string term, out int pageSize, out int totalItemCount)
        //{
        //    var queryable = productService.ProductsView().Include(a => a.Prices).AsQueryable();
        //    if (!string.IsNullOrEmpty(term))
        //    {
        //        queryable = queryable.Where(c => c.Title.Contains(term));

        //    }
        //    if (groupId.HasValue)
        //    {
        //        queryable = queryable.Where(c => c.ProductCategories.Any(pc => pc.CategoryId == groupId));
        //    }

        //    totalItemCount = queryable.Count();
        //    pageSize = (int)Math.Ceiling((double)totalItemCount / recordsPerPage);

        //    page = page > pageSize || page < 1 ? 1 : page;

        //    var skiped = (page - 1) * recordsPerPage;
        //    queryable = queryable.Skip(skiped).Take(recordsPerPage);


        //    var data = queryable.ToList();
        //    productImageService.FixImageUrls(data);
        //    return data;
        //}



        public ActionResult Category(string term, long? id, int page = 1)
        {
            int totalItemCount;
            string txtSearch = "جستجو";
            if (!string.IsNullOrEmpty(term))
                txtSearch = "جستجو برای " + term;

            var isParentCategories = false;
            if (id.HasValue)
            {
                var c = db.Categories.FirstOrDefault(a => a.Id == id);
                if (c != null)
                {
                    txtSearch = "جستجو در دسته بندی " + c.Title;
                    isParentCategories = db.Categories.Any(a => a.ParentId == id);
                }
            }
            ViewData["Title"] = txtSearch;
            int pageSize = 12;
            ViewBag.groupId = id;
            ViewBag.term = term;
            var pager = GetProductSource(id, term, out totalItemCount).ToPagerList(page, pageSize);
            productImageService.FixImageUrls(pager);
            ViewBag.IsShowAlert = (isParentCategories == false && totalItemCount <= 0);
            return View(pager.AsEnumerable());
        }

        [NonAction]
        private IEnumerable<Product> GetProductSource(long? groupId, string term, out int totalItemCount)
        {
            var queryable = productService.ProductsView(true);
            if (!string.IsNullOrEmpty(term))
            {
                queryable = queryable.Where(c => c.Title.Contains(term));

            }
            if (groupId.HasValue)
            {
                queryable = queryable.Where(c => c.ProductCategories.Any(pc => pc.CategoryId == groupId));
            }
            totalItemCount = queryable.Count();
            return queryable;
        }

    }

}
