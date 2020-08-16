using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.Models;
using MahtaKala.Models.CustomerModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MahtaKala.Controllers.Api
{
    [ApiController()]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1")]
    public class CustomerController : ControllerBase
    {
        private readonly DataContext db;
        public CustomerController(DataContext context)
        {
            this.db = context;
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Wishlist([FromBody]UpdateWishlistRequest updateWishlistRequest)
        {
            if (!db.Wishlists.Any(a=>a.UserId==UserId && a.ProductId==updateWishlistRequest.ProductId))
            {
                Wishlist wishlist = new Wishlist();
                wishlist.ProductId = updateWishlistRequest.ProductId;
                wishlist.UserId = UserId;
                db.Wishlists.Add(wishlist);
                await db.SaveChangesAsync();
            }
            return StatusCode(200);
        }

        [Authorize]
        [HttpGet]
        public async Task<List<WishlistModels>> Wishlist([FromQuery]PagerModel pagerModel)
        {
            var list = db.Wishlists.Where(a => a.UserId == UserId).Skip(pagerModel.Offset).Take(pagerModel.Page);
            return await list.Select(a => new WishlistModels
            {
                Id = a.Id,
                Title = ""
            }).ToListAsync();
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Wishlist([FromBody]IdModel idModel)
        {
            var wishlists = await db.Wishlists.FirstOrDefaultAsync(c => c.Id == idModel.Id);
            if (wishlists == null)
            {
                throw new Exception("Wishlist Not Found.");
            }
            db.Wishlists.Remove(wishlists);
            await db.SaveChangesAsync();
            return StatusCode(200);

        }


        /// <summary>
        /// Update the Existing Bascket or Create New One
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Success. The Bascket was updated.</response>
        /// <response code="201">Success. Bascket was new.</response>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Basket([FromBody]BasketModel updateBasketRequest)
        {
            Basket basket = null;
            bool newBasket = false;
            if (updateBasketRequest.Id == 0)
            {
                newBasket = true;
                basket = new Basket();
                db.Baskets.Add(basket);
            }
            else
            {
                basket = await db.Baskets.FirstOrDefaultAsync(b => b.Id == updateBasketRequest.Id);
                if (basket == null)
                    throw new Exception("Basket Not Found.");
            }

            basket.ProductId = updateBasketRequest.ProductId;
            basket.Date = updateBasketRequest.Date;
            basket.Quantity = updateBasketRequest.Qty;
            basket.Price = updateBasketRequest.Price;

            await db.SaveChangesAsync();
            if (newBasket)
                return StatusCode(200);
            else
                return StatusCode(201);
        }
        /// <summary>
        /// Return the List of Baskets with the given offset and page
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<List<BasketModel>> Basket([FromQuery]PagerModel pagerModel)
        {
            return await db.Baskets.OrderBy(b => b.Id)
                .Skip(pagerModel.Offset)
                .Take(pagerModel.Page)
                .Select(b => new BasketModel
                {
                    Id = b.Id,
                    ProductId = b.ProductId,
                    Date = b.Date,
                    Price = b.Price,
                    Qty = b.Quantity
                }).ToListAsync();
        }
        /// <summary>
        /// Removes the Basket with the given ID
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Success. The Basket was Deleted.</response>
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Basket([FromBody]IdModel idModel)
        {
            var basket = await db.Baskets.FirstOrDefaultAsync(b => b.Id == idModel.Id);
            if (basket == null)
            {
                throw new Exception("Basket Not Found.");
            }
            db.Baskets.Remove(basket);
            await db.SaveChangesAsync();
            return StatusCode(200);

        }
    }
}