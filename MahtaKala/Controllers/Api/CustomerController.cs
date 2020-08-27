using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.Entities.Extentions;
using MahtaKala.Infrustructure;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Models;
using MahtaKala.Models.CustomerModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Serilog.Core;
using MahtaKala.Messages;
using MahtaKala.GeneralServices.Payment;
using MahtaKala.SharedServices;

namespace MahtaKala.Controllers.Api
{
    [ApiController()]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1")]
    [Authorize]
    public class CustomerController : ApiControllerBase<CustomerController>
    {
        private readonly PaymentService paymentService;
        private readonly IPathService pathService;

        public CustomerController(DataContext context, ILogger<CustomerController> logger, PaymentService paymentService,
            IPathService pathService)
            : base(context, logger)
        {
            this.paymentService = paymentService;
            this.pathService = pathService;
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
            var prevOrderId = await db.Orders.Where(a => a.UserId == UserId && a.State == OrderState.Initial)
                .Select(a => a.Id).FirstOrDefaultAsync();
            OrderItem item;
            if (addToCart.Id > 0)
            {
                item = await db.OrderItems.FirstOrDefaultAsync(a => a.Order.UserId == UserId && a.Id == addToCart.Id);
                if (item == null)
                    throw new EntityNotFoundException<OrderItem>(addToCart.Id);
                if (item.OrderId != prevOrderId)
                {
                    //TODO: Debug log
                    logger.LogCritical("Order does not exist " + item.OrderId);
                    throw new ApiException(500, Messages.Messages.Order.OrderItemDoesNotBelongToOrder);
                }
            }
            else
            {
                item = new OrderItem();

                if (prevOrderId == 0)
                {
                    Order order = new Order()
                    {
                        UserId = UserId,
                        State = OrderState.Initial
                    };
                    item.Order = order;
                }
                else
                    item.OrderId = prevOrderId;

                db.OrderItems.Add(item);
            }

            item.ProductId = addToCart.Product_Id;
            item.Quantity = addToCart.Quantity;
            item.CharacteristicValues = addToCart.CharacteristicValues;

            await db.SaveChangesAsync();
            return item.Id;
        }

        /// <summary>
        /// اقلام موجود در سبد خرید کاربر را برمیگرداند
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<CartItemModel>> Cart()
        {

            var items = await db.OrderItems.Where(a => a.Order.UserId == UserId && a.Order.State == OrderState.Initial).ToListAsync();

            return items.Select(b => new CartItemModel
            {
                Id = b.Id,
                Product_Id = b.ProductId,
                Quantity = b.Quantity,
                CharacteristicValues = b.CharacteristicValues,
                Thumbnail = b.Product.Thubmnail,
                Title = b.Product.Title,
                Price = b.UnitPrice
            }).ToList();
        }

        /// <summary>
        /// قلم را از سبد خرید حذف میکند
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<StatusCodeResult> RemoveFromCart(long id)
        {
            var item = await db.OrderItems.FirstOrDefaultAsync(a => a.Order.UserId == UserId && a.Order.State == OrderState.Initial && a.Id == id);
            if (item == null)
                //TODO: Debug log
                throw new EntityNotFoundException<OrderItem>(id);

            db.OrderItems.Remove(item);
            await db.SaveChangesAsync();
            return StatusCode(200);
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
            var order = await db.Orders
                .Include(a => a.Items)
                .Where(a => a.UserId == UserId && a.State == OrderState.Initial)
                .FirstOrDefaultAsync();

            if(order == null || order.Items.Count == 0)
            {
                throw new ApiException(400, Messages.Messages.Order.EmptyCart);
            }

            order.OrrderDate = DateTime.Now;
            //TODO: check the address
            order.AddressId = checkoutModel.AddressId;
            order.TotalPrice = order.Items.Sum(a => a.UnitPrice * a.Quantity);
            order.State = OrderState.CheckedOut;
            await db.SaveChangesAsync();

            var payment = await paymentService.InitPayment(order, pathService.AppBaseUrl + "/Payment/Paid?source=api");
            string payUrl = pathService.AppBaseUrl + $"/Payment/Pay?pid={payment.Id}&uid={payment.UniqueId}&source=api";

            return new CheckoutResponseModel
            {
                OrderId = order.Id,
                PaymentUrl = payUrl
            };
        }


    }
}