using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
using MahtaKala.Models;
using MahtaKala.Models.ProductModels;
using MahtaKala.Models.StaffModels;
using MahtaKala.Services;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Z.EntityFramework.Plus;

namespace MahtaKala.Controllers
{
    public class StaffController : ApiControllerBase<StaffController>
    {
        private readonly IProductImageService productImageService;
        private readonly ICategoryImageService categoryImageService;
        private readonly ImportService importService;
        private readonly CategoryService categoryService;
        private readonly ISMSService smsService;

        public StaffController(
            DataContext context,
            ILogger<StaffController> logger,
            ISMSService smsService,
            IProductImageService productImageService,
            ICategoryImageService categoryImageService,
            ImportService importService,
            CategoryService categoryService
            ) : base(context, logger)
        {
            this.productImageService = productImageService;
            this.categoryImageService = categoryImageService;
            this.importService = importService;
            this.categoryService = categoryService;
            this.smsService = smsService;
        }
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Delivery, UserType.Seller })]
        public async Task<IActionResult> Index()
        {
            if (base.User.Type == UserType.Delivery)
            {
                return RedirectToAction("BuyHistory");
            }
            var report = new ReportModel();
            var user = base.User;
            var orders = db.Orders.Where(o => o.State == OrderState.Paid ||
                                                  o.State == OrderState.Delivered ||
                                                  o.State == OrderState.Sent)
                        .Where(a => a.CheckOutData != null);
            var orderChart = orders.OrderBy(o => o.CheckOutData)
                .GroupBy(o => o.CheckOutData.Value.Date)
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
            var saleChart = orders.OrderBy(o => o.CheckOutData)
                .GroupBy(o => o.CheckOutData.Value.Date)
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
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(UserType.Admin)]
        public new IActionResult User(User model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    if (string.IsNullOrEmpty(model.Password))
                    {
                        ModelState.AddModelError(nameof(model.Password), "Password Required.");
                        return View(model);
                    }
                    db.Users.Add(model);
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
                }
                db.SaveChanges();
                return RedirectToAction("UserList");
            }
            return View(model);
        }

        [HttpPost]
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
            return ConvertDataToJson(db.Provinces, request);
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
            return ConvertDataToJson(db.Cities.Include(c => c.Province), request);
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
            ViewBag.Provinces = db.Provinces.ToList();
            return View(city);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public IActionResult City(City model)
        {
            ViewBag.Provinces = db.Provinces.ToList();
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
        public ActionResult GetAllCategories([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Categories.Include(c => c.Parent), request);
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
                }
                await db.SaveChangesAsync();
                ViewBag.IsPostback = true;
                return View(model);
            }
            return View(model);
        }
        [HttpPost]
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
            if(db.Categories.Any(c=>c.ParentId == id))
            {
                var category = await db.Categories.FindAsync(id);
                category.Published = false;
                await db.SaveChangesAsync();
                return Json(new { Success = false, Message = "دسته بندی دارای فرزند است. از حالت انتشار خارج میشود" });
            }
            else
            {
                db.Categories.Where(c => c.Id == id).Delete();
                return Json(new { Success = true });
            }
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


        #region Product

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public IActionResult ProductList()
        {
            ViewData["Title"] = "لیست کالا و خدمات";
            return View();
        }

        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public async Task<IActionResult> Product_Read([DataSourceRequest] DataSourceRequest request)
        {
            var data = await db.Products.AsQueryable().Project().ToListResultAsync(request);
            return KendoJson(data);
        }

        [HttpPost]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public async Task<JsonResult> Product_Destroy(long id)
        {
            if (db.OrderItems.Any(a => a.ProductPrice.ProductId == id))
            {
                var prod = await db.Products.FindAsync(id);
                prod.Published = false;
                await db.SaveChangesAsync();
                return Json(new { Success = false, Message = "محصول در سفارش استفاده شده است. از حالت انتشار خارج میشود" });
            }
            else
            {
                db.ProductQuantities.Where(a => a.ProductId == id).Delete();
                db.ProductPrices.Where(a => a.ProductId == id).Delete();
                db.Products.Where(a => a.Id == id).Delete();
            }
            return Json(new { Success = true });
        }

        [HttpGet]
        public async Task<ActionResult> GetCategories(long? id)
        {
            var data = await db.Categories.Cacheable().Where(a => a.ParentId == id)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    hasChildren = a.Children.Any()
                }).ToListAsync();
            return Json(data);
        }

        [HttpGet]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public async Task<IActionResult> Product(long? id)
        {
            ViewData["Title"] = "درج کالا و خدمات";

            Product p;
            if (id.HasValue)
            {
                p = await db.Products.Include(a => a.Prices)
                    .Include(a => a.ProductCategories).ThenInclude(a => a.Category)
                    .Where(a => a.Id == id).FirstOrDefaultAsync();
                if (p == null)
                    throw new EntityNotFoundException<Product>(id.Value);
                productImageService.FixImageUrls(p);
                var productPrices = db.ProductPrices.FirstOrDefault(a => a.ProductId == id);
                if (productPrices != null)
                {
                    p.DiscountPrice = productPrices.DiscountPrice;
                    p.Price = productPrices.Price;
                }
            }
            else
            {
                p = new Product()
                {
                    Characteristics = new List<Characteristic>(),
                    ProductCategories = new List<ProductCategory>()
                };
            }
            ViewBag.IsPostback = false;
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin })]
        public async Task<IActionResult> Product(Product model)
        {
            ViewData["Title"] = "درج کالا و خدمات";
            if (ModelState.IsValid)
            {
                Product product;
                if (model.Id == 0)
                {
                    product = new Product();
                    db.Products.Add(product);
                }
                else
                {
                    product = await db.Products
                        .Include(a => a.ProductCategories).ThenInclude(a => a.Category)
                        .Include(p => p.Prices)//.Where(c=>c.Active))
                        .FirstOrDefaultAsync(p => p.Id == model.Id);
                }
                product.Properties = JsonConvert.DeserializeObject<IList<KeyValuePair<string, string>>>(Request.Form["Properties"]);
                product.Title = model.Title;
                product.BrandId = model.BrandId;
                product.Description = model.Description;
                product.Status = model.Status;
                product.Published = model.Published;
                product.MaxBuyQuota = model.MaxBuyQuota;
                product.MinBuyQuota = model.MinBuyQuota;
                product.BuyQuotaDays = model.BuyQuotaDays;

                var categoryIds = JsonConvert.DeserializeObject<string[]>(Request.Form["CategoryIds"][0]).Select(a => long.Parse(a));
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
                    price.Price = model.Price;
                    price.DiscountPrice = model.DiscountPrice == 0 ? model.Price : model.DiscountPrice;
                }
                else
                {
                    product.Prices.Add(new ProductPrice
                    {
                        Price = model.Price,
                        DiscountPrice = model.DiscountPrice == 0 ? model.Price : model.DiscountPrice
                    });
                }

                await db.SaveChangesAsync();
                ViewBag.IsPostback = true;
                return View(product);
            }
            return View(model);
        }



        [HttpPost]
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
                if (product.ImageList == null)
                {
                    product.ImageList = new List<string>();
                }
                foreach (var file in images)
                {
                    if (file.Length > 0)
                    {
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
                return Json(productImageService.GetImageUrls(product));
            }
            // Return an empty string to signify success
            return Content("");
        }


        [HttpPost]
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
                    var file = thumbnails.First();
                    var fileName = $"Thumbnail-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
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

        public async Task<ActionResult> DeleteImage(long Id, string fileName)
        {
            var product = await db.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();
            if (product == null)
            {
                throw new EntityNotFoundException<Product>(Id);
            }
            productImageService.DeleteImage(Id, fileName);
            product.ImageList.Remove(fileName);
            db.Entry(product).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(product.ImageList);
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
        public async Task<IActionResult> GetBuyHistory([DataSourceRequest] DataSourceRequest request)
        {
            var data = await db.Orders.Where(o => o.State == OrderState.Paid ||
                                                  o.State == OrderState.Delivered ||
                                                  o.State == OrderState.Sent)
                .Select(a => new
                {
                    Id = a.Id,
                    Price = a.TotalPrice,
                    a.CheckOutData,
                    a.ApproximateDeliveryDate,
                    a.ActualDeliveryDate,
                    Name = a.User.FirstName + " " + a.User.LastName,
                    a.SendDate,
                    State = a.State
                }).ToDataSourceResultAsync(request, a => new BuyHistoryModel
                {
                    Id = a.Id,
                    CheckoutDate = Util.GetPersianDate(a.CheckOutData),
                    ApproximateDeliveryDate = Util.GetPersianDate(a.ApproximateDeliveryDate),
                    SendDate = Util.GetPersianDate(a.SendDate),
                    ActualDeliveryDate = Util.GetPersianDate(a.ActualDeliveryDate),
                    Price = (long)a.Price,
                    Customer = a.Name,
                    State = TranslateExtentions.GetTitle(a.State)
                });

            var list = JsonConvert.SerializeObject(data, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
            return Content(list, "application/json");
        }

        [AjaxAction]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin }, Order = 1)]
        public async Task<ActionResult> ConfirmSent(long Id, string DelivererId)
        {
            var order = await db.Orders.Where(o => o.Id == Id).FirstOrDefaultAsync();
            if (order == null)
            {
                throw new EntityNotFoundException<Order>(Id);
            }
            var user = await db.Users.Where(u => u.Id == order.UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new EntityNotFoundException<User>(order.UserId);
            }
            if (order.State == OrderState.Paid)
            {
                order.SendDate = DateTime.Now;
                order.State = OrderState.Sent;
                order.DelivererNo = DelivererId;
                order.TrackNo = new Random().Next(100000, 999999).ToString();
                await db.SaveChangesAsync();
                await smsService.Send(user.MobileNumber, string.Format(Messages.Messages.Order.DeliveredOTPMessage, order.TrackNo));
            }
            else
            {
                return Json(new { success = false, message = Messages.Messages.Order.ErrorConvertStateToSent });
            }
            return Json(new { success = true });
        }

        [AjaxAction]
        [Authorize(new UserType[] { UserType.Staff, UserType.Admin, UserType.Delivery }, Order = 1)]
        public async Task<ActionResult> ConfirmDelivered(long Id, string TrackNo)
        {
            var order = await db.Orders.Where(o => o.Id == Id).FirstOrDefaultAsync();
            if (order == null)
            {
                throw new EntityNotFoundException<Order>(Id);
            }
            if (order.TrackNo != TrackNo)
            {
                return Json(new { success = false, message = Messages.Messages.Order.ErrorWrongTrackNo });
            }
            if (order.State == OrderState.Sent)
            {
                order.State = OrderState.Delivered;
                order.ActualDeliveryDate = DateTime.Now;
            }
            else
            {
                return Json(new { success = false, message = Messages.Messages.Order.ErrorConvertStateToDelivered });
            }
            await db.SaveChangesAsync();
            return Json(new { success = true });
        }


        #endregion


        #region Seller
        public ActionResult Sellers()
        {
            return View();
        }
        public ActionResult GetAllSellers([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Sellers, request);
        }

        public async Task<ActionResult> UpdateSeller(Seller seller)
        {
            if (seller.Id > 0)
            {
                var dbSeller = await db.Sellers.FindAsync(seller.Id);
                dbSeller.Name = seller.Name;
                dbSeller.AccountBankName = seller.AccountBankName;
            }
            else
            {
                db.Sellers.Add(seller);
            }
            await db.SaveChangesAsync();
            return Json(seller);
        }

        public async Task<ActionResult> RemoveSeller(Seller seller)
        {
            db.Sellers.Attach(seller);
            db.Sellers.Remove(seller);
            await db.SaveChangesAsync();
            return Json(null);
        }

        #endregion Seller


        [HttpGet]
        public ActionResult ImportProductPrices()
        {
            ViewBag.Message = TempData["Message"];
            ViewBag.Error = TempData["Error"];
            return View();
        }
        [HttpPost]
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

        [HttpGet]

        private ContentResult ConvertDataToJson<T>(IQueryable<T> data, [DataSourceRequest] DataSourceRequest request)
        {
            var list = JsonConvert.SerializeObject(data.ToDataSourceResult(request), Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
            return Content(list, "application/json");
        }
    }
}