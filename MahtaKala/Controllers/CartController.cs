using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Z.EntityFramework.Plus;

namespace MahtaKala.Controllers
{

    public class CartController : SiteControllerBase<CartController>
    {

        public CartController(DataContext dataContext, ILogger<CartController> logger, IHttpContextAccessor contextAccessor) : base(dataContext, logger)
        {
            this.contextAccessor = contextAccessor;
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
            var cartItem = db.ShppingCarts.Include(a=>a.ProductPrice).FirstOrDefault(c => c.Id == id);
            cartItem.Count = count;
            db.SaveChanges();
            var finalcostRow = Util.Sub3Number(count * cartItem.ProductPrice.DiscountPrice);
            List<ShppingCart> cartItems = GetCartItems();
            var sumPrice = Util.Sub3Number(cartItems.Sum(a=>a.ProductPrice.Price) * cartItems.Sum(a => a.Count));
            var sumFinalPrice = Util.Sub3Number(cartItems.Sum(a => a.ProductPrice.DiscountPrice) * cartItems.Sum(a => a.Count));
            return Json(new { success = true, count = cartItems.Sum(a=>a.Count), id, finalcostRow, sumPrice , sumFinalPrice });
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

     

        public IActionResult UserAddress()
        {
            if(UserId==0)
            {
                return RedirectToAction("Login", "Account");
            }
            if(GetShoppingCartCount()==0)
            {
                return RedirectToAction("Category", "home");
            }
            return View();
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
    }
}