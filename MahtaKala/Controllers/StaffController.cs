using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using EFSecondLevelCache.Core;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.Entities.Security;
using MahtaKala.GeneralServices;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure;
using MahtaKala.Infrustructure.ActionFilter;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Infrustructure.Extensions;
using MahtaKala.Models;
using MahtaKala.Models.ProductModels;
using MahtaKala.Models.StaffModels;
using MahtaKala.Models.UserModels;
using MahtaKala.Services;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Writers;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Z.EntityFramework.Plus;

namespace MahtaKala.Controllers
{
    public class StaffController : SiteControllerBase<StaffController>
    {
        private readonly IProductImageService productImageService;
        private readonly ICategoryImageService categoryImageService;
        private readonly ImportService importService;
        private readonly CategoryService categoryService;
        private readonly IPathService pathService;
        private readonly ProductService productService;
        private readonly ISMSService smsService;

        public StaffController(
            DataContext context,
            ILogger<StaffController> logger,
            ISMSService smsService,
            IProductImageService productImageService,
            ICategoryImageService categoryImageService,
            ImportService importService,
            CategoryService categoryService,
            IPathService pathService,
            ProductService productService
            ) : base(context, logger)
        {
            this.productImageService = productImageService;
            this.categoryImageService = categoryImageService;
            this.importService = importService;
            this.categoryService = categoryService;
            this.pathService = pathService;
            this.productService = productService;
            this.smsService = smsService;
        }

        [Authorize(UserType.Staff, UserType.Admin, UserType.Delivery, UserType.Seller)]
        public async Task<IActionResult> Index()
        {
            if (base.User.Type == UserType.Delivery)
            {
                return RedirectToAction("BuyHistory");
            }
            if (base.User.Type == UserType.Seller)
            {
                return Redirect("~/Staff/Orders/Items");
            }
            var report = new ReportModel();
            var user = base.User;
            var orders = db.Orders.Where(o => o.State == OrderState.Paid ||
                                                  o.State == OrderState.Delivered ||
                                                  o.State == OrderState.Sent)
                        .Where(a => a.CheckOutDate != null);
            var orderChart = orders.OrderBy(o => o.CheckOutDate)
                .GroupBy(o => o.CheckOutDate.Value.Date)
                .Select(o => new
                {
                    Date = o.Key,
                    Value = o.Count()
                })
                .AsEnumerable()
                .Select(o => new ChartModel
                {
                    Date = Util.GetPersianDate(o.Date),
                    Value = o.Value
                }).ToList();
            var saleChart = orders.OrderBy(o => o.CheckOutDate)
                .GroupBy(o => o.CheckOutDate.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Value = g.Sum(o => o.TotalPrice)
                })
                .AsEnumerable()
                .Select(g => new ChartModel
                {
                    Date = Util.GetPersianDate(g.Date),
                    Value = g.Value
                }).ToList();
            if (user.Type == UserType.Admin)
            {
                report.TotalOrders = await orders.CountAsync();
                report.TotalPayments = await orders.Select(o => o.TotalPrice).SumAsync();
                report.TotalProducts = await db.Products.CountAsync();
                report.TotalUsers = await db.Users.CountAsync();
                report.OrderChart = orderChart;
                report.SaleChart = saleChart;
            }
            else
            {
                report.TotalOrders = await orders.CountAsync();
                report.TotalPayments = await orders.Select(o => o.TotalPrice).SumAsync();
                report.TotalProducts = await db.Products.Where(p => p.SellerId == user.Id).CountAsync();
                report.TotalUsers = await db.Users.CountAsync();
                report.OrderChart = orderChart;
                report.SaleChart = saleChart;
            }
            return View(report);
        }

        async Task<long> GetSellerId()
        {
            return await db.Sellers.Where(a => a.UserId == UserId).Select(a => a.Id).FirstOrDefaultAsync();
        }

        #region Users

        [Authorize(UserType.Admin)]
        public IActionResult UserList()
        {
            return View();
        }
        [Authorize(UserType.Admin)]
        public ActionResult GetAllUsers([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Users, request);
        }
        [Authorize(UserType.Admin)]
        public new ActionResult User(long id)
        {
            User user = null;
            if (id == 0)
            {
                user = new User();
            }
            else
            {
                user = db.Users.Where(u => u.Id == id).FirstOrDefault();
                if (user == null)
                {
                    throw new EntityNotFoundException<User>(id);
                }
            }
            if (user.Type == UserType.Seller)
                ViewBag.sellerID = db.Sellers.Where(a => a.UserId == user.Id).Select(a => a.Id).FirstOrDefault();
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(UserType.Admin)]
        public new IActionResult User(User model, string sellerId)
        {
            if (ModelState.IsValid)
            {
                Seller seller = null;
                if (model.Type == UserType.Seller)
                {
                    seller = db.Sellers.Find(long.Parse(sellerId));
                }

                if (model.Id == 0)
                {
                    if (string.IsNullOrEmpty(model.Password))
                    {
                        ModelState.AddModelError(nameof(model.Password), "Password Required.");
                        return View(model);
                    }
                    db.Users.Add(model);
                    if (seller != null)
                        seller.User = model;
                    model.Password = PasswordHasher.Hash(model.Password, ((int)model.Type).ToString());
                }
                else
                {
                    var user = db.Users.Where(u => u.Id == model.Id).FirstOrDefault();
                    if (user == null)
                    {
                        throw new EntityNotFoundException<User>(model.Id);
                    }
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Username = model.Username;
                    user.EmailAddress = model.EmailAddress;
                    user.MobileNumber = model.MobileNumber;
                    user.NationalCode = model.NationalCode;
                    user.Type = model.Type;
                    if (string.IsNullOrEmpty(model.Password))
                    {
                        user.Password = user.Password;
                    }
                    else
                    {
                        user.Password = PasswordHasher.Hash(model.Password, ((int)model.Type).ToString());
                    }
                    db.Entry(user).State = EntityState.Modified;
                    if (seller != null)
                        seller.User = user;
                }

                db.SaveChanges();
                return RedirectToAction("UserList");
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(UserType.Admin)]
        public async Task<JsonResult> UserDestroy(long id)
        {
            if (await db.Products.AnyAsync(p => p.SellerId == id))
            {

                return Json(new { Success = false, msg = "کاربر دارای کالا می باشد." });
            }
            else
            {
                await db.Users.Where(u => u.Id == id).DeleteAsync();
                await db.SaveChangesAsync();
            }
            return Json(new { Success = true });
        }

        #endregion


        #region Provinces

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public IActionResult ProvinceList()
        {
            return View();
        }
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public ActionResult GetAllProvinces([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Provinces.OrderBy(x => x.Name), request);
        }
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public ActionResult Province(long id)
        {
            Province province = null;
            if (id == 0)
            {
                province = new Province();
            }
            else
            {
                province = db.Provinces.Where(u => u.Id == id).FirstOrDefault();
                if (province == null)
                {
                    throw new EntityNotFoundException<Province>(id);
                }
            }
            return View(province);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public IActionResult Province(Province model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    if (string.IsNullOrEmpty(model.Name))
                    {
                        ModelState.AddModelError(nameof(model.Name), "Name Required.");
                        return View(model);
                    }
                    db.Provinces.Add(model);
                }
                else
                {
                    if (!db.Provinces.Any(u => u.Id == model.Id))
                    {
                        throw new EntityNotFoundException<Province>(model.Id);
                    }
                    db.Entry(model).State = EntityState.Modified;
                }
                db.SaveChanges();
                return RedirectToAction("ProvinceList");
            }
            return View(model);
        }

        #endregion


        #region City

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public IActionResult CityList()
        {
            return View();
        }
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public ActionResult GetAllCities([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Cities.OrderBy(x => x.Province.Name).ThenBy(x => x.Name).Include(c => c.Province), request);
        }
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public ActionResult City(long id)
        {
            City city = null;
            if (id == 0)
            {
                city = new City();
            }
            else
            {
                city = db.Cities.Where(u => u.Id == id).FirstOrDefault();
                if (city == null)
                {
                    throw new EntityNotFoundException<City>(id);
                }
            }
            ViewBag.Provinces = db.Provinces.OrderBy(x => x.Name).ToList();
            return View(city);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public IActionResult City(City model)
        {
            ViewBag.Provinces = db.Provinces.OrderBy(x => x.Name).ToList();
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    if (string.IsNullOrEmpty(model.Name))
                    {
                        ModelState.AddModelError(nameof(model.Name), "Name Required.");
                        return View(model);
                    }
                    db.Cities.Add(model);
                }
                else
                {
                    if (!db.Cities.Any(u => u.Id == model.Id))
                    {
                        throw new EntityNotFoundException<City>(model.Id);
                    }
                    db.Entry(model).State = EntityState.Modified;
                }
                if (model.IsCenter)
                {
                    if (db.Cities.Any(c => c.IsCenter && c.Id != model.Id))
                    {
                        throw new BadRequestException("استان انتخاب شده دارای مرکز استان می باشد.");
                    }
                }
                db.SaveChanges();
                return RedirectToAction("CityList");
            }
            return View(model);
        }

        #endregion


        #region Category

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public IActionResult CategoryList()
        {
            return View();
        }

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public IActionResult Categories_List()
        {
            var query = db.Categories.OrderByDescending(c => c.ParentId).ThenBy(a => a.Order).ThenBy(x => x.Id).AsQueryable();
            return KendoJson(query);
        }


        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public IActionResult GetAllCategories([DataSourceRequest] DataSourceRequest request,
            string nameFilter,
            string categoryFilter,
            string disabledFilter,
            string publishedFilter)
        {
            var query = db.Categories.Include(c => c.Parent).OrderByDescending(c => c.ParentId)
                .ThenBy(a => a.Order).ThenBy(x => x.Id).AsQueryable();
            if (!string.IsNullOrEmpty(nameFilter))
            {
                var parts = nameFilter.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in parts)
                {
                    var ss = s.Trim().ToLower();
                    query = query.Where(a => a.Title.ToLower().Contains(ss));
                }
            }
            if (!string.IsNullOrEmpty(categoryFilter))
            {
                var parts = categoryFilter.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in parts)
                {
                    var ss = s.Trim().ToLower();
                    query = query.Where(a => a.Parent.Title.ToLower().Contains(ss));
                }
            }
            if (!string.IsNullOrWhiteSpace(disabledFilter)
                && bool.TryParse(disabledFilter, out bool disabledFilterBoolean))
            // In case "all" is the value of disabledFilter, the function call bool.TryParse in the IF statement above
            // will return false, and this block would be omitted, which is the correct action for the "all" filter option!
            {
                query = query.Where(a => a.Disabled == disabledFilterBoolean);
            }
            if (!string.IsNullOrWhiteSpace(publishedFilter)
                && bool.TryParse(publishedFilter, out bool publishedFilterBoolean))
            // In case "all" is the value of publishedFilter, the function call bool.TryParse in the IF statement above
            // will return false, and this block would be omitted, which is the correct action for the "all" filter option!
            {
                query = query.Where(a => a.Published == publishedFilterBoolean);
            }

            return KendoJson(query.ToDataSourceResult(request));
        }

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public ActionResult Category(long id)
        {
            Category productCategory = null;
            if (id == 0)
            {
                productCategory = new Category();
            }
            else
            {
                productCategory = db.Categories.Where(u => u.Id == id).FirstOrDefault();
                if (productCategory == null)
                {
                    throw new EntityNotFoundException<Category>(id);
                }
                categoryImageService.FixImageUrls(productCategory);
            }
            ViewBag.Categories = db.Categories.Where(c => c.Id != id).ToList();
            return View(productCategory);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public async Task<IActionResult> Category(Category model)
        {
            ViewBag.Categories = db.Categories.Where(c => c.Id != model.Id).ToList();
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    var index = await db.Categories.Where(c => c.ParentId == model.ParentId)
                        .OrderByDescending(c => c.Order)
                        .Select(c => c.Order)
                        .FirstOrDefaultAsync();
                    model.Order = index == 0 ? index : index + 1;
                    db.Categories.Add(model);
                }
                else
                {
                    var cat = await db.Categories.FindAsync(model.Id);
                    if (cat == null)
                    {
                        throw new EntityNotFoundException<Category>(model.Id);
                    }
                    cat.Order = model.Order;
                    cat.Title = model.Title;
                    cat.Disabled = model.Disabled;
                    cat.Published = model.Published;
                    cat.ParentId = model.ParentId;
                    cat.Color = model.Color;
                }
                await db.SaveChangesAsync();
                ViewBag.IsPostback = true;
                return View(model);
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        [AjaxAction]
        public async Task<ActionResult> SaveCategoryImage(IEnumerable<IFormFile> images, long ID)
        {
            // The Name of the Upload component is "images"
            if (images != null)
            {
                if (images.Count() > 0)
                {
                    var category = await db.Categories.Where(p => p.Id == ID).FirstOrDefaultAsync();
                    if (category == null)
                    {
                        throw new EntityNotFoundException<Product>(ID);
                    }
                    var file = images.First();
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    using (var stream = file.OpenReadStream())
                    {
                        await categoryImageService.SaveImage(ID, fileName, stream);
                    }
                    category.Image = fileName;
                    db.Entry(category).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return Json(categoryImageService.GetImageUrl(ID, category.Image));
                }
            }
            // Return an empty string to signify success
            return Content("");
        }

        [HttpPost]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public async Task<JsonResult> Category_Destroy(long id)
        {
            if (db.Categories.Any(c => c.ParentId == id))
            {
                var category = await db.Categories.FindAsync(id);
                category.Published = false;
                await db.SaveChangesAsync();
                return Json(new { Success = false, Message = "دسته بندی دارای فرزند است. از حالت انتشار خارج میشود" });
            }
            else
            {
                db.Categories.Where(c => c.Id == id).Delete();
                categoryImageService.DeleteImages(id);
                return Json(new { Success = true });
            }
        }

        [HttpPost]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public async Task<JsonResult> Category_Up(long id)
        {
            var category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return Json(new { Success = false, Message = "دسته بندی یافت نشد." });
            }
            var categories = await db.Categories.Where(c => c.ParentId == category.ParentId).OrderBy(c => c.Order).ToListAsync();
            for (int i = 0; i < categories.Count; i++)
            {
                categories[i].Order = i;
            }
            var index = categories.FindIndex(c => c.Id == id);
            if (index > 0)
            {
                categories[index].Order--;
                categories[index - 1].Order++;
            }
            await db.SaveChangesAsync();
            return Json(new { Success = true });
        }

        [HttpPost]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public async Task<JsonResult> Category_Down(long id)
        {
            var category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return Json(new { Success = false, Message = "دسته بندی یافت نشد." });
            }
            var categories = await db.Categories.Where(c => c.ParentId == category.ParentId).OrderBy(c => c.Order).ToListAsync();
            for (int i = 0; i < categories.Count; i++)
            {
                categories[i].Order = i;
            }
            var index = categories.FindIndex(c => c.Id == id);
            if (index < categories.Count - 1)
            {
                categories[index].Order++;
                categories[index + 1].Order--;
            }
            await db.SaveChangesAsync();
            return Json(new { Success = true });
        }


        #endregion


        #region Brand

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public IActionResult BrandList()
        {
            return View();
        }
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public ActionResult GetAllBrands([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Brands, request);
        }
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public ActionResult Brand(long id)
        {
            Brand brand = null;
            if (id == 0)
            {
                brand = new Brand();
            }
            else
            {
                brand = db.Brands.Where(u => u.Id == id).FirstOrDefault();
                if (brand == null)
                {
                    throw new EntityNotFoundException<Brand>(id);
                }
            }
            return View(brand);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public IActionResult Brand(Brand model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError(nameof(model.Name), "نام را وارد کنید.");
                    return View(model);
                }
                if (model.Id == 0)
                {
                    db.Brands.Add(model);
                }
                else
                {
                    if (!db.Brands.Any(u => u.Id == model.Id))
                    {
                        throw new EntityNotFoundException<Brand>(model.Id);
                    }
                    db.Entry(model).State = EntityState.Modified;
                }
                db.SaveChanges();
                return RedirectToAction("BrandList");
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public JsonResult Brand_Destroy(long id)
        {
            if (db.Products.Any(c => c.BrandId == id))
            {
                return Json(new { Success = false, Message = "امکان حذف برند دارای کالا نمی باشد." });
            }
            else
            {
                db.Brands.Where(c => c.Id == id).Delete();
                return Json(new { Success = true });
            }
        }

        #endregion

        #region Supplier

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public IActionResult SupplierList()
        {
            return View();
        }
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public ActionResult GetAllSuppliers([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Suppliers, request);
        }
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public ActionResult Supplier(long id)
        {
            Supplier su = null;
            if (id == 0)
            {
                su = new Entities.Supplier();
            }
            else
            {
                su = db.Suppliers.Where(u => u.Id == id).FirstOrDefault();
                if (su == null)
                {
                    throw new EntityNotFoundException<Brand>(id);
                }
            }
            return View(su);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public IActionResult Supplier(Supplier model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    ModelState.AddModelError(nameof(model.Name), "نام را وارد کنید.");
                    return View(model);
                }
                if (model.Id == 0)
                {
                    db.Suppliers.Add(model);
                }
                else
                {
                    if (!db.Suppliers.Any(u => u.Id == model.Id))
                    {
                        throw new EntityNotFoundException<Supplier>(model.Id);
                    }
                    db.Entry(model).State = EntityState.Modified;
                }
                db.SaveChanges();
                return RedirectToAction("SupplierList");
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public JsonResult Supplier_Destroy(long id)
        {
            if (db.Products.Any(c => c.SupplierId == id))
            {
                return Json(new { Success = false, Message = "این تامین کننده در کالا استفاده شده است." });
            }
            else
            {
                db.Suppliers.Where(c => c.Id == id).Delete();
                return Json(new { Success = true });
            }
        }

        #endregion


        #region Product

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Seller })]
        public IActionResult ProductList()
        {
            ViewData["Title"] = "لیست کالا و خدمات";
            return View();
        }

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Seller })]
        public async Task<IActionResult> Product_Read(
            [DataSourceRequest] DataSourceRequest request,
            int? stateFilter,
            string nameFilter,
            string categoryFilter,
            string tagFilter,
            string idFilter,
            string productCodeFilter,
            string sellerNameFilter,
            string brandNameFilter,
            string supplierNameFilter,
            int? maxPriceFilter,
            int? minPriceFilter,
            int? maxDiscountedPriceFilter,
            int? minDiscountedPriceFilter,
            string isPublishedFilter)
        {
            var query = db.Products.OrderBy(x => x.Id).AsQueryable();
            if (base.User.Type == UserType.Seller)
            {
                var sid = await GetSellerId();
                query = query.Where(p => p.SellerId == sid).AsQueryable();
            }
            //FlexTextFilter<Product>(query, p => p.Title, nameFilter);
            if (!string.IsNullOrWhiteSpace(idFilter))
            {
                idFilter = idFilter.Trim();
                //if (idFilter.ContainsOnlyDigits())
                query = query.Where(x => x.Id.ToString().Contains(idFilter));
            }
            if (!string.IsNullOrWhiteSpace(nameFilter))
            {
                var parts = nameFilter.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in parts)
                {
                    var ss = s.Trim().ToLower();
                    query = query.Where(a => a.Title.ToLower().Contains(ss));
                }
            }
            if (!string.IsNullOrWhiteSpace(productCodeFilter))
            {
                productCodeFilter = productCodeFilter.Trim();
                query = query.Where(x => x.Code.Contains(productCodeFilter));
            }
            // if (categoryFilter != null)
            if (!string.IsNullOrWhiteSpace(categoryFilter))
            {
                categoryFilter = categoryFilter.Trim();
                var parts = categoryFilter.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in parts)
                {
                    var ss = s.Trim().ToLower();
                    query = query.Where(a => a.ProductCategories.Any(c => c.Category.Title.ToLower().Contains(ss)));
                }
            }
            //if (tagFilter != null)
            if (!string.IsNullOrWhiteSpace(tagFilter))
            {
                var parts = tagFilter.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in parts)
                {
                    var ss = s.Trim().ToLower();
                    query = query.Where(a => a.Tags.Any(c => c.Tag.Name.ToLower().Contains(ss)));
                }
            }
            if (stateFilter != null)
            {
                query = query.Where(a => a.Status == (ProductStatus)stateFilter);
            }
            if (base.User.Type == UserType.Admin || base.User.Type == UserType.Staff)
            {
                if (!string.IsNullOrWhiteSpace(sellerNameFilter))
                {
                    sellerNameFilter = sellerNameFilter.Trim().ToLower();
                    var sellers = db.Sellers.Where(x => x.Name.ToLower().Contains(sellerNameFilter));
                    query = query.Where(x => sellers.Any(y => y.Id == x.SellerId));
                }
            }
            if (!string.IsNullOrWhiteSpace(brandNameFilter))
            {
                brandNameFilter = brandNameFilter.Trim().ToLower();
                var brands = db.Brands.Where(x => x.Name.ToLower().Contains(brandNameFilter));
                query = query.Where(x => brands.Any(y => y.Id == x.BrandId));
            }
            if (!string.IsNullOrWhiteSpace(supplierNameFilter))
            {
                supplierNameFilter = supplierNameFilter.Trim().ToLower();
                var suppliers = db.Suppliers.Where(x => x.Name.ToLower().Contains(supplierNameFilter));
                query = query.Where(x => suppliers.Any(y => y.Id == x.SupplierId));
            }
            if (minPriceFilter.HasValue)
            {
                query = query.Where(x => x.Prices.First().RawPrice * x.Prices.First().PriceCoefficient
                                            >= minPriceFilter.Value);
            }
            if (maxPriceFilter.HasValue)
			{
                query = query.Where(x => x.Prices.First().RawPrice * x.Prices.First().PriceCoefficient 
                                            <= maxPriceFilter.Value);
			}
            if (minDiscountedPriceFilter.HasValue)
            {
                query = query.Where(x => x.Prices.First().RawDiscountedPrice * x.Prices.First().PriceCoefficient 
                                            >= minDiscountedPriceFilter.Value);
            }
            if (maxDiscountedPriceFilter.HasValue)
            {
                query = query.Where(x => x.Prices.First().RawDiscountedPrice * x.Prices.First().PriceCoefficient
                                            <= maxDiscountedPriceFilter.Value);
            }
            if (!string.IsNullOrWhiteSpace(isPublishedFilter))
            {
                bool isPublishedFilterValue;
                if (bool.TryParse(isPublishedFilter, out isPublishedFilterValue))
                {
                    query = query.Where(x => x.Published == isPublishedFilterValue);
                }                    
            }
            var data = await query.Project().ToListResultAsync(request);
            return KendoJson(data);
        }

        //private void FlexTextFilter<T>(IQueryable<T> query, Expression<Func<T, object>> p, string text)
        //{
        //    query.Where(Expression.)
        //}

        [HttpPost]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Seller })]
        [AjaxAction]
        public async Task<JsonResult> Product_Destroy(long id)
        {
            if (base.User.Type == UserType.Seller)
            {
                var sid = await GetSellerId();
                if (!db.Products.Any(p => p.SellerId == sid && p.Id == id))
                {
                    return Json(new { Success = false, Message = "محصول یافت نشد." });
                }
            }
            else
            {
                if (!db.Products.Any(p => p.Id == id))
                {
                    return Json(new { Success = false, Message = "محصول یافت نشد." });
                }
            }

            if (db.OrderItems.Any(a => a.ProductPrice.ProductId == id))
            {
                var prod = await db.Products.FindAsync(id);
                prod.Published = false;
                await db.SaveChangesAsync();
                return Json(new { Success = false, Message = "محصول در سفارش استفاده شده است. از حالت انتشار خارج میشود" });
            }
            else
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                await db.ProductQuantities.Where(a => a.ProductId == id).DeleteAsync();
                await db.ProductPrices.Where(a => a.ProductId == id).DeleteAsync();
                await db.Products.Where(a => a.Id == id).DeleteAsync();
                productImageService.DeleteImages(id);
                scope.Complete();
            }
            return Json(new { Success = true });
        }

        [HttpGet]
        public async Task<ActionResult> GetCategories(long? id)
        {
            var data = await db.Categories.OrderByDescending(c => c.ParentId).ThenBy(a => a.Order).ThenBy(x => x.Id)
                .Cacheable().Where(a => a.ParentId == id)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    hasChildren = a.Children.Any()
                }).ToListAsync();
            return Json(data);
        }

        [HttpGet]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Seller })]
        public async Task<IActionResult> Product(long? id)
        {
            var userType = base.User.Type;

            EditProductModel p;
            var sellerId = await GetSellerId();
            if (id.HasValue)
            {
                ViewData["Title"] = "ویرایش کالا و خدمات";
                var pr = await db.Products.Include(a => a.Prices)
                    .Include(a => a.ProductCategories).ThenInclude(a => a.Category)
                    .Include(p => p.Tags).Include(a => a.BuyLimitations)
                    .Include(a => a.Quantities)
                    .Where(a => a.Id == id && (userType != UserType.Seller || a.SellerId == UserId))
                    .FirstOrDefaultAsync();
                if (pr == null)
                    throw new EntityNotFoundException<Product>(id.Value);
                if (base.User.Type == UserType.Seller)
                {
                    if (pr.SellerId != sellerId)
                        throw new AccessDeniedException();
                }
                //productImageService.FixImageUrls(p);
                p = new EditProductModel(pr);
                //var productPrices = db.ProductPrices.FirstOrDefault(a => a.ProductId == id);
                //if (productPrices != null)
                //{
                //    p.DiscountPrice = productPrices.DiscountPrice;
                //    p.Price = productPrices.Price;
                //}
                p.Thubmnail = productImageService.GetImageUrl(p.Id, p.Thubmnail);
            }
            else
            {
                ViewData["Title"] = "درج کالا و خدمات";
                p = new EditProductModel();
            }
            ViewBag.ImagePathFormat = productImageService.GetImagePathFormatString(p.Id);
            ViewBag.IsPostback = false;
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Seller })]
        public async Task<IActionResult> Product(EditProductModel model)
        {
            if (ModelState.IsValid)
            {
                if(model.Quantity == 0 && model.Status == ProductStatus.Available)
                {
                    ShowMessage("در صورت تعیین موجودی 0، وضعیت نمیتواند موجود باشد.", Messages.MessageType.Error);
                    return View(model);
                }

                Product product;

                // The following foreach loop is checking for selected categories to be "child" categories, and
                // NOT parents, because, it's the policy that only child (or leaf, in graph terminology) categories 
                // can have products directly connected to them. 
                // Also, the code block has been moved here, at the top of the action method, because it's a 
                // validation, and, in case of failure, the whole method should be stoped (and reversed, if neccessary).
                var categoryIds = JsonConvert.DeserializeObject<string[]>(Request.Form["CategoryIds"][0]).Select(a => long.Parse(a));
                var categories = db.Categories.Where(x => categoryIds.Contains(x.Id)).ToList();
                foreach (var cat in categoryIds)
                {
                    var thisCategoryIsAProudParent = db.Categories.Any(c => c.ParentId == cat);
                    if (thisCategoryIsAProudParent)
                    {
                        //ShowMessage(string.Format("امکان ثبت در دسته بندی {0} وجود ندارد.",
                        //    categories.First(x => x.Id == cat).Title));
                        ViewBag.ErrorMessage = string.Format("محصول را نمیتوان در دسته بندی \'{0}\' قرار داد", categories.First(x => x.Id == cat).Title);
                        return View(model);
                    }
                }

                if (model.Id == 0)
                {
                    ViewData["Title"] = "درج کالا و خدمات";
                    product = new Product();
                    db.Products.Add(product);
                }
                else
                {
                    ViewData["Title"] = "ویرایش کالا و خدمات";
                    var userType = base.User.Type;
                    product = await db.Products
                        .Include(a => a.ProductCategories)
                        .ThenInclude(a => a.Category)
                        .Include(p => p.Prices)//.Where(c=>c.Active))
                        .Include(p => p.Tags)
                        .Include(p => p.BuyLimitations)
                        .Include(p => p.Quantities)
                        .FirstOrDefaultAsync(p => p.Id == model.Id && (userType != UserType.Seller || p.SellerId == UserId));
                    if (product == null)
                        throw new EntityNotFoundException<Product>(model.Id);

                    await CheckProductAccess(product);
                }
                product.Properties = JsonConvert.DeserializeObject<IList<KeyValuePair<string, string>>>(Request.Form["Properties"]);
                product.Title = model.Title;
                product.BrandId = model.BrandId;
                product.SupplierId = model.SupplierId;
                product.Description = model.Description;
                product.Status = model.Status;
                product.Published = model.Published;
                product.MaxBuyQuota = model.MaxBuyQuota;
                product.MinBuyQuota = model.MinBuyQuota;
                product.BuyQuotaDays = model.BuyQuotaDays;
                product.SellerId = base.User.Type == UserType.Seller ? await GetSellerId() : model.SellerId;
                product.Code = model.Code;
                product.Weight = model.Weight * (decimal)Math.Pow(10, (int)model.WeightUnit * 3);
                product.Volume = model.Volume * (decimal)Math.Pow(10, (int)model.VolumeUnit * 3);

                //var categoryIds = JsonConvert.DeserializeObject<string[]>(Request.Form["CategoryIds"][0]).Select(a => long.Parse(a));
                product.ProductCategories.Clear();
                foreach (var cat in categoryIds)
                {
                    product.ProductCategories.Add(new ProductCategory
                    {
                        CategoryId = cat
                    });
                }
                if (product.Prices.Any())
                {
                    var price = product.Prices.First();
                    price.RawPrice = model.RawPrice;
                    price.RawDiscountedPrice = model.RawDiscountPrice == 0 ? model.RawPrice : model.RawDiscountPrice;
                    price.PriceCoefficient = model.PriceCoefficient.HasValue ? model.PriceCoefficient.Value : 1;
                }
                else
                {
                    product.Prices.Add(new ProductPrice
                    {
                        RawPrice = model.RawPrice,
                        RawDiscountedPrice = model.RawDiscountPrice == 0 ? model.RawPrice : model.RawDiscountPrice,
                        PriceCoefficient = model.PriceCoefficient.HasValue ? model.PriceCoefficient.Value : 1
                    });
                }
                if (product.Quantities.Any())
                {
                    var q = product.Quantities.First();
                    q.Quantity = model.Quantity;
                    if (model.Quantity == 0)
                        product.Status = ProductStatus.NotAvailable;
                }
                else
                {
                    product.Quantities.Add(new ProductQuantity
                    {
                        Quantity = model.Quantity
                    });
                    if (model.Quantity == 0)
                        product.Status = ProductStatus.NotAvailable;
                }
                //foreach (var cat in product.ProductCategories)
                //{
                //    var parentCategory = await db.Categories.Include(c => c.Parent)
                //        .Where(c => c.ParentId == cat.CategoryId).FirstOrDefaultAsync();
                //    if (parentCategory != null)
                //    {
                //        ViewBag.ErrorMessage = string.Format("محصول را نمیتوان در دسته بندی {0} قرار داد", parentCategory.Parent.Title);
                //        return View(model);
                //    }
                //}
                product.Tags?.Clear();
                if (model.TagIds != null)
                {
                    product.Tags = new List<ProductTag>();
                    foreach (var tid in model.TagIds)
                    {
                        product.Tags.Add(new ProductTag()
                        {
                            Product = product,
                            TagId = tid
                        });
                    }
                }
                product.BuyLimitations?.Clear();
                if (model.LimitationIds != null)
                {
                    product.BuyLimitations = new List<ProductBuyLimitation>();
                    foreach (var id in model.LimitationIds)
                    {
                        product.BuyLimitations.Add(new ProductBuyLimitation()
                        {
                            Product = product,
                            BuyLimitationId = id
                        });
                    }
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Product", new { id = product.Id });
            }
            return View(model);
        }



        [HttpPost]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Seller })]
        public async Task<ActionResult> SaveImages(IEnumerable<IFormFile> images, long ID)
        {
            if (images != null)
            {
                List<string> imageList = new List<string>();
                var product = await db.Products.Where(p => p.Id == ID).FirstOrDefaultAsync();
                if (product == null)
                {
                    throw new EntityNotFoundException<Product>(ID);
                }
                await CheckProductAccess(product);
                if (product.ImageList == null)
                {
                    product.ImageList = new List<string>();
                }
                foreach (var file in images)
                {
                    if (file.Length > 0)
                    {
                        if (file.Length > 3145728)  // This number, expressed in bytes, is equivalent to 3 megabytes (3145728 bytes = 3 megabytes)
                        {
                            throw new ApiException(400, "حجم فایل عکس محصول بیش از اندازه بزرگ است! (ماکزیمم: 3 مگابایت)");
                        }
                        var fileExtension = Path.GetExtension(file.FileName);
                        var fileName = Guid.NewGuid() + fileExtension;

                        using (var stream = file.OpenReadStream())
                        {
                            await productImageService.SaveImage(ID, fileName, stream);
                        }
                        imageList.Add(fileName);
                    }
                }
                product.ImageList.AddRange(imageList);
                db.Entry(product).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Json(product.ImageList);
            }
            // Return an empty string to signify success
            return Content("");
        }


        [HttpPost]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Seller })]
        public async Task<ActionResult> SaveThumbnail(IEnumerable<IFormFile> thumbnails, long ID)
        {
            // The Name of the Upload component is "thumbnails"
            if (thumbnails != null)
            {
                if (thumbnails.Count() > 0)
                {
                    var product = await db.Products.Where(p => p.Id == ID).FirstOrDefaultAsync();
                    if (product == null)
                    {
                        throw new EntityNotFoundException<Product>(ID);
                    }
                    await CheckProductAccess(product);
                    var file = thumbnails.First();
                    if (file.Length > 51200) // 51200 bytes = 50 kilobytes
                    {
                        throw new ApiException(400, "حجم فایل عکس پیش نمایش بیش از اندازه بزرگ است! (ماکزیمم: 50 کیلوبایت)");
                    }
                    var fileName = $"thumbnail-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    //var path = Path.Combine(ProductsImagesPath, ID.ToString());
                    //using var ms = new MemoryStream();
                    //file.CopyTo(ms);
                    //var fileBytes = ms.ToArray();
                    //FileService.SaveFile(fileBytes, path, fileName);
                    using (var stream = file.OpenReadStream())
                    {
                        await productImageService.SaveImage(ID, fileName, stream);
                    }
                    product.Thubmnail = fileName;
                    db.Entry(product).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return Json(productImageService.GetThumbnailUrl(product));
                }
            }
            // Return an empty string to signify success
            return Content("");
        }

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Seller })]
        public async Task<ActionResult> DeleteImage(long Id, string fileName)
        {
            var product = await db.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();
            if (product == null)
            {
                throw new EntityNotFoundException<Product>(Id);
            }
            await CheckProductAccess(product);
            productImageService.DeleteImage(Id, fileName);
            product.ImageList.Remove(fileName);
            db.Entry(product).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(product.ImageList);
        }

        async Task CheckProductAccess(Product p)
        {
            var sellerId = await GetSellerId();
            if (base.User.Type == UserType.Seller && p.SellerId != sellerId)
                throw new AccessDeniedException();
        }

        [HttpPost]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Seller })]
        public async Task<JsonResult> Product_Change_Category(ProductChangeCategoryModel model)
        {
            var products = await db.Products.Include(p => p.ProductCategories).Where(p => model.ProductIds.Contains(p.Id)).ToListAsync();
            if (products.Count == 0)
            {
                return Json(new { Success = false, Message = "هیچ محصولی یافت نشد." });
            }
            var category = await db.Categories.Where(c => c.Id == model.CategoryId).FirstOrDefaultAsync();
            if (category == null)
            {
                return Json(new { Success = false, Message = "دسته بندی مورد نظر یافت نشد." });
            }
            var childCategories = await db.Categories.Where(x => x.ParentId.HasValue 
                    && x.ParentId.Value == category.Id).ToListAsync();
            if (childCategories != null && childCategories.Count > 0)
            {
                return Json(new { Success = false, Message = 
                    string.Format("اضافه کردن محصول در دسته بندی {0} مجاز نمیباشد.", category.Title) });
            }
            foreach (var product in products)
            {
                await CheckProductAccess(product);
                product.ProductCategories.Clear();
                product.ProductCategories.Add(new ProductCategory
                {
                    CategoryId = category.Id,
                    ProductId = product.Id
                });
            }
            await db.SaveChangesAsync();
            return Json(new { Success = true });
        }

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Seller })]
        public async Task<JsonResult> Product_AssignTag(ProductChangeCategoryModel model)
        {
            var products = await db.Products.Include(p => p.Tags).Where(p => model.ProductIds.Contains(p.Id)).ToListAsync();
            if (products.Count == 0)
            {
                return Json(new { Success = false, Message = "هیچ محصولی یافت نشد." });
            }
            //var category = await db.Categories.Where(c => c.Id == model.CategoryId).FirstOrDefaultAsync();
            //if (category == null)
            //{
            //    return Json(new { Success = false, Message = "دسته بندی مورد نظر یافت نشد." });
            //}
            foreach (var product in products)
            {
                await CheckProductAccess(product);
                product.Tags.Clear();
                product.Tags.Add(new ProductTag
                {
                    TagId = model.CategoryId,
                    ProductId = product.Id
                });
            }
            await db.SaveChangesAsync();
            return Json(new { Success = true });
        }

        #endregion


        #region Order


        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Delivery }, Order = 1)]
        public ActionResult BuyHistory()
        {
            ViewData["Title"] = "گزارش خرید ها";
            return View();
        }

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Delivery }, Order = 1)]
        public async Task<IActionResult> GetBuyHistory([DataSourceRequest] DataSourceRequest request, int? stateFilter)
        {
            var query = db.Orders.OrderByDescending(x => x.CheckOutDate)
                                    .Where(o => o.State == OrderState.Paid ||
                                                  o.State == OrderState.Delivered ||
                                                  o.State == OrderState.Sent);

            if (base.User.Type != UserType.Admin)
            {
                if (base.User.Type == UserType.Seller)
                {
                    var sellerId = await db.Sellers.Where(a => a.UserId == UserId).Select(a => a.Id).FirstOrDefaultAsync();
                    query = query.Where(a => a.Items.Any(a => a.ProductPrice.Product.SellerId == sellerId));
                }
            }

            if (stateFilter != null)
            {
                query = query.Where(a => a.State == (OrderState)stateFilter);
            }

            var data = await query
                .Select(a => new
                {
                    Id = a.Id,
                    TotalPrice = a.TotalPrice,
                    a.DeliveryPrice,
                    a.CheckOutDate,
                    a.ApproximateDeliveryDate,
                    a.ActualDeliveryDate,
                    a.User.FirstName,
                    a.User.LastName,
                    a.AddressId,
                    a.Address,
                    a.SendDate,
                    State = a.State
                }).ToDataSourceResultAsync(request, a => new OrderModel
                {
                    Id = a.Id,
                    CheckoutDate = Util.GetPersianDate(a.CheckOutDate),
                    ApproximateDeliveryDate = Util.GetPersianDate(a.ApproximateDeliveryDate),
                    SendDate = Util.GetPersianDate(a.SendDate),
                    ActualDeliveryDate = Util.GetPersianDate(a.ActualDeliveryDate),
                    Price = a.TotalPrice,
                    DeliveryPrice = a.DeliveryPrice,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Address_Id = a.AddressId,
                    Address = new AddressModel(a.Address),
                    State = TranslateExtentions.GetTitle(a.State)
                });

            var list = JsonConvert.SerializeObject(data, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
            return Content(list, "application/json");
        }

        //[AjaxAction]
        //[Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Delivery }, Order = 1)]
        //public async Task<ActionResult> ConfirmSent(long Id, string DelivererId)
        //{
        //    var order = await db.Orders.Where(o => o.Id == Id).FirstOrDefaultAsync();
        //    if (order == null)
        //    {
        //        throw new EntityNotFoundException<Order>(Id);
        //    }
        //    var user = await db.Users.Where(u => u.Id == order.UserId).FirstOrDefaultAsync();
        //    if (user == null)
        //    {
        //        throw new EntityNotFoundException<User>(order.UserId);
        //    }
        //    if (order.State == OrderState.Paid)
        //    {
        //        order.SendDate = DateTime.Now;
        //        order.State = OrderState.Sent;
        //        order.DelivererNo = DelivererId;
        //        order.TrackNo = new Random((int)(DateTime.Now.Ticks % int.MaxValue)).Next(100000, 999999).ToString();
        //        await db.SaveChangesAsync();
        //        await smsService.Send(user.MobileNumber, string.Format(Messages.Messages.Order.DeliveredOTPMessage, order.TrackNo));
        //    }
        //    else
        //    {
        //        return Json(new { success = false, message = Messages.Messages.Order.ErrorConvertStateToSent });
        //    }
        //    return Json(new { success = true });
        //}

        // This action was implemented as a test, and it has no actual usage for the project!
        // So, there's no reason for not commenting it out!
        //[AjaxAction]
        //[Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Delivery }, Order = 1)]
        //public async Task<ActionResult> ConfirmDelivered(long Id, string TrackNo)
        //{
        //    var order = await db.Orders.Where(o => o.Id == Id).FirstOrDefaultAsync();
        //    if (order == null)
        //    {
        //        throw new EntityNotFoundException<Order>(Id);
        //    }
        //    if (order.TrackNo != TrackNo)
        //    {
        //        return Json(new { success = false, message = Messages.Messages.Order.ErrorWrongTrackNo });
        //    }
        //    if (order.State == OrderState.Sent)
        //    {
        //        order.State = OrderState.Delivered;
        //        order.ActualDeliveryDate = DateTime.Now;
        //    }
        //    else
        //    {
        //        return Json(new { success = false, message = Messages.Messages.Order.ErrorConvertStateToDelivered });
        //    }
        //    await db.SaveChangesAsync();
        //    return Json(new { success = true });
        //}


        #endregion


        #region Seller
        [Authorize(UserType.Admin)]
        public async Task<ActionResult> Sellers()
        {
            ViewBag.Users = await db.Users.Where(u => u.Type == UserType.Seller).OrderBy(x => x.Id)
                .Select(u => new SelectListItem
            {
                Text = u.FirstName + " " + u.LastName + " (" + u.Username + ")",
                Value = u.Id.ToString()
            }).ToListAsync();
            var users = db.Users.Where(a => a.Type == UserType.Seller).ToList();
            ViewData["users"] = users;
            ViewData["defaultUser"] = users.FirstOrDefault();
            return View();
        }
        [Authorize(UserType.Admin)]
        public ActionResult GetAllSellers([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Sellers.OrderBy(x => x.Name).Include(s => s.User), request);
        }

        [Authorize(UserType.Admin)]
        public async Task<ActionResult> UpdateSeller(Seller seller)
        {
            seller.UserId = seller.User?.Id;
            seller.User = null;
            if (seller.Id > 0)
            {
                var dbSeller = await db.Sellers.FindAsync(seller.Id);
                dbSeller.Name = seller.Name;
                dbSeller.UserId = seller.UserId;
                dbSeller.Lat = seller.Lat;
                dbSeller.Lng = seller.Lng;
                dbSeller.Address = seller.Address;
                dbSeller.PhoneNumber = seller.PhoneNumber;
                dbSeller.AccountNumber = seller.AccountNumber;
                dbSeller.Basket = seller.Basket;
            }
            else
            {
                db.Sellers.Add(seller);
            }
            await db.SaveChangesAsync();
            return Json(seller);
        }

        [Authorize(UserType.Admin)]
        public async Task<ActionResult> RemoveSeller(Seller seller)
        {
            db.Sellers.Attach(seller);
            db.Sellers.Remove(seller);
            await db.SaveChangesAsync();
            return Json(null);
        }

        #endregion Seller


        #region Tags
        [Authorize(UserType.Admin)]
        public ActionResult Tags()
        {
            return View();
        }
        [Authorize(UserType.Admin)]
        public ActionResult GetAllTags([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Tags.OrderBy(x => x.Name), request);
        }

        [HttpPost]
        [Authorize(UserType.Admin)]
        public async Task<ActionResult> CreateTag(Tag tag)
        {
            tag.Name = Util.TrimString(tag.Name);
            if (tag.Id > 0)
            {
                throw new BadRequestException("Tag already exists!");
            }
            else
            {
                //if (db.Tags.Any(x => Util.TrimString(x.Name).Equals(tag.Name)))
                if (db.Tags.Any(x => x.Name.Trim().Trim(Util.ZeroWidthNonBreakingSpace).Equals(tag.Name)))
                {
                    throw new BadRequestException($"درج ناموفق: نام وارد شده تکراری است! تگ با نام \"{tag.Name}\" قبلاً در دیتابیس تعریف شده است.");
                }
                else
                {
                    db.Tags.Add(tag);
                    await db.SaveChangesAsync();
                }
            }
            return Json(tag);
        }

        [HttpPost]
        [Authorize(UserType.Admin)]
        public async Task<IActionResult> UpdateTag(Tag tag)
        {
            if (tag.Id > 0)
            {
                var dbTag = await db.Tags.FindAsync(tag.Id);
                tag.Name = Util.TrimString(tag.Name);
                if (db.Tags.Any(x => !x.Id.Equals(tag.Id) && //Util.TrimString(x.Name).Equals(tag.Name)))
                    x.Name.Trim().Trim(Util.ZeroWidthNonBreakingSpace).Equals(tag.Name)))
                {
                    throw new BadRequestException($"ویرایش ناموفق: نام وارد شده تکراری است! تگ با نام \"{tag.Name}\" قبلاً در دیتابیس تعریف شده است.");
                }
                dbTag.Name = tag.Name;
                dbTag.Order = tag.Order;
                await db.SaveChangesAsync();
            }
            else
            {
                throw new BadRequestException("You're trying to update a non-existing tag!");
            }
            return Json(tag);
        }

        [Authorize(UserType.Admin)]
        public async Task<ActionResult> RemoveTag(Tag tag)
        {
            db.Tags.Attach(tag);
            db.Tags.Remove(tag);
            await db.SaveChangesAsync();
            return Json(null);
        }

        #endregion Tags

        #region Settlements
        [HttpGet]
        [Authorize(new UserType[] { UserType.Admin, UserType.Seller })]
        public ActionResult ProductPaymentPartyList()
        {
            ViewData["Title"] = "لیست اقلام تسهیم";
            return View();
        }
        [HttpPost]
        [Authorize(new UserType[] { UserType.Admin, UserType.Seller })]
        public async Task<IActionResult> GetProductPaymentPartyListData([DataSourceRequest] DataSourceRequest request)
        {
            var query = db.ProductPaymentParties
                .Include(x => x.PaymentParty)
                .Include(x => x.Product).AsQueryable();
            if (base.User.Type != UserType.Admin)
            {
                if (base.User.Type == UserType.Seller)
                {
                    var sellerId = await GetSellerId();
                    var seller = db.Sellers.Where(x => x.Id == sellerId).SingleOrDefault();
                    if (seller == null)
                        return null;
                    query = query.Where(x => x.PaymentParty.ShabaId.ToLower().Equals(seller.AccountNumber.ToLower()));
                }
            }
            var result = await query.Select(x =>
            new ProductPaymentPartyVM()
            {
                ProductId = x.ProductId,
                Product = x.Product,
                PaymentPartyId = x.PaymentPartyId,
                PaymentParty = x.PaymentParty,
                Percent = x.Percent
            }).ToDataSourceResultAsync(request);
            return KendoJson(result);
        }

        [HttpGet]
        [Authorize(new UserType[] { UserType.Admin, UserType.Seller })]
        public ActionResult PaymentSettlementList()
        {
            ViewData["Title"] = "لیست اقلام تسهیم";
            return View();
        }
        [HttpPost]
        [Authorize(new UserType[] { UserType.Admin, UserType.Seller })]
        public async Task<IActionResult> GetPaymentSettlementListData([DataSourceRequest] DataSourceRequest request)
        {
            var query = db.PaymentSettlements.AsQueryable();
            if (base.User.Type != UserType.Admin)
            {
                if (base.User.Type == UserType.Seller)
                {
                    var sellerId = await GetSellerId();
                    var seller = db.Sellers.Where(x => x.Id == sellerId).SingleOrDefault();
                    if (seller == null)
                        return null;
                    query = query.Where(x => x.ShabaId.ToLower().Equals(seller.AccountNumber.ToLower()));
                }
            }
            var result = await query.Select(x => 
            new PaymentSettlementVM()
            { 
                Id = x.Id,
                Amount = x.Amount,
                Date = x.Date,
                ItemId = x.ItemId,
                Name = x.Name,
                OrderId = x.OrderId,
                Order = x.Order,
                PayFor = x.PayFor,
                PaymentId = x.PaymentId,
                Payment = x.Payment,
                Response = x.Response,
                ShabaId = x.ShabaId,
                Status = x.Status
            }).ToDataSourceResultAsync(request);
            return KendoJson(result);
        }

        #endregion Settlements

        [HttpGet]
        [Authorize(UserType.Admin)]
        public ActionResult ImportProductPrices()
        {
            ViewBag.Message = TempData["Message"];
            ViewBag.Error = TempData["Error"];
            return View();
        }
        [HttpPost]
        [Authorize(UserType.Admin)]
        public async Task<ActionResult> ImportProductPrices(IEnumerable<IFormFile> files)
        {
            var file = files.FirstOrDefault();
            if (file == null)
                ViewBag.Error = "فایل انتخاب نشده است";
            else
            {
                try
                {
                    var count = await importService.ImportProductCountAndPrices(file.OpenReadStream());
                    TempData["Message"] = $"تعداد {count} سطر از فایل دریافت شد";
                    return RedirectToAction("ImportProductPrices");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                    ViewBag.Error = ex.Message;
                }
            }
            return View();
        }

        private ContentResult ConvertDataToJson<T>(IQueryable<T> data, [DataSourceRequest] DataSourceRequest request)
        {
            var list = JsonConvert.SerializeObject(data.ToDataSourceResult(request), Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
            return Content(list, "application/json");
        }


        [HttpGet]
        public ActionResult ReleaseNotes()
        {
            var contents = System.IO.File.ReadAllText(Path.Combine(pathService.AppRoot, "ReleaseNotes.json"));
            var dic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(contents);
            return View(dic);
        }
    }
}