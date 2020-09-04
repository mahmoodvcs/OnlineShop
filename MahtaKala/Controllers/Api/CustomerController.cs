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
        private readonly PaymentService paymentService;
        private readonly IProductImageService productImageService;
        private readonly IPathService pathService;
        private readonly OrderService orderService;

        public CustomerController(DataContext context,
            ILogger<CustomerController> logger,
            PaymentService paymentService,
            IProductImageService productImageService,
            IPathService pathService,
            OrderService orderService)
            : base(context, logger)
        {
            this.paymentService = paymentService;
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
            var prevOrder = await orderService.GetUserOrder();
            OrderItem item;
            if (addToCart.Id > 0)
            {
                item = await db.OrderItems.FirstOrDefaultAsync(a => a.Order.UserId == UserId && a.Id == addToCart.Id);
                if (item == null)
                    throw new EntityNotFoundException<OrderItem>(addToCart.Id);
                if (item.OrderId != prevOrder?.Id)
                {
                    //TODO: Debug log
                    logger.LogCritical("Order does not exist " + item.OrderId);
                    throw new ApiException(500, Messages.Messages.Order.OrderItemDoesNotBelongToOrder);
                }
            }
            else
            {
                item = new OrderItem();

                if (prevOrder == null)
                {
                    Order order = new Order()
                    {
                        UserId = UserId,
                        State = OrderState.Initial
                    };
                    item.Order = order;
                }
                else
                    item.OrderId = prevOrder.Id;

                db.OrderItems.Add(item);
            }

            var productPrice = db.ProductPrices.First(a => a.ProductId == addToCart.Product_Id);
            if (productPrice.Price == 0)
            {
                var prodName = await db.Products.Where(a => a.Id == productPrice.ProductId).Select(a => a.Title).FirstOrDefaultAsync();
                throw new ApiException(400, string.Format(Messages.Messages.Order.ProductDoesNotExistInStore, prodName));
            }
            item.ProductPriceId = productPrice.Id;
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
        public async Task<CartModel> Cart()
        {
            var order = await orderService.GetUserOrder();
            if (order == null || order.Items.Count == 0)
            {
                return new CartModel
                {
                    Items = new List<CartItemModel>(),
                    DeliveryPrice = 0,
                    TotlaPrice = 0
                };
            }
            var priceIds = order.Items.Select(a => a.ProductPriceId);
            var products = db.ProductPrices.Where(a => priceIds.Contains(a.Id))
                .Select(a => new
                {
                    a.Id,
                    a.Product.Title,
                    a.ProductId,
                    a.Product.Thubmnail
                }).ToDictionary(a => a.Id);

            var data = order.Items.Select(b => new CartItemModel
            {
                Id = b.Id,
                Product_Id = b.ProductPrice.ProductId,
                Quantity = b.Quantity,
                CharacteristicValues = b.CharacteristicValues,
                Thumbnail = productImageService.GetImageUrl(products[b.ProductPriceId].ProductId, products[b.ProductPriceId].Thubmnail),
                Title = products[b.ProductPriceId].Title,
                Price = b.UnitPrice
            }).ToList();

            return new CartModel
            {
                Items = data,
                DeliveryPrice = paymentService.GetDeliveryPrice(order),
                TotlaPrice = paymentService.CalculateTotalPrice(order)
            };
        }

        /// <summary>
        /// قلم را از سبد خرید حذف میکند
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<StatusCodeResult> RemoveFromCart(long id)
        {
            //TODO: orderService.GetUserOrder();
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
            var order = await orderService.GetUserOrder();

            if (order == null || order.Items.Count == 0)
            {
                throw new ApiException(400, Messages.Messages.Order.EmptyCart);
            }

            order.CheckOutData = DateTime.Now;
            //TODO: check the address
            order.AddressId = checkoutModel.AddressId;
            order.TotalPrice = paymentService.CalculateTotalPrice(order);
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
                    a.SentDateTime,
                    State = a.State
                }).ToListAsync();

            return data.Select(a => new BuyHistoryModel
            {
                Id = a.Id,
                CheckoutDate = Util.GetPersianDate(a.CheckOutData),
                SendDate = Util.GetPersianDate(a.SentDateTime),
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