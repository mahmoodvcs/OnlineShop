using MahtaKala.Entities;
using MahtaKala.GeneralServices.Payment;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders.Physical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace MahtaKala.Services
{
    public class OrderService
    {
        public const int DeliveryPrice = 120000;

        private readonly ICurrentUserService currentUserService;
        private readonly DataContext db;
        private readonly IBankPaymentService bankService;

        public OrderService(
            ICurrentUserService currentUserService,
            DataContext dataContext,
            IBankPaymentService bankService
            )
        {
            this.currentUserService = currentUserService;
            this.db = dataContext;
            this.bankService = bankService;
        }
        User User => currentUserService.User;
        public async Task<Order> GetUserOrder()
        {
            var list = await db.Orders
                .Include(a => a.Items)
                .Where(a => a.UserId == currentUserService.User.Id && (a.State == OrderState.Initial || a.State == OrderState.CheckedOut))
                .ToListAsync();
            if (list.Count == 0)
                return null;
            var order = list.FirstOrDefault(a => a.State == OrderState.Initial);
            if (order != null)
            {
                foreach (var o in list.Where(a => a.State == OrderState.CheckedOut))
                {
                    o.State = OrderState.Canceled;
                }
                await db.SaveChangesAsync();
                return order;
            }
            order = list.First();
            if (order.State == OrderState.CheckedOut)
            {
                order.State = OrderState.Initial;
            }
            foreach (var o in list.Where(o => o.State == OrderState.CheckedOut))
            {
                o.State = OrderState.Canceled;
            }
            await db.SaveChangesAsync();

            return order;
        }

        public async Task<long> AddToCart(long productPriceId, int count = 1)
        {
            var cart = GetCartQuery();
            var cartItem = await cart.FirstOrDefaultAsync(c => c.ProductPriceId == productPriceId);
            if (cartItem != null)
                count += cartItem.Count;
            return await UpdateCart(cartItem, productPriceId, count);
        }
        public async Task<ShoppingCart> UpdateCartItem(long shoppingCartId, int count)
        {
            var cart = GetCartQuery();
            var cartItem = await cart.FirstOrDefaultAsync(c => c.Id == shoppingCartId);
            await UpdateCart(cartItem, cartItem.ProductPriceId, count);
            return cartItem;
        }
        public async Task<long> UpdateCart(long productPriceId, int count)
        {
            var cart = GetCartQuery();
            var cartItem = await cart.FirstOrDefaultAsync(c => c.ProductPriceId == productPriceId);
            return await UpdateCart(cartItem, productPriceId, count);
        }

        private async Task CheckProductMaxQuota(long priceId, int count, ProductInfo prod)
        {
            if (prod.MaxBuyQuota == null)
                return;
            if (count > prod.MaxBuyQuota)
                throw new ApiException(400, string.Format(Messages.Messages.Order.CannotAddProduct_MaxQuota,
                    Util.GetTimeSpanPersianString(TimeSpan.FromDays(prod.BuyQuotaDays.Value)), prod.MaxBuyQuota));

            var minTime = DateTime.Now.AddDays(-prod.BuyQuotaDays.Value);
            var userId = currentUserService.User?.Id;
            if (userId != null)
            {
                var prevProductCount = await
                    (from order in db.Orders
                            .Where(o => o.UserId == userId
                                && (o.State == OrderState.Paid || o.State == OrderState.Sent || o.State == OrderState.Delivered)
                                && o.CheckOutData > minTime)
                     from orderItem in order.Items.Where(a => a.ProductPriceId == priceId)
                     select orderItem.Quantity).SumAsync();
                if (prevProductCount + count > prod.MaxBuyQuota)
                {
                    //var prodName = await db.ProductPrices.Where(a => a.Id == productPriceId).Select(a => a.Product.Title).FirstOrDefaultAsync();
                    throw new ApiException(400, string.Format(Messages.Messages.Order.CannotAddProduct_MaxQuota,
                        Util.GetTimeSpanPersianString(TimeSpan.FromDays(prod.BuyQuotaDays.Value)), prod.MaxBuyQuota));
                }
            }
        }

        private async Task<long> UpdateCart(ShoppingCart cartItem, long productPriceId, int count)
        {
            ProductInfo info = await GetProductInfo(cartItem, productPriceId);
            await CheckProductMaxQuota(productPriceId, count, info);
            //if (count < info.MinBuyQuota)
            //    throw new ApiException(400, Messages.Messages.Order.CannotAddProduct_NotAvailable);

            var cart = GetCartQuery();
            if (cartItem == null)
            {
                if (info.Status != ProductStatus.Available)
                    throw new ApiException(400, Messages.Messages.Order.CannotAddProduct_NotAvailable);

                var priceIds = cart.Select(a => a.ProductPriceId);
                if (db.ProductPrices.Where(a => priceIds.Contains(a.Id) && a.Product.Seller.Basket != info.Basket).Any())
                    throw new ApiException(412, Messages.Messages.Order.CannotAddProduct_DefferentSeller);


                cartItem = new ShoppingCart
                {
                    ProductPriceId = productPriceId,
                    Count = count,
                    SessionId = currentUserService.AnonymousSessionId,
                    DateCreated = DateTime.Now,
                };
                if (User != null)
                    cartItem.UserId = User.Id;
                else
                    cartItem.SessionId = currentUserService.AnonymousSessionId;

                db.ShoppingCarts.Add(cartItem);
            }

            cartItem.Count = count;
            await db.SaveChangesAsync();
            return cartItem.Id;
        }

        private async Task<ProductInfo> GetProductInfo(ShoppingCart cartItem, long productPriceId)
        {
            ProductInfo info;
            if (cartItem == null)
            {
                info = await db.ProductPrices
                    .Where(p => p.Id == productPriceId)
                    .Select(a => new ProductInfo
                    {
                        SellerId = a.Product.SellerId,
                        Basket = a.Product.Seller.Basket,
                        Status = a.Product.Status,
                        MinBuyQuota = a.Product.MinBuyQuota,
                        MaxBuyQuota = a.Product.MaxBuyQuota,
                        BuyQuotaDays = a.Product.BuyQuotaDays,
                        Title = a.Product.Title
                    })
                    .FirstOrDefaultAsync();
            }
            else
            {
                info = new ProductInfo
                {
                    SellerId = cartItem.ProductPrice.Product.SellerId,
                    BuyQuotaDays = cartItem.ProductPrice.Product.BuyQuotaDays,
                    MaxBuyQuota = cartItem.ProductPrice.Product.MaxBuyQuota,
                    MinBuyQuota = cartItem.ProductPrice.Product.MinBuyQuota,
                    Status = cartItem.ProductPrice.Product.Status,
                    Title = cartItem.ProductPrice.Product.Title,
                    Basket = await db.Sellers.Where(a=>a.Id == cartItem.ProductPrice.Product.SellerId).Select(a=>a.Basket).FirstOrDefaultAsync()
                };
            }

            return info;
        }

        internal async Task RemoveFromCart(long id)
        {
            await GetCartQuery().Where(a => a.Id == id).DeleteAsync();
        }

        public IQueryable<ShoppingCart> GetCartQuery(long? userId = null)
        {
            var query = db.ShoppingCarts.Include(a => a.ProductPrice.Product).AsQueryable();
            if (User == null && userId == null)
                query = query.Where(c => c.SessionId == currentUserService.AnonymousSessionId && c.UserId == null);
            else
                query = query.Where(c => c.UserId == (userId ?? User.Id));
            return query;
        }

        public async Task<int> GetShoppingCartCount()
        {
            return await GetCartQuery().SumAsync(a => a.Count);
        }

        public async Task<List<ShoppingCart>> GetCartItems()
        {
            var items = await GetCartQuery().ToListAsync();
            foreach (var item in items)
            {
                db.Entry(item).State = EntityState.Detached;
            }
            return items;
        }

        public async Task MigrateAnonymousUserShoppingCart(long userId)
        {
            var cartItems = await db.ShoppingCarts.Where(a => a.SessionId == currentUserService.AnonymousSessionId || a.UserId == userId ).ToListAsync();
            foreach (var item in cartItems)
            {
                item.SessionId = null;
                item.UserId = userId;
            }

            var duplicates = cartItems.GroupBy(a => a.ProductPriceId).Where(a=>a.Count()>1).ToList();
            foreach(var d in duplicates)
            {
                d.First().Count += d.ElementAt(1).Count;
                db.ShoppingCarts.Remove(d.ElementAt(1));
            }
            await db.SaveChangesAsync();
            currentUserService.RemoveCartCookie();
        }

        public async Task<Order> Checkout(long addressId)
        {
            throw new InvalidOperationException("در حال حاظر امکان پرداخت وجود ندارد.");
            if (User == null)
                throw new InvalidOperationException("User is not logged in.");

            var cartItems = await GetCartItems();
            if (cartItems.Count == 0)
                throw new BadRequestException(Messages.Messages.Order.EmptyCart);

            await CheckCartValidity(cartItems);

            var order = new Order();
            order.State = OrderState.Initial;
            order.UserId = User.Id;
            order.AddressId = addressId;
            var now = DateTime.Now;
            order.CheckOutData = now;
            order.ApproximateDeliveryDate = now.TimeOfDay.Hours > 12 ? now.Date.AddDays(1).AddHours(10) : now.Date.AddHours(16);
            db.Orders.Add(order);
            foreach (var item in cartItems)
            {
                OrderItem orderItem = new OrderItem();
                orderItem.UnitPrice = item.ProductPrice.DiscountPrice;
                orderItem.FinalPrice = item.ProductPrice.DiscountPrice * item.Count;
                orderItem.ProductPriceId = item.ProductPriceId;
                orderItem.Quantity = item.Count;
                order.Items.Add(orderItem);
            }

            order.TotalPrice = CalculateTotalPrice(order);
            await db.SaveChangesAsync();
            return order;
        }

        private async Task CheckCartValidity(List<ShoppingCart> cartItems)
        {
            foreach (var item in cartItems)
            {
                var p = item.ProductPrice.Product;
                if (p.Status != ProductStatus.Available)
                {
                    throw new CartItemException(Messages.Messages.Order.CannotAddProduct_NotAvailable, p.Id, p.Title);
                }
                if (p.MinBuyQuota != null && item.Count < p.MinBuyQuota)
                {
                    throw new CartItemException(string.Format(Messages.Messages.Order.CannotAddProduct_MinQuota, p.Title, p.MinBuyQuota), p.Id, p.Title);
                }

                ProductInfo info = await GetProductInfo(item, item.ProductPriceId);
                await CheckProductMaxQuota(item.ProductPriceId, item.Count, info);
            }
        }

        public async Task EmptyCart(long? userId = null)
        {
            await GetCartQuery(userId).DeleteAsync();
        }

        public decimal CalculateTotalPrice(Order order)
        {
            return order.Items.Sum(a => a.UnitPrice * a.Quantity) + GetDeliveryPrice(order);
        }
        public async Task<Payment> InitPayment(Order order, string returnUrl)
        {
            var payment = new Entities.Payment()
            {
                Amount = order.TotalPrice,
                Order = order,
                State = PaymentState.Registerd,
                RegisterDate = DateTime.Now
            };
            db.Payments.Add(payment);
            await db.SaveChangesAsync();
            payment.PayToken = await bankService.GetToken(payment, returnUrl);
            order.State = OrderState.CheckedOut;
            await db.SaveChangesAsync();
            return payment;
        }

        public async Task Paid(Payment payment)
        {
            if (payment.State == PaymentState.Succeeded)
                await EmptyCart(payment.Order?.UserId);
        }

        public decimal GetDeliveryPrice(Order order)
        {
            return DeliveryPrice;
        }

        #region Nested types
        #endregion Nested types

    }

    internal class ProductInfo
    {
        public long? SellerId { get; set; }
        public string Basket { get; set; }
        public ProductStatus Status { get; set; }
        public int? MinBuyQuota { get; set; }
        public int? MaxBuyQuota { get; set; }
        public int? BuyQuotaDays { get; set; }
        public string Title { get; internal set; }
    }
}
