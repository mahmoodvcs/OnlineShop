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
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Index.Bintree;
using Z.EntityFramework.Plus;

namespace MahtaKala.Controllers
{

    public class CartController : SiteControllerBase<CartController>
    {
        private readonly PaymentService paymentService;
        private readonly IPathService pathService;

        public CartController(DataContext dataContext, ILogger<CartController> logger, IHttpContextAccessor contextAccessor,
            PaymentService paymentService,
            IPathService pathService
            ) : base(dataContext, logger)
        {
            this.contextAccessor = contextAccessor;
            this.paymentService = paymentService;
            this.pathService = pathService;
        }
        private readonly IHttpContextAccessor contextAccessor;
        public HttpContext Current => contextAccessor.HttpContext;

        public IActionResult Index()
        {
            List<ShppingCart> cartItems = GetCartItems();
            return View(cartItems);
        }

        public ActionResult ShoppingBag()
        {
            string sessionId = Current.Session.Id;
            List<ShppingCart> cartItems;
            if (UserId != 0)
            {
                cartItems = db.ShppingCarts.Include(a => a.ProductPrice.Product).Where(c => c.UserId == UserId).ToList();
            }
            else
            {
                cartItems = db.ShppingCarts.Include(a => a.ProductPrice.Product).Where(c => c.SessionId == sessionId).ToList();
            }
            return PartialView("ShoppingBag", cartItems);
        }

        [HttpPost]
        public ActionResult AddToCart(int id, int count = 1)
        {
            string sessionId = Current.Session.Id;
            ShppingCart cartItem;
            if (UserId > 0)
            {
                cartItem = db.ShppingCarts.FirstOrDefault(c => c.UserId == UserId && c.ProductPriceId == id);
            }
            else
            {
                cartItem = db.ShppingCarts.FirstOrDefault(c => c.SessionId == sessionId && c.ProductPriceId == id);
            }

            if (cartItem == null)
            {
                cartItem = new ShppingCart();
                cartItem.ProductPriceId = id;
                if (UserId > 0)
                    cartItem.UserId = UserId;
                else
                    cartItem.SessionId = sessionId;
                cartItem.Count = 0;
                cartItem.DateCreated = DateTime.Now;
                db.ShppingCarts.Add(cartItem);
            }
            cartItem.Count += count;
            db.SaveChanges();
            return Json(new { success = true, count = GetShoppingCartCount() });
        }

        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            var cartItem = db.ShppingCarts.FirstOrDefault(c => c.Id == id);
            if (cartItem != null)
            {
                db.ShppingCarts.Remove(cartItem);
                db.SaveChanges();
            }
            return Json(new { success = true, count = GetShoppingCartCount() });
        }

        [HttpPost]
        public ActionResult UpdateCart(int id, int count)
        {
            var cartItem = db.ShppingCarts.Include(a => a.ProductPrice).FirstOrDefault(c => c.Id == id);
            cartItem.Count = count;
            db.SaveChanges();
            var finalcostRow = Util.Sub3Number(count * cartItem.ProductPrice.DiscountPrice);
            List<ShppingCart> cartItems = GetCartItems();
            var sumPrice = Util.Sub3Number(cartItems.Sum(a => a.ProductPrice.Price) * cartItems.Sum(a => a.Count));
            var sumFinalPrice = Util.Sub3Number(cartItems.Sum(a => a.ProductPrice.DiscountPrice) * cartItems.Sum(a => a.Count));
            return Json(new { success = true, count = cartItems.Sum(a => a.Count), id, finalcostRow, sumPrice, sumFinalPrice });
        }

        [HttpPost]
        public ActionResult DeleteItemCart(int id)
        {
            db.ShppingCarts.Where(c => c.Id == id).Delete();
            List<ShppingCart> cartItems = GetCartItems();
            var sumPrice = Util.Sub3Number(cartItems.Sum(a => a.ProductPrice.Price) * cartItems.Sum(a => a.Count));
            var sumFinalPrice = Util.Sub3Number(cartItems.Sum(a => a.ProductPrice.DiscountPrice) * cartItems.Sum(a => a.Count));
            return Json(new { success = true, count = cartItems.Sum(a => a.Count), id, sumPrice, sumFinalPrice });
        }



        public IActionResult Checkout()
        {
            var user = User;
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var getCartItems = GetCartItems();
            if (getCartItems.Count() == 0)
            {
                return RedirectToAction("Category", "home");
            }

            decimal postCost = 10000;
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

            var getCartItems = GetCartItems();
            if (getCartItems.Count() == 0)
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

            User user = db.Users.FirstOrDefault(a => a.Id == UserId);
            user.FirstName = vm.UserData.FirstName;
            user.EmailAddress = vm.UserData.EmailAddress;
            user.LastName = vm.UserData.LastName;
            user.NationalCode = vm.UserData.NationalCode;

            if (!vm.IsNewAddress && vm.UserData.AddressId == null)
            {
                return Json(new { success = false, msg = "لطفا آدرس را انتخاب نمایید" });
            }


            Order order = db.Orders.Include(a => a.Items).Where(a => a.UserId == user.Id && a.State == OrderState.Initial).FirstOrDefault();
            if (order == null)
            {
                order = new Order();
                order.State = OrderState.Initial;
                order.UserId = user.Id;
                db.Orders.Add(order);
            }
            if (vm.IsNewAddress)
            {
                if (vm.UserAddress.CityId==0)
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
                order.Address = ua;
            }
            else
            {
                order.AddressId = vm.UserData.AddressId;
            }
        
            order.OrrderDate = DateTime.Now;
            db.OrderItems.Where(a => a.OrderId == order.Id).Delete();
            decimal sumFinalPrice = 0;
            foreach (var item in getCartItems)
            {
                OrderItem orderItem = new OrderItem();
                orderItem.FinalPrice = item.ProductPrice.DiscountPrice * item.Count;
                orderItem.Order = order;
                orderItem.UnitPrice = item.ProductPrice.DiscountPrice;
                orderItem.ProductPriceId = item.ProductPriceId;
                orderItem.Quantity = item.Count;
                db.OrderItems.Add(orderItem);
                sumFinalPrice += orderItem.FinalPrice;
            }
            order.TotalPrice = sumFinalPrice + postCost;
            db.SaveChanges();

            var payment = await paymentService.InitPayment(order, pathService.AppBaseUrl + "/Payment/CallBackPay");
            string payUrl = pathService.AppBaseUrl + $"/Payment/Pay?pid={payment.Id}&uid={payment.UniqueId}&source=api";
            return Json(new { success = true, msg = payUrl });
        }




        [NonAction]
        private int GetShoppingCartCount()
        {
            if (UserId != 0)
            {
                return db.ShppingCarts.Where(a => a.UserId == UserId).Sum(a => a.Count);
            }
            else
            {
                string sessionId = Current.Session.Id;
                return db.ShppingCarts.Where(a => a.SessionId == sessionId).Sum(a => a.Count);
            }
        }

        [NonAction]
        private List<ShppingCart> GetCartItems()
        {
            List<ShppingCart> cartItems;
            if (UserId != 0)
            {
                cartItems = db.ShppingCarts.Include(a => a.ProductPrice.Product).Where(c => c.UserId == UserId).ToList();
            }
            else
            {
                string sessionId = Current.Session.Id;
                cartItems = db.ShppingCarts.Include(a => a.ProductPrice.Product).Where(c => c.SessionId == sessionId).ToList();
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