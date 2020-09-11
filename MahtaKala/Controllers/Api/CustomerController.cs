﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.Entities.Extentions;
using MahtaKala.Infrustructure;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Models;
using MahtaKala.SharedServices;
using MahtaKala.Models.CustomerModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Serilog.Core;
using MahtaKala.Messages;
using MahtaKala.GeneralServices.Payment;
using MahtaKala.Services;
using MahtaKala.Helpers;
using MahtaKala.Models.Payment;

namespace MahtaKala.Controllers.Api
{
    [ApiController()]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1")]
    [Authorize]
    public class CustomerController : ApiControllerBase<CustomerController>
    {
        private readonly IProductImageService productImageService;
        private readonly IPathService pathService;
        private readonly OrderService orderService;

        public CustomerController(DataContext context,
            ILogger<CustomerController> logger,
            IProductImageService productImageService,
            IPathService pathService,
            OrderService orderService)
            : base(context, logger)
        {
            this.productImageService = productImageService;
            this.pathService = pathService;
            this.orderService = orderService;
        }


        [HttpPost]
        public async Task<IActionResult> Wishlist([FromBody] UpdateWishlistRequest updateWishlistRequest)
        {
            if (!db.Wishlists.Any(a => a.UserId == UserId && a.ProductId == updateWishlistRequest.ProductId))
            {
                Wishlist wishlist = new Wishlist();
                wishlist.ProductId = updateWishlistRequest.ProductId;
                wishlist.UserId = UserId;
                db.Wishlists.Add(wishlist);
                await db.SaveChangesAsync();
            }
            return StatusCode(200);
        }

        [HttpGet]
        public async Task<List<WishlistModels>> Wishlist([FromQuery] PagerModel pagerModel)
        {
            var list = db.Wishlists.Where(a => a.UserId == UserId).OrderBy(p => p.Id).Page(pagerModel);
            return await list.Select(a => new WishlistModels
            {
                Id = a.Id,
                Title = a.Product.Title
            }).ToListAsync();
        }

        [HttpDelete]
        public async Task<IActionResult> Wishlist([FromBody] IdModel idModel)
        {
            var wishlists = await db.Wishlists.FirstOrDefaultAsync(c => c.Id == idModel.Id);
            if (wishlists == null)
            {
                throw new EntityNotFoundException<Wishlist>(idModel.Id);
            }
            db.Wishlists.Remove(wishlists);
            await db.SaveChangesAsync();
            return StatusCode(200);
        }

        /// <summary>
        /// قلمی را به سبد خرید اضافه میکند و یا قلم موجود را ویرایش میکند. برای ویرایش قلم(مثلا تغییر تعداد) باید فیلد Id ارسال شود
        /// </summary>
        /// <param name="addToCart"></param>
        /// <returns>شناسه ی آیتم در سبد خرید را برمیگرداند. برای ویرایش یا حذف قلم از سبد مورد نیاز است</returns>
        [HttpPost]
        public async Task<long> AddToCart([FromBody] AddToCartModel addToCart)
        {
            var productPrice = db.ProductPrices.First(a => a.ProductId == addToCart.Product_Id);
            return await orderService.UpdateCart(productPrice.Id, addToCart.Quantity);
        }

        /// <summary>
        /// اقلام موجود در سبد خرید کاربر را برمیگرداند
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<CartModel> Cart()
        {
            var items = await orderService.GetCartQuery()
                .Select(a => new CartItemModel
                {
                    Id = a.Id,
                    Product_Id = a.ProductPrice.ProductId,
                    CharacteristicValues = null,
                    Quantity = a.Count,
                    Price = a.ProductPrice.DiscountPrice,
                    Thumbnail = a.ProductPrice.Product.Thubmnail,
                    Title = a.ProductPrice.Product.Title
                }).ToListAsync();
            foreach (var item in items)
            {
                item.Thumbnail = productImageService.GetImageUrl(item.Product_Id, item.Thumbnail);
            }

            return new CartModel
            {
                Items = items,
                DeliveryPrice = OrderService.DeliveryPrice,
                TotlaPrice = items.Sum(a=>a.Quantity * a.Price) + OrderService.DeliveryPrice
            };
        }

        /// <summary>
        /// سبد خرید را خالی میکند
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EmptyCart()
        {
            await orderService.EmptyCart();
            return Ok();
        }

        /// <summary>
        /// قلم را از سبد خرید حذف میکند
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<StatusCodeResult> RemoveFromCart(long id)
        {
            await orderService.RemoveFromCart(id);
            return Ok();
        }


        /// <summary>
        /// سبد خرید را نهایی میکند. کاربر باید به صفحه ی پرداخت بانک هدایت شود.
        /// موجودی کالا نیز باید به طور موقت رززو شود
        /// </summary>
        /// <param name="checkoutModel">آدرس انتخابی کاربر باید ارسال شود</param>
        /// <returns>شناسه ی سفارش و آدرس انتقال کاربر برای پرداخت را برمیگرداند</returns>
        [HttpPost]
        public async Task<CheckoutResponseModel> Checkout(CheckoutModel checkoutModel)
        {
            var order = await orderService.Checkout(checkoutModel.AddressId);

            var payment = await orderService.InitPayment(order, pathService.AppBaseUrl + "/Payment/Paid?source=api");
            string payUrl = pathService.AppBaseUrl + $"/Payment/Pay?pid={payment.Id}&uid={payment.UniqueId}&source=api";

            return new CheckoutResponseModel
            {
                OrderId = order.Id,
                PaymentUrl = payUrl
            };
        }

        [HttpGet]
        public async Task<IEnumerable<BuyHistoryModel>> Orders()
        {
            var data = await db.Orders.Where(o => o.State == OrderState.Paid ||
                                      o.State == OrderState.Delivered ||
                                      o.State == OrderState.Sent)
                .Where(a => a.UserId == UserId)
                .Select(a => new
                {
                    Id = a.Id,
                    Price = a.TotalPrice,
                    a.CheckOutData,
                    a.ApproximateDeliveryDate,
                    a.ActualDeliveryDate,
                    a.SendDate,
                    State = a.State
                }).ToListAsync();

            return data.Select(a => new BuyHistoryModel
            {
                Id = a.Id,
                CheckoutDate = Util.GetPersianDate(a.CheckOutData),
                ApproximateDeliveryDate = Util.GetPersianDate(a.ApproximateDeliveryDate),
                SendDate = Util.GetPersianDate(a.SendDate),
                ActualDeliveryDate = Util.GetPersianDate(a.ActualDeliveryDate),
                Price = (long)a.Price,
                State = TranslateExtentions.GetTitle(a.State)
            });
        }

        //[HttpGet]
        //public async Task<PaymentStatusResponse> PayStatus(long orderId)
        //{
        //    db.Payments.Where(p => p.OrderId == orderId).Where(;

        //}
    }
}