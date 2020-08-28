﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.Infrustructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            string sessionId = Current.Session.Id;
            var cartItems = db.ShppingCarts.Include(a => a.ProductPrice.Product).Where(c => c.SessionId == sessionId).ToList();
            if (UserId != 0)
            {
                cartItems = db.ShppingCarts.Include(a => a.ProductPrice.Product).Where(c => c.UserId == UserId).ToList();
            }
            return View(cartItems);
        }

        [HttpPost]
        public ActionResult AddToCart(int id, int count = 1)
        {
            string sessionId = Current.Session.Id;
            bool isAnyUser = UserId != 0;
            var cartItem = db.ShppingCarts.FirstOrDefault(c => c.SessionId == sessionId && c.ProductPriceId == id);
            if (isAnyUser)
            {
                cartItem = db.ShppingCarts.FirstOrDefault(c => c.UserId == UserId && c.ProductPriceId == id);
            }
            if (cartItem == null)
            {
                cartItem = new ShppingCart();
                cartItem.ProductPriceId = id;
                if (isAnyUser)
                    cartItem.UserId = UserId;
                else
                    cartItem.SessionId = sessionId;
                cartItem.Count = 0;
                cartItem.DateCreated = DateTime.Now;
                db.ShppingCarts.Add(cartItem);
            }
            cartItem.Count++;
            db.SaveChanges();
            return Json(new { success = true, count = GetShoppingCartCount() });
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
    }
}