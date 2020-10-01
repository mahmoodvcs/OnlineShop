using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus.DataSets;
using MahtaKala.Entities;
using MahtaKala.GeneralServices.Payment;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure;
using MahtaKala.Infrustructure.ActionFilter;
using MahtaKala.Models;
using MahtaKala.Models.CustomerModels;
using MahtaKala.Services;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Index.Bintree;
using Z.EntityFramework.Plus;

namespace MahtaKala.Controllers
{

    public class CartController : SiteControllerBase<CartController>
    {
        private readonly IPathService pathService;
        private readonly IProductImageService imageService;
        private readonly OrderService orderService;

        public CartController(DataContext dataContext, ILogger<CartController> logger, IHttpContextAccessor contextAccessor,
            IPathService pathService,
            IProductImageService imageService,
            OrderService orderService
            ) : base(dataContext, logger)
        {
            this.contextAccessor = contextAccessor;
            this.pathService = pathService;
            this.imageService = imageService;
            this.orderService = orderService;
        }
        private readonly IHttpContextAccessor contextAccessor;

        public async Task<IActionResult> Index()
        {
            List<ShoppingCart> cartItems = await GetCartItems(true);
            return View(cartItems);
        }

        public async Task<ActionResult> ShoppingBag()
        {

            List<ShoppingCart> cartItems = await orderService.GetCartItems();

            foreach (var item in cartItems)
            {
                imageService.FixImageUrls(item.ProductPrice.Product);
            }
            return PartialView("ShoppingBag", cartItems);
        }

        [HttpPost]
        [AjaxAction]
        public async Task<ActionResult> AddToCart(int id, int count = 1)
        {
            await orderService.AddToCart(id, count);
            return Json(new { success = true, count = await orderService.GetShoppingCartCount() });
        }

        [HttpPost]
        public async Task<ActionResult> RemoveFromCart(long id)
        {
            await orderService.RemoveFromCart(id);
            return Json(new { success = true, count = await orderService.GetShoppingCartCount() });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCart(int id, int count)
        {
            var cartItem = await orderService.UpdateCartItem(id, count);
            var finalcostRow = Util.Sub3Number(count * cartItem.ProductPrice.DiscountPrice);
            List<ShoppingCart> cartItems = await GetCartItems();
            decimal sumPrice = 0;
            decimal sumFinalPrice = 0;
            foreach (var item in cartItems)
            {
                sumPrice += item.ProductPrice.Price * item.Count;
                sumFinalPrice += item.ProductPrice.DiscountPrice * item.Count;
            }
            return Json(new { success = true, count = cartItems.Sum(a => a.Count), id, finalcostRow, sumPrice, sumFinalPrice });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteItemCart(int id)
        {
            await orderService.RemoveFromCart(id);
            List<ShoppingCart> cartItems = await GetCartItems();
            var sumPrice = Util.Sub3Number(cartItems.Sum(a => a.ProductPrice.Price) * cartItems.Sum(a => a.Count));
            var sumFinalPrice = Util.Sub3Number(cartItems.Sum(a => a.ProductPrice.DiscountPrice) * cartItems.Sum(a => a.Count));
            return Json(new { success = true, count = cartItems.Sum(a => a.Count), id, sumPrice, sumFinalPrice });
        }



        public async Task<IActionResult> Checkout()
        {
            var user = User;
            if (user == null)
            {
                return RedirectToAction("index", "Account");
            }
            var getCartItems = await GetCartItems(true);
            if (getCartItems.Count() == 0)
            {
                return RedirectToAction("Category", "home");
            }

            decimal postCost = orderService.GetDeliveryPrice();
            decimal sumFinalPrice = 0;
            foreach (var item in getCartItems)
            {
                sumFinalPrice += item.ProductPrice.DiscountPrice * item.Count;
            }
            CheckOutVM model = new CheckOutVM();
            UserDataVM vm = new UserDataVM();
            vm.EmailAddress = user.EmailAddress;
            vm.FirstName = user.FirstName;
            vm.LastName = user.LastName;
            vm.NationalCode = user.NationalCode;
            model.UserData = vm;
            model.CartItemCount = getCartItems.Count();
            model.Cost = Util.Sub3Number(sumFinalPrice);
            model.PostCost = Util.Sub3Number(postCost);
            model.FinalCost = Util.Sub3Number(sumFinalPrice + postCost);
            return View(model);
        }

        [HttpPost]
        [AjaxAction]
        public async Task<IActionResult> Checkout(CheckOutVM vm)
        {
            var cartItems = await GetCartItems();
            if (cartItems.Count() == 0)
            {
                return Json(new { success = false, msg = "سبد خرید خالی می باشد" });
            }

            if (string.IsNullOrEmpty(vm.UserData.FirstName))
                return Json(new { success = false, msg = "لطفا نام را وارد کنید" });
            if (Util.IsAnyNumberInString(vm.UserData.FirstName))
                return Json(new { success = false, msg = "لطفا برای نام از حروف استفاده نمایید" });
            if (string.IsNullOrEmpty(vm.UserData.LastName))
                return Json(new { success = false, msg = "لطفا نام خانوادگی را وارد کنید" });
            if (Util.IsAnyNumberInString(vm.UserData.LastName))
                return Json(new { success = false, msg = "لطفا برای نام خانوادگی از حروف استفاده نمایید" });
            if (!string.IsNullOrEmpty(vm.UserData.EmailAddress))
            {
                if (!Util.IsValidEmailaddress(vm.UserData.EmailAddress))
                    return Json(new { success = false, msg = "لطفا ایمیل را به صورت صحیح وارد کنید" });
            }
            if (!string.IsNullOrEmpty(vm.UserData.NationalCode))
            {
                string msg;
                if (!Util.IsCheckNationalCode(vm.UserData.NationalCode, out msg))
                    return Json(new { success = false, msg });
            }


            User user = db.Users.FirstOrDefault(a => a.Id == UserId);
            user.FirstName = vm.UserData.FirstName;
            user.EmailAddress = vm.UserData.EmailAddress;
            user.LastName = vm.UserData.LastName;
            user.NationalCode = vm.UserData.NationalCode;

            if (!vm.UserData.AddressId.HasValue)
            {
                return Json(new { success = false, msg = "لطفا آدرس را انتخاب نمایید" });
            }

            var order = await orderService.Checkout(vm.UserData.AddressId.Value);
            var payment = await orderService.InitPayment(order, pathService.AppBaseUrl + "/Payment/CallBackPay");
            string payUrl = pathService.AppBaseUrl + $"/Payment/Pay?pid={payment.Id}&uid={payment.UniqueId}&source=site";
            return Json(new { success = true, msg = payUrl });
        }


        public IActionResult UserAddress()
        {
            var p = new UserAddress();
            p.UserId = UserId;
            return PartialView("UserAddress", p);
        }

        [HttpPost]
        public IActionResult UserAddress(UserAddress model)
        {
            if (string.IsNullOrEmpty(model.Title))
                return Json(new { success = false, msg = "لطفا عنوان را وارد نمایید" });
            if (string.IsNullOrEmpty(model.PostalCode))
                return Json(new { success = false, msg = "لطفا کد پستی را وارد نمایید" });
            if (model.PostalCode.Length != 10)
                return Json(new { success = false, msg = "کد پستی را به صورت 10 رقم وارد نمایید" });
            if (model.CityId==0)
                return Json(new { success = false, msg = "لطفا شهر را انتخاب نمایید" });
            if (string.IsNullOrEmpty(model.Details))
                return Json(new { success = false, msg = "لطفا آدرس را وارد نمایید" });
            db.Addresses.Add(model);
            db.SaveChanges();
            return Json(new { success = true, msg = "ثبت آدرس جدید با موفقیت انجام شد" });
        }


        [NonAction]
        private async Task<List<ShoppingCart>> GetCartItems(bool prepareToView = false)
        {
            List<ShoppingCart> cartItems = await orderService.GetCartItems();
            if (prepareToView)
            {
                foreach (var item in cartItems)
                {
                    imageService.FixImageUrls(item.ProductPrice.Product);
                }
            }
            return cartItems;
        }

        public JsonResult GetUserAddress()
        {
            var lst = db.Addresses.Where(a => a.UserId == UserId).OrderByDescending(a => a.Id).Select(a => new { a.Id, Name = (a.City.Province.Name + "-" + a.City.Name + "-" + a.Details) });
            return Json(lst);
        }
    }
}