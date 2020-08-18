using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.Entities.Extentions;
using MahtaKala.Models;
using MahtaKala.Models.CustomerModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace MahtaKala.Controllers.Api
{
    [ApiController()]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1")]
    public class CustomerController : ApiControllerBase<CustomerController>
    {
        public CustomerController(DataContext context, ILogger<CustomerController> logger)
            : base(context, logger)
        {
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
            var list = db.Wishlists.Where(a => a.UserId == UserId).OrderBy(p => p.Id).Page(pagerModel);
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
        /// Update the Existing Basket or Create New One
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Success. The Backet was updated.</response>
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

            basket.ProductId = updateBasketRequest.Product_Id;
            basket.Date = updateBasketRequest.Date;
            basket.Quantity = updateBasketRequest.Qty;
            basket.Price = updateBasketRequest.Price;
            basket.CharacteristicName = updateBasketRequest.Characteristic_Name;
            basket.CharacteristicValue = updateBasketRequest.Characteristic_Value;

            await db.SaveChangesAsync();
            return StatusCode(200);
        }

        /// <summary>
        /// Return the List of Baskets with the given offset and page
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<List<BasketModel>> Basket([FromQuery]PagerModel pagerModel)
        {
            return await db.Baskets
                .OrderBy(b => b.Id)
                .Page(pagerModel)
                .Select(b => new BasketModel
                {
                    Id = b.Id,
                    Product_Id = b.ProductId,
                    Date = b.Date,
                    Price = b.Price,
                    Qty = b.Quantity,
                    Characteristic_Name = b.CharacteristicName,
                    Characteristic_Value = b.CharacteristicValue
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