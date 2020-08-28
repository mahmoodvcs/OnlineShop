using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.Entities.Security;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Z.EntityFramework.Plus;

namespace MahtaKala.Controllers
{
    [Authorize(Entities.UserType.Admin)]
    public class StaffController : ApiControllerBase<StaffController>
    {
        public StaffController(DataContext context, ILogger<StaffController> logger)
            : base(context, logger)
        {
        }
        public IActionResult Index()
        {
            return View();
        }


        #region Users


        public IActionResult UserList()
        {
            return View();
        }
        public ActionResult GetAllUsers([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Users.ToList(), request);
        }
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

        #endregion


        #region Provinces

        public IActionResult ProvinceList()
        {
            return View();
        }
        public ActionResult GetAllProvinces([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Provinces.ToList(), request);
        }
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

        public IActionResult CityList()
        {
            return View();
        }
        public ActionResult GetAllCities([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Cities.Include(c => c.Province).ToList(), request);
        }
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

        public IActionResult CategoryList()
        {
            return View();
        }
        public ActionResult GetAllCategories([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Categories.Include(c => c.Parent).ToList(), request);
        }
        public ActionResult Category(long id)
        {
            ProductCategory productCategory = null;
            if (id == 0)
            {
                productCategory = new ProductCategory();
            }
            else
            {
                productCategory = db.Categories.Where(u => u.Id == id).FirstOrDefault();
                if (productCategory == null)
                {
                    throw new EntityNotFoundException<ProductCategory>(id);
                }
            }
            ViewBag.Categories = db.Categories.Where(c => c.Id != id).ToList();
            return View(productCategory);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Category(ProductCategory model)
        {
            ViewBag.Categories = db.Categories.Where(c => c.Id != model.Id).ToList();
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    if (string.IsNullOrEmpty(model.Title))
                    {
                        ModelState.AddModelError(nameof(model.Title), "Title Required.");
                        return View(model);
                    }
                    db.Categories.Add(model);
                }
                else
                {
                    if (!db.Categories.Any(u => u.Id == model.Id))
                    {
                        throw new EntityNotFoundException<ProductCategory>(model.Id);
                    }
                    db.Entry(model).State = EntityState.Modified;
                }
                db.SaveChanges();
                return RedirectToAction("CategoryList");
            }
            return View(model);
        }

        #endregion


        #region Brand

        public IActionResult BrandList()
        {
            return View();
        }
        public ActionResult GetAllBrands([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Brands.ToList(), request);
        }
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
        public IActionResult Brand(Brand model)
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

        #endregion


        #region Product

        public IActionResult ProductList()
        {
            ViewData["Title"] = "لیست محصولات";
            return View();
        }

        public IActionResult Product_Read([DataSourceRequest] DataSourceRequest request)
        {
            return ConvertDataToJson(db.Products.Include(c => c.Category).ToList(), request);
        }

        [HttpPost]
        public JsonResult Product_Destroy(int id)
        {
            db.ProductQuantities.Where(a => a.ProductId == id).Delete();
            db.ProductPrices.Where(a => a.ProductId == id).Delete();
            db.Products.Where(a => a.Id == id).Delete();
            return Json(new { Success = true });
        }

        [HttpGet]
        public IActionResult Product(int? id)
        {
            ViewData["Title"] = "درج محصول";           
            Product p;
            if (id.HasValue)
            {
                p = db.Products.FirstOrDefault(a => a.Id == id);
                var productPrices = db.ProductPrices.FirstOrDefault(a=>a.ProductId==id);
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
                    Characteristics = new List<Characteristic>()
                };
            }
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Product(Product model)
        {
            ViewData["Title"] = "درج محصول";
            if (ModelState.IsValid)
            {
                model.Characteristics = JsonConvert.DeserializeObject<IList<Characteristic>>(Request.Form["characteristicsJson"]);
                if (model.Id == 0)
                {
                    db.Products.Add(model);
                }
                else
                {
                    db.Entry(model).State = EntityState.Modified;
                }

                var productPrices = db.ProductPrices.FirstOrDefault(a => a.ProductId == model.Id);
                if (productPrices == null)
                {
                    productPrices = new ProductPrice();
                    productPrices.Product = model;
                    db.ProductPrices.Add(productPrices);
                }
                productPrices.DiscountPrice = model.DiscountPrice;
                productPrices.Price = model.Price;

                db.SaveChanges();
                return RedirectToAction("ProductList", "Staff");
            }
            return View(model);
        }

  
        #endregion







        private ContentResult ConvertDataToJson<T>(IEnumerable<T> data, [DataSourceRequest] DataSourceRequest request)
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