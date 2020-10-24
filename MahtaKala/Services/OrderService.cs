using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.GeneralServices.Payment;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders.Physical;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Z.EntityFramework.Plus;

namespace MahtaKala.Services
{
    public class OrderService
    {
        private readonly ICurrentUserService currentUserService;
        private readonly DataContext db;
        private readonly IBankPaymentService bankService;
        private readonly SettingsService settingsService;
        private readonly IDeliveryService deliveryService;

        public OrderService(
            ICurrentUserService currentUserService,
            DataContext dataContext,
            IBankPaymentService bankService,
            SettingsService settingsService,
            IDeliveryService deliveryService
            )
        {
            this.currentUserService = currentUserService;
            this.db = dataContext;
            this.bankService = bankService;
            this.settingsService = settingsService;
            this.deliveryService = deliveryService;
        }

        #region Constants

        public readonly OrderState[] SuccessfulOrderStates =
        {
            OrderState.Paid,
            OrderState.Sent,
            OrderState.Delivered
        };
        public readonly OrderState[] QuantitySubtractedOrderStates =
        {
            OrderState.Initial,
            OrderState.CheckedOut,
            OrderState.Paid,
            OrderState.Sent,
            OrderState.Delivered
        };

        #endregion Constants
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

        public async Task ChangeOrderItemsState(long[] ids, OrderItemState state, long? sellerId = null)
        {

            var query = db.OrderItems.Where(a => ids.Contains(a.Id));
            if (sellerId != null)
            {
                query = query.Where(a => a.ProductPrice.Product.SellerId == sellerId.Value);
            }
            var items = await query.ToListAsync();
            foreach (var item in items)
            {
                if (!IsValidTransition(item.State, state))
                    throw new Exception(string.Format("امکان تغییر وضعیت از {0} به {1} وجود ندارد",
                        TranslateExtentions.GetTitle(item.State),
                        TranslateExtentions.GetTitle(state)
                    ));
                item.State = state;
            }
            await db.SaveChangesAsync();
        }

        public async Task SetItemsPacked(long[] ids)
        {
            using var tr = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            await ChangeOrderItemsState(ids, OrderItemState.Packed);
            var order = db.Orders.FirstOrDefault(x => x.Items.Any(y => ids.Contains(y.Id)));
            if (order == null)
            {
                throw new Exception(string.Format("سفارش مورد نظر یافت نشد! - ItemIds: {0}", JsonConvert.SerializeObject(ids)));
            }
            await deliveryService.InitDelivery(order.Id);
            tr.Complete();
        }

        bool IsValidTransition(OrderItemState from, OrderItemState to)
        {
            switch (to)
            {
                case OrderItemState.Packed:
                    return from == OrderItemState.None;
                case OrderItemState.Sent:
                    return from == OrderItemState.Packed;
            }
            return false;
        }

        public async Task<long> UpdateCart(long productPriceId, int count)
        {
            var cart = GetCartQuery();
            var cartItem = await cart.FirstOrDefaultAsync(c => c.ProductPriceId == productPriceId);
            return await UpdateCart(cartItem, productPriceId, count);
        }

        private async Task CheckProductBuyLimitations(long addressId, long priceId, int count, long pid, string name)
        {
            var limits = from pr in db.ProductPrices.Where(a => a.Id == priceId)
                         from pl in pr.Product.BuyLimitations
                         select pl.BuyLimitation;
            limits = limits.Union(
                from pr in db.ProductPrices.Where(a => a.Id == priceId)
                from cat in pr.Product.ProductCategories
                from cl in cat.Category.BuyLimitations
                select cl.BuyLimitation);

            var items = await limits.ToListAsync();
            long cityId = 0, provinceId = 0;
            if (items.Any())
            {
                var addr = await db.Addresses.Where(a => a.Id == addressId).Select(a => new
                {
                    a.CityId,
                    a.City.ProvinceId
                }).FirstOrDefaultAsync();
                cityId = addr.CityId;
                priceId = addr.ProvinceId;
            }
            foreach (var item in items)
            {
                await CheckProductBuyLimitations(cityId, provinceId, priceId, item, count, pid, name);
            }

        }
        private async Task CheckProductBuyLimitations(long cityId, long provinceId, long priceId, BuyLimitation limitation, int count, long pid, string name)
        {
            if (limitation.MinBuyQuota != null && count < limitation.MinBuyQuota)
            {
                throw new CartItemException(string.Format(Messages.Messages.Order.CannotAddProduct_MinQuota, name, limitation.MinBuyQuota), pid, name);
            }

            if (count > limitation.MaxBuyQuota)
                throw new ApiException(400, string.Format(Messages.Messages.Order.CannotAddProduct_MaxQuota,
                    //Util.GetTimeSpanPersianString(TimeSpan.FromDays(limitation.BuyQuotaDays.Value)), 
                    name, limitation.MaxBuyQuota));

            if (limitation.BuyQuotaDays != null)
            {
                var minTime = DateTime.Now.AddDays(-limitation.BuyQuotaDays.Value);
                var userId = currentUserService.User?.Id;
                if (userId != null)
                {
                    var prevProductCount = await
                        (from order in db.Orders
                                .Where(o => o.UserId == userId
                                    && SuccessfulOrderStates.Contains(o.State)
                                    && o.CheckOutDate > minTime)
                         from orderItem in order.Items.Where(a => a.ProductPriceId == priceId)
                         select orderItem.Quantity).SumAsync();
                    if (prevProductCount + count > limitation.MaxBuyQuota)
                    {
                        throw new ApiException(400, string.Format(Messages.Messages.Order.CannotAddProduct_MaxQuota,
                            Util.GetTimeSpanPersianString(TimeSpan.FromDays(limitation.BuyQuotaDays.Value)), limitation.MaxBuyQuota));
                    }
                }
            }

            if (limitation.CityId != null && limitation.CityId != cityId)
            {
                var cityName = await db.Cities.Where(a => a.Id == limitation.CityId).Select(a => a.Name).FirstOrDefaultAsync();
                throw new ApiException(400, string.Format(Messages.Messages.Order.CannotAddProduct_WrongCity,
                    name, cityName));
            }

            if (limitation.ProvinceId != null && limitation.ProvinceId != provinceId)
            {
                var provinceName = await db.Provinces.Where(a => a.Id == limitation.ProvinceId).Select(a => a.Name).FirstOrDefaultAsync();
                throw new ApiException(400, string.Format(Messages.Messages.Order.CannotAddProduct_WrongProvince,
                    name, provinceName));
            }
        }

        private async Task CheckCartItemValidity(long priceId, int count, ProductInfo prod)
        {
            if (prod.Quantity.HasValue && prod.Quantity < count)
            {
                throw new ApiException(400, $"محصول '{prod.Title}' موجود نیست");
            }
            await CheckProductMaxQuota(priceId, count, prod);
        }
        private async Task CheckProductMaxQuota(long priceId, int count, ProductInfo prod)
        {
            if (prod.MaxBuyQuota == null)
                return;
            if (count > prod.MaxBuyQuota)
                throw new ApiException(400, string.Format(Messages.Messages.Order.CannotAddProduct_MaxQuota,
                    //Util.GetTimeSpanPersianString(TimeSpan.FromDays(prod.BuyQuotaDays ?? 0))
                    prod.Title
                    , prod.MaxBuyQuota));

            if ((prod.BuyQuotaDays ?? 0) > 0)
            {
                var minTime = DateTime.Now.AddDays(-prod.BuyQuotaDays.Value);
                var userId = currentUserService.User?.Id;
                if (userId != null)
                {
                    var prevProductCount = await
                        (from order in db.Orders
                                .Where(o => o.UserId == userId
                                    && SuccessfulOrderStates.Contains(o.State)
                                    && o.CheckOutDate > minTime)
                         from orderItem in order.Items.Where(a => a.ProductPriceId == priceId)
                         select orderItem.Quantity).SumAsync();
                    if (prevProductCount + count > prod.MaxBuyQuota)
                    {
                        throw new ApiException(400, string.Format(Messages.Messages.Order.CannotAddProduct_MaxQuota,
                            //Util.GetTimeSpanPersianString(TimeSpan.FromDays(prod.BuyQuotaDays ?? 0))
                            prod.Title
                            , prod.MaxBuyQuota));
                    }
                }
            }
        }

        private async Task<long> UpdateCart(ShoppingCart cartItem, long productPriceId, int count)
        {
            ProductInfo info = await GetProductInfo(cartItem, productPriceId);
            await CheckCartItemValidity(productPriceId, count, info);

            var cart = GetCartQuery();
            if (cartItem == null)
            {
                if (info.Status != ProductStatus.Available)
                    throw new ApiException(400, string.Format(Messages.Messages.Order.CannotAddProduct_NotAvailable, info.Title));

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
                        Title = a.Product.Title,
                        Quantity = a.Product.Quantities.FirstOrDefault().Quantity
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
                    Basket = await db.Sellers.Where(a => a.Id == cartItem.ProductPrice.Product.SellerId).Select(a => a.Basket).FirstOrDefaultAsync(),
                    Quantity = await db.ProductQuantities.Where(a => a.ProductId == cartItem.ProductPrice.ProductId).Select(a => a.Quantity).FirstOrDefaultAsync()
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
            var cartItems = await db.ShoppingCarts.Where(a => a.SessionId == currentUserService.AnonymousSessionId || a.UserId == userId).ToListAsync();
            foreach (var item in cartItems)
            {
                item.SessionId = null;
                item.UserId = userId;
            }

            var duplicates = cartItems.GroupBy(a => a.ProductPriceId).Where(a => a.Count() > 1).ToList();
            foreach (var d in duplicates)
            {
                d.First().Count += d.ElementAt(1).Count;
                db.ShoppingCarts.Remove(d.ElementAt(1));
            }
            await db.SaveChangesAsync();
            currentUserService.RemoveCartCookie();
        }

        public async Task<Order> Checkout(long addressId)
        {
            //throw new InvalidOperationException("در حال حاضر امکان خرید وجود ندارد.");
            if (User == null)
                throw new InvalidOperationException("User is not logged in.");
            try
            {
                User.CheckProfileCompletion();
            }
            catch(Exception ex)
            {
                throw new ApiException(400, ex.Message);
            }

            var cartItems = await GetCartItems();
            if (cartItems.Count == 0)
                throw new ApiException(400, Messages.Messages.Order.EmptyCart);

            await CheckCartValidity(addressId, cartItems);

            using var transaction = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(30), TransactionScopeAsyncFlowOption.Enabled);

            var order = new Order();
            order.State = OrderState.Initial;
            order.UserId = User.Id;
            order.AddressId = addressId;
            order.CheckOutDate = DateTime.Now;
            order.ApproximateDeliveryDate = GetApproximateDeilveryDate();
            db.Orders.Add(order);

            var productIds = cartItems.Select(a => a.ProductPrice.ProductId).ToList();
            var quantities = await db.ProductQuantities.FromSqlRaw<ProductQuantity>($"SELECT* FROM product_quantities pq WHERE pq.product_id in ({string.Join(',', productIds)}) FOR UPDATE").ToListAsync();

            foreach (var item in cartItems)
            {
                OrderItem orderItem = new OrderItem();
                orderItem.UnitPrice = item.ProductPrice.DiscountPrice;
                orderItem.FinalPrice = item.ProductPrice.DiscountPrice * item.Count;
                orderItem.ProductPriceId = item.ProductPriceId;
                orderItem.Quantity = item.Count;
                order.Items.Add(orderItem);

                var pqs = quantities.Where(a => a.ProductId == item.ProductPrice.ProductId).ToList();
                if (pqs.Count > 1)
                {
                    throw new CartItemException($"اطلاعات موجودی محصول {item.ProductPrice.Product.Title} اشتباه است", item.ProductPrice.ProductId, item.ProductPrice.Product.Title);
                }
                if (pqs.Count == 0)
                {
                    throw new CartItemException($"محصول {item.ProductPrice.Product.Title} موجود نیست", item.ProductPrice.ProductId, item.ProductPrice.Product.Title);
                }
                var quantity = pqs[0];
                if (quantity.Quantity < item.Count)
                {
                    throw new CartItemException($"محصول {item.ProductPrice.Product.Title} به تعداد {quantity.Quantity} موجود است", item.ProductPrice.ProductId, item.ProductPrice.Product.Title);
                }
                quantity.Quantity -= item.Count;
                if (quantity.Quantity <= 0)
                    item.ProductPrice.Product.Status = ProductStatus.NotAvailable;
            }

            order.TotalPrice = CalculateTotalPrice(order);
            order.DeliveryPrice = GetDeliveryPrice(order);

            await db.SaveChangesAsync();

            transaction.Complete();
            return order;
        }

        public DateTime GetApproximateDeilveryDate()
        {
            DateTime now = DateTime.Now;
            return now.TimeOfDay.Hours > 12 ? now.Date.AddDays(1).AddHours(10) : now.Date.AddHours(19);
        }

        public TimeSpan DeliveryTimeSpan => TimeSpan.FromHours(2);

        private async Task CheckCartValidity(long addressId, List<ShoppingCart> cartItems)
        {
            foreach (var item in cartItems)
            {
                var p = item.ProductPrice.Product;
                if (p.Status != ProductStatus.Available)
                {
                    throw new CartItemException(string.Format(Messages.Messages.Order.CannotAddProduct_NotAvailable, p.Title), p.Id, p.Title);
                }
                if (p.MinBuyQuota != null && item.Count < p.MinBuyQuota)
                {
                    throw new CartItemException(string.Format(Messages.Messages.Order.CannotAddProduct_MinQuota, p.Title, p.MinBuyQuota), p.Id, p.Title);
                }

                ProductInfo info = await GetProductInfo(item, item.ProductPriceId);
                await CheckCartItemValidity(item.ProductPriceId, item.Count, info);
                await CheckProductBuyLimitations(addressId, item.ProductPriceId, item.Count, item.ProductPrice.ProductId, item.ProductPrice.Product.Title);
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
            {
                await EmptyCart(payment.Order?.UserId);
                await deliveryService.InitDelivery(payment.OrderId);
            }
            else
            {
                await RollbackQuantity(payment.Order);
            }
        }

        public async Task RollbackUnsuccessfulPayments()
        {
            var initalStates = QuantitySubtractedOrderStates.Except(SuccessfulOrderStates).ToArray();
            var list = await db.Orders.Where(a => a.CheckOutDate < DateTime.Now.AddMinutes(-20)
                && initalStates.Contains(a.State)).ToListAsync();
            foreach (var item in list)
            {

            }
        }

        async Task RollbackQuantity(Order origOrder)
        {
            if (SuccessfulOrderStates.Contains(origOrder.State))
                throw new Exception($"Invalid order state: Id: {origOrder.Id} - State: {origOrder.State}");

            var order = await db.Orders.Include(o => o.Items).ThenInclude(a => a.ProductPrice).ThenInclude(a => a.Product).FirstAsync(a => a.Id == origOrder.Id);
            using var transaction = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(30), TransactionScopeAsyncFlowOption.Enabled);

            order.State = OrderState.Canceled;
            var productIds = order.Items.Select(a => a.ProductPrice.ProductId).ToList();
            var quantities = await db.ProductQuantities.FromSqlRaw<ProductQuantity>($"SELECT* FROM product_quantities pq WHERE pq.product_id in ({string.Join(',', productIds)}) FOR UPDATE").ToListAsync();

            foreach (var item in order.Items)
            {
                var quantity = quantities.FirstOrDefault(a => a.ProductId == item.ProductPrice.ProductId);
                quantity.Quantity += item.Quantity;
                if (item.ProductPrice.Product.Status == ProductStatus.NotAvailable && quantity.Quantity > 0)
                {
                    item.ProductPrice.Product.Status = ProductStatus.Available;
                }
            }

            await db.SaveChangesAsync();
            transaction.Complete();
        }

        public decimal GetDeliveryPrice(Order order = null)
        {
            return settingsService.DeliveryPrice;
        }

        public async Task ShareOrderPayment(long orderId, bool includeDelivery)
        {
            Payment payment = await GetPaymentToShare(orderId);

            var items = db.OrderItems.Where(a => a.OrderId == orderId)
                .Select(a => new
                {
                    a.ProductPrice.ProductId,
                    a.FinalPrice,
                    Share = a.ProductPrice.Product.PaymentParties.Select(p => new
                    {
                        p.PaymentParty.ShabaId,
                        p.PaymentParty.Name,
                        p.Percent
                    }).ToList()
                }).ToList()
                .Select(a => new
                {
                    Share = a.Share.Select(s => new
                    {
                        Amount = a.FinalPrice / 100m * (decimal)s.Percent,
                        s.ShabaId,
                        s.Name,
                        a.ProductId,
                    }).ToList()
                })
                .SelectMany(a => a.Share)
                .GroupBy(a => new { a.ShabaId, a.Name })
                .Select(a => new PaymentShareDataItem
                {
                    ShabaId = a.Key.ShabaId,
                    Name = a.Key.Name,
                    Amount = a.Sum(s => s.Amount)
                }).ToList();

            if (includeDelivery)
            {
                items.Add(new PaymentShareDataItem
                {
                    Amount = GetDeliveryPrice(),
                    Name = deliveryService.GetName(),
                    ShabaId = deliveryService.GetShabaId()
                });
            }

            await bankService.SharePayment(payment, items);
        }

        private async Task<Payment> GetPaymentToShare(long orderId)
        {
            var payments = await db.Payments.Where(a => a.OrderId == orderId && a.State == PaymentState.Succeeded).ToListAsync();
            if (payments.Count == 0)
            {
                throw new Exception("No succeeded payment found for order " + orderId);
            }
            if (payments.Count > 1)
            {
                throw new Exception($"More than one succeeded payments found for order {orderId}. Payment Ids: {string.Join(',', payments.Select(a => a.Id).ToList())}");
            }

            return payments[0];
        }

        public async Task ShareDeliveryPayment(long orderId)
        {
            Payment payment = await GetPaymentToShare(orderId);
            await bankService.SharePayment(payment, new List<PaymentShareDataItem>
            {
                new PaymentShareDataItem
                {
                    Amount = GetDeliveryPrice(),
                    Name = deliveryService.GetName(),
                    ShabaId = deliveryService.GetShabaId()
                }
            });
        }
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
        public int? Quantity { get; set; }
    }
}
