using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus.DataSets;
using MahtaKala.Entities;
using MahtaKala.GeneralServices.Payment;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure;
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

        public ActionResult ShoppingBag()
        {

            List<ShoppingCart> cartItems;
            if (UserId != 0)
            {
                cartItems = db.ShoppingCarts.Include(a => a.ProductPrice.Product).Where(c => c.UserId == UserId).ToList();
            }
            else
            {
                CartCookie cartCookie = new CartCookie(contextAccessor);
                cartItems = db.ShoppingCarts.Include(a => a.ProductPrice.Product)
                    .Where(c => c.SessionId == cartCookie.GetCartCookie() && c.UserId == null).ToList();
            }

            foreach (var item in cartItems)
            {
                imageService.FixImageUrls(item.ProductPrice.Product);
            }
            return PartialView("ShoppingBag", cartItems);
        }

        [HttpPost]
        public async Task<ActionResult> AddToCart(int id, int count = 1)
        {
            await orderService.AddToCart(id, count);
            return Json(new { success = true, count = await orderService.GetShoppingCartCount() });
        }

        [HttpPost]
        public async Task<ActionResult> RemoveFromCart(int id)
        {
            var cartItem = db.ShoppingCarts.FirstOrDefault(c => c.Id == id);
            if (cartItem != null)
            {
                db.ShoppingCarts.Remove(cartItem);
                db.SaveChanges();
            }
            return Json(new { success = true, count = await orderService.GetShoppingCartCount() });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCart(int id, int count)
        {
            var cartItem = db.ShoppingCarts.Include(a => a.ProductPrice).FirstOrDefault(c => c.Id == id);
            cartItem.Count = count;
            db.SaveChanges();
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
            db.ShoppingCarts.Where(c => c.Id == id).Delete();
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

            decimal postCost = OrderService.DeliveryPrice;
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
        public async Task<IActionResult> Checkout(CheckOutVM vm)
        {
            decimal postCost = 10000;

            var cartItems = await GetCartItems();
            if (cartItems.Count() == 0)
            {
                return Json(new { success = false, msg = "سبد خرید خالی می باشد" });
            }

            if (string.IsNullOrEmpty(vm.UserData.FirstName))
            {
                return Json(new { success = false, msg = "لطفا نام را وارد کنید" });
            }
            if (string.IsNullOrEmpty(vm.UserData.LastName))
            {
                return Json(new { success = false, msg = "لطفا نام خانوادگی را وارد کنید" });
            }

            if (!string.IsNullOrEmpty(vm.UserData.EmailAddress) && !Util.IsValidEmailaddress(vm.UserData.EmailAddress))
            {
                return Json(new { success = false, msg = "لطفا ایمیل را به صورت صحیح وارد کنید" });
            }
            User user = db.Users.FirstOrDefault(a => a.Id == UserId);
            user.FirstName = vm.UserData.FirstName;
            user.EmailAddress = vm.UserData.EmailAddress;
            user.LastName = vm.UserData.LastName;
            user.NationalCode = vm.UserData.NationalCode;

            if (!vm.IsNewAddress && vm.UserData.AddressId == null)
            {
                return Json(new { success = false, msg = "لطفا آدرس را انتخاب نمایید" });
            }

            long addressId;

            if (vm.IsNewAddress)
            {
                if (vm.UserAddress.CityId == 0)
                {
                    return Json(new { success = false, msg = "لطفا شهر را انتخاب نمایید" });
                }
                if (string.IsNullOrEmpty(vm.UserAddress.Details))
                {
                    return Json(new { success = false, msg = "لطفا آدرس را وارد نمایید" });
                }
                if (string.IsNullOrEmpty(vm.UserAddress.PostalCode))
                {
                    return Json(new { success = false, msg = "لطفا کد پستی را وارد نمایید" });
                }

                var ua = new UserAddress();
                ua.CityId = vm.UserAddress.CityId;
                ua.Title = vm.UserAddress.Title;
                ua.PostalCode = vm.UserAddress.PostalCode;
                ua.Details = vm.UserAddress.Details;
                ua.UserId = user.Id;
                db.Addresses.Add(ua);
                await db.SaveChangesAsync();
                addressId = ua.Id;
            }
            else
            {
                addressId = vm.UserData.AddressId.Value;
            }

            var order = await orderService.Checkout(addressId);

            var payment = await orderService.InitPayment(order, pathService.AppBaseUrl + "/Payment/CallBackPay");
            string payUrl = pathService.AppBaseUrl + $"/Payment/Pay?pid={payment.Id}&uid={payment.UniqueId}&source=api";
            return Json(new { success = true, msg = payUrl });
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
            var lst = db.Addresses.Where(a => a.UserId == UserId).Select(a => new { a.Id, Name = (a.City.Province.Name + "-" + a.City.Name + "-" + a.Details) });
            return Json(lst);
        }
    }
}