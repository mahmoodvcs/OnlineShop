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
    }
}