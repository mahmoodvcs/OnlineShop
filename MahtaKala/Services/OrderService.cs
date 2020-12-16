using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.GeneralServices.Payment;
using MahtaKala.GeneralServices.SMS;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
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
        private readonly ILogger<OrderService> logger;
        private readonly RandomGenerator randomNumberGenerator;
        private readonly IList<string> HamrahKhodroSaipa_TitleWordsList = new List<string>() { "همراه", "خودرو", "سایپا" }.AsReadOnly();

        private const long CarSpareParts_CategoryId = 168;
        private const long SuperMarket_CategoryId = 134;
        private const long ProteinProducts_CategoryId = 135;
        private const long FruitsAndVegetables_CategoryId = 136;
        private const long StationeryProducts_CategoryId = 140;
        private const long HamrahKhodroSaipa_SellerId = 57;
        private const int MillisecondsStepPeriod = 3000; // 3 seconds is the step length of "waiting before trying again"

        public OrderService(
            ICurrentUserService currentUserService,
            DataContext dataContext,
            IBankPaymentService bankService,
            SettingsService settingsService,
            IDeliveryService deliveryService,
            ILogger<OrderService> logger,
            RandomGenerator randomGenerator
            )
        {
            this.currentUserService = currentUserService;
            this.db = dataContext;
            this.bankService = bankService;
            this.settingsService = settingsService;
            this.deliveryService = deliveryService;
            this.logger = logger;
            this.randomNumberGenerator = randomGenerator;
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
        public async Task<Order> GetUserOrder() // WHAT THE HELL IS THIS??
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
            if (sellerId != null && sellerId > 0)
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
                    return from == OrderItemState.Packed || from == OrderItemState.None;
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
            if ((prod.Quantity.HasValue && prod.Quantity < count) ||
                prod.Status != ProductStatus.Available)
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

        private async Task CheckForCategoryConflicts(List<ShoppingCart> cart, ProductInfo productInfo)
        {
            if (cart == null || cart.Count() == 0)
                return;
            //var newProductCategoryId = await db.ProductPrices.Where(x => x.Id == productPriceId).FirstOrDefaultAsync();
            bool newItemIsASparePart = await ProductBelongsToCarSparePartsCategory(productInfo.Id);

            foreach (var cartItem in cart)
            {
                var cartItemIsASparePart = await ProductBelongsToCarSparePartsCategory(cartItem.ProductPrice.ProductId);
                if (newItemIsASparePart != cartItemIsASparePart)
                    throw new ApiException(412, Messages.Messages.Order.CannotAddProduct_SparePartsCategoryConflict);
            }
        }

        private void GetCategoryChildrenRecursive(Category category, List<long> resultList)
        {
            if (category.Children == null || category.Children.Count == 0)
                return;
            foreach (var child in category.Children)
            {
                if (!resultList.Contains(child.Id))
                {
                    resultList.Add(child.Id);
                    GetCategoryChildrenRecursive(child, resultList);
                }
            }
        }

		private async Task<bool> ProductBelongsToCarSparePartsCategory(long productId)
        {
            var baseCategory = await db.Categories.Where(x => x.Id == CarSpareParts_CategoryId).Include(x => x.Children)
                .ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                .ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                .ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                .ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children)
                .FirstAsync();
            var sparePartsCategoryIds = new List<long>();
            sparePartsCategoryIds.Add(baseCategory.Id);
            GetCategoryChildrenRecursive(baseCategory, sparePartsCategoryIds);
            
            bool thisIsASparePartProduct = await db.Products.AnyAsync(x => x.Id == productId
                    && x.ProductCategories.Any(y => sparePartsCategoryIds.Contains(y.CategoryId)));
            return thisIsASparePartProduct;
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
                if (db.ProductPrices.Where(a => priceIds.Contains(a.Id) && 
                        !string.IsNullOrWhiteSpace(a.Product.Seller.Basket) && 
                        !string.IsNullOrWhiteSpace(info.Basket) &&  
                        a.Product.Seller.Basket != info.Basket).Any())
                    throw new ApiException(412, Messages.Messages.Order.CannotAddProduct_DefferentSeller);
                
                await CheckForCategoryConflicts(cart.ToList(), info);
                //TODO: Check for Tehran/Non-Tehran category-address conflicts (issue #244)

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
                        Id = a.Product.Id,
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
                    Id = cartItem.ProductPrice.ProductId,
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
            var query = db.ShoppingCarts.Include(a => a.ProductPrice.Product.Quantities).AsQueryable();
            query = query.OrderByDescending(x => x.DateCreated).ThenByDescending(x => x.Id);
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
            catch (Exception ex)
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
            var query = GetCartQuery();
            var priceIds = query.Select(x => x.ProductPriceId).ToList();
            if (db.ProductPrices.Where(x => priceIds.Contains(x.Id) && x.Product.SellerId == HamrahKhodroSaipa_SellerId).Any())
			{
                return now.AddDays(2).AddMinutes(now.Minute > 0 ? 60 - now.Minute : 0);
			}
            return now.TimeOfDay.Hours > 13 ? now.Date.AddDays(1).AddHours(19) : now.Date.AddHours(19);
        }

        public TimeSpan DeliveryTimeSpan => TimeSpan.FromHours(2);

        private async Task CheckCartValidity(long addressId, List<ShoppingCart> cartItems)
        {
            List<ShoppingCart> progressiveCart = new List<ShoppingCart>();
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
                await CheckForCategoryConflicts(progressiveCart, info);
                progressiveCart.Add(item);
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
        public async Task<Payment> InitPayment(Order order, string returnUrl, SourceUsedForPayment paymentOriginatedFrom)
        {
            var payment = new Entities.Payment()
            {
                Amount = order.TotalPrice,
                Order = order,
                State = PaymentState.Registerd,
                RegisterDate = DateTime.Now,
                PaymentSourceUsed = paymentOriginatedFrom
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
                //await deliveryService.InitDelivery(payment.OrderId);
            }
            else
            {
                //try
                //{
                    await RollbackQuantity(payment.Order);
    //            }
    //            catch (Exception e)
    //            {
				//	//var message = e.Message;
				//	//var iterator = e;
				//	//while (iterator.InnerException != null)
				//	//{
				//	//    iterator = iterator.InnerException;
				//	//    message += iterator.Message;
				//	//}
				//	//message = message.ToLower();
				//	//if (message.Contains("System.InvalidOperationException")
				//	//    && message.Contains("An exception has been raised that is likely due to a transient failure")
				//	//    && message.Contains("40001: could not serialize access due to concurrent update")) 
				//	//{ // This means that roll-back operation failed due to the transaction lock on product quantities, 
				//	//  // so, we just need to wait and try again! We will wait a while, and then, try again.
    //                if (ExceptionIsDueToTransactionLockOnDatabase(e))
				//	    await WaitAndRetryRollingBack(payment.Order, 2);
				//	else
				//	{
    //                    LogRollBackFailure(e, payment);
				//	}
				//	//}
				//}
            }
        }

        private void LogRollBackFailure(Exception e, Payment payment)
        {
            var message = e.Message;
            var exceptionIterator = e;
            while (exceptionIterator.InnerException != null)
            {
                exceptionIterator = exceptionIterator.InnerException;
                message += Environment.NewLine + ">>>GOING ONE LEVEL IN<<<" + Environment.NewLine +
                    "Inner Exception: " + exceptionIterator.Message;
            }
            logger.LogError($"!!!CRITICAL ERROR!!! Rollback operation failed for payment with id {payment.Id}" +
                $" Exception thrown is as follows: {message}");
        }

        private bool ExceptionIsDueToTransactionLockOnDatabase(Exception ex)
        {
            var message = ex.Message;
            var iterator = ex;
            while (iterator.InnerException != null)
            {
                iterator = iterator.InnerException;
                message += iterator.Message;
            }
            message = message.ToLower();
            if (message.Contains("An exception has been raised that is likely due to a transient failure".ToLower())
                && message.Contains("40001: could not serialize access due to concurrent update".ToLower()))
            { // This means that roll-back operation failed due to the transaction lock on product quantities, 
              // so, we just need to wait and try again! We will wait a while, and then, try again.
                return true;
            }
            return false;
        }

        private async Task WaitAndRetryRollingBack(Order order, int howManyTriesBeforeGivingUp)
        {
            string timeTag = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
            logger.LogWarning($"WaitAndRetryRollingBack - {timeTag}");
            int trialNumber = 0;
            while (trialNumber < howManyTriesBeforeGivingUp)
            {
                double randomWaitScale = randomNumberGenerator.NextDouble();
                randomWaitScale = (randomWaitScale + 1.0) / 2.0;    // Changing the random number range from [0, 1) to [0.5, 1)
                int millisecondsToWait = (int)(Math.Pow(2, trialNumber) * randomWaitScale * MillisecondsStepPeriod);
                logger.LogWarning($"Sudo-random decesion made! Gonna wait for {millisecondsToWait} milliseconds...");
                System.Threading.Thread.Sleep(millisecondsToWait);
                try
                {
                    logger.LogWarning($"Waiting finished - Trial number: {trialNumber} - Calling Rollback on Order: {order.Id}");
                    await TryRollingQuantityBack(order);
                    logger.LogWarning($"This try was successful! RollBack is done on order: {order.Id} - Function time tag: {timeTag}!");
                    return;
                }
                catch (Exception e)
                {
                    if (!ExceptionIsDueToTransactionLockOnDatabase(e))
                        throw e;
                    trialNumber++;
                }
            }
            // If we're here, it means that after a bunch of trials (the number given as an input parameter), rolling back
            // still wasn't successful, and all failures were due to the transaction lock set on database table Quantities.
            logger.LogError($"Tried {howManyTriesBeforeGivingUp} times to roll back order with id {order.Id}, all of which " +
                $"was to no avail! Also, all trials were faield with the same reason - the transaction lock, set on database in order to " +
                $"keep write operations from running concurrently - which could create inconsistency in the inventory quantity data.");
            //logger.LogError($"TODO: This order (id:{order.Id}) is now ");
            //TODO: Now, this order should be considered as "orphan", and treated as such next time the "catcher" wakes up!
            //I am a compile error! I'm here to make sure this commit won't get executed!
        }

        

        public async Task RollbackOrder(long orderId)
        {
            var order = new Order() { Id = orderId };
            await RollbackQuantity(order);
        }

        async Task RollbackQuantity(Order order)
        {
            try
            {
                await TryRollingQuantityBack(order);
            }
            catch (Exception e)
            {
                if (ExceptionIsDueToTransactionLockOnDatabase(e))
                    await WaitAndRetryRollingBack(order, 5);
                else
                    throw e;
            }
        }

        private async Task TryRollingQuantityBack(Order origOrder)
        {
            var order = await db.Orders.Include(o => o.Items).ThenInclude(a => a.ProductPrice).ThenInclude(a => a.Product).FirstAsync(a => a.Id == origOrder.Id);
            if (order == null)
                throw new BadRequestException($"QuantityRollBack - Invalid order Id! Order with Id {origOrder.Id} does not exist!");
            if (SuccessfulOrderStates.Contains(order.State))
                throw new BadRequestException($"QuantityRollBack - Invalid order state: Id: {origOrder.Id} - State: {order.State}");

            using var transaction = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(30), TransactionScopeAsyncFlowOption.Enabled);
            order = await db.Orders.FromSqlRaw<Order>($"SELECT * FROM orders WHERE id = {origOrder.Id} FOR UPDATE").FirstOrDefaultAsync();

            if (order.State == OrderState.Canceled)
            {
                transaction.Complete();
                throw new BadRequestException($"{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fffffff")} QuantityRollBack - Invalid order state: Id: {origOrder.Id} - State: {order.State}");
            }
			string timeTag = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
            string rollbackLog = $"<<ROLLING BACK SOME QUANTITIES>>{timeTag}\t" + Environment.NewLine;
            rollbackLog += $"OrderId: {order.Id} - OrderState-before: {order.State}" + Environment.NewLine;
            order.State = OrderState.Canceled;
            var productIds = order.Items.Select(a => a.ProductPrice.ProductId).ToList();
            var quantities = await db.ProductQuantities.FromSqlRaw<ProductQuantity>($"SELECT* FROM product_quantities pq WHERE pq.product_id in ({string.Join(',', productIds)}) FOR UPDATE").ToListAsync();
            rollbackLog += "Quantity.Id, Quantity.quantity-before, Quantity.quantity-after, OrderItem.Id, OrderItem.quantity\t" + Environment.NewLine;
            foreach (var item in order.Items)
            {
                var quantity = quantities.FirstOrDefault(a => a.ProductId == item.ProductPrice.ProductId);
                bool itWasZeroBeforeRollBack = (quantity.Quantity == 0);
                rollbackLog += $"{quantity.Id}, {quantity.Quantity}, ";
                quantity.Quantity += item.Quantity;
                rollbackLog += $"{quantity.Quantity}, {item.Id}, {item.Quantity}\t" + Environment.NewLine;
                if (item.ProductPrice.Product.Status == ProductStatus.NotAvailable && quantity.Quantity > 0 && itWasZeroBeforeRollBack)
                {
                    item.ProductPrice.Product.Status = ProductStatus.Available;
                }
            }
            rollbackLog += $"CurrentTime:{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fffffff")}---Now Saving Changes--- FunctionTimeTag:{timeTag}\t{Environment.NewLine}";
            await db.SaveChangesAsync();
            rollbackLog += $"CurrentTime:{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fffffff")}---Changes Were Saved--- FunctionTimeTag:{timeTag}\t{Environment.NewLine}";
            logger.LogWarning(rollbackLog);
            transaction.Complete();
        }

        public decimal GetDeliveryPrice(Order order = null)
        {
            return settingsService.DeliveryPrice;
        }

        public async Task SetOrderDelivered(long orderId)
        {
            var order = await db.Orders.Include(a => a.Items).FirstOrDefaultAsync(a => a.Id == orderId);
            if (order.State == OrderState.Delivered)
            {
                throw new BadRequestException(SMSManager.TEMP_MARK + "این کد قبلاً دریافت شده است.");
            }
            if (order.State == OrderState.Initial || order.State == OrderState.CheckedOut)
            {
                throw new BadRequestException(SMSManager.TEMP_MARK + "هزینه ی سفارش پرداخت نشده است.");
            }
            else if (order.State == OrderState.Canceled)
                throw new BadRequestException(SMSManager.TEMP_MARK + "این سفارش قبلاً لغو شده است.");


            var items = order.Items.Where(a => a.State == OrderItemState.Sent || a.State == OrderItemState.None);
            if (items.Count() == 0)
            {
                throw new BadRequestException(SMSManager.TEMP_MARK + "این سفارش قلم ارسال شده ندارد.");
            }
            if (items.Any(x => x.State == OrderItemState.None))
            {
                var unsentItems = items.Where(x => x.State == OrderItemState.None).ToList();
                var message = $"CAUTION!! OrderId: {order.Id} - SetOrderDelivered - {unsentItems.Count} items with state \"None\" - These items' state will be set as \"Delivered\" as well, but, take caution!" +
                    $"The items' ids are as follows: {string.Join(',', unsentItems.Select(x => x.Id.ToString()))}";
                logger.LogError(message);
            }

            foreach (var item in items)
            {
                item.State = OrderItemState.Delivered;
            }
            if (order.Items.All(a => a.State == OrderItemState.Delivered))
                order.State = OrderState.Delivered;
            await db.SaveChangesAsync();

            await ShareOrderPayment(orderId, true);
        }

        public async Task ShareOrderPayment(long orderId, bool includeDelivery)
        {
            string functionTImeTag = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");

            logger.LogWarning($"OrderService.ShareOrderPayment - StartTime: {functionTImeTag}");
            Payment payment = await GetPaymentToShare(orderId);
            var items = db.OrderItems.Where(a => a.OrderId == orderId && a.State == OrderItemState.Delivered)
                .Select(a => new
                {
                    a.Id,
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
                        a.Id
                    }).ToList()
                })
                .SelectMany(a => a.Share)
                .GroupBy(a => new { a.ShabaId, a.Name, a.Id })
                .Select(a => new PaymentShareDataItem
                {
                    ShabaId = a.Key.ShabaId,
                    Name = a.Key.Name,
                    Amount = a.Sum(s => s.Amount),
                    ItemId = a.Key.Id,
                    PayFor = PayFor.OrderItem
                }).ToList();

            if (includeDelivery)
            {
                items.Add(new PaymentShareDataItem
                {
                    Amount = GetDeliveryPrice(),
                    Name = deliveryService.GetName(),
                    ShabaId = deliveryService.GetShabaId(),
                    PayFor = PayFor.Delivery
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

        //public async Task ShareDeliveryPayment(long orderId)
        //{
        //    Payment payment = await GetPaymentToShare(orderId);
        //    await bankService.SharePayment(payment, new List<PaymentShareDataItem>
        //    {
        //        new PaymentShareDataItem
        //        {
        //            Amount = GetDeliveryPrice(),
        //            Name = deliveryService.GetName(),
        //            ShabaId = deliveryService.GetShabaId()
        //        }
        //    });
        //}
    }



    internal class ProductInfo
    {
		public long Id { get; set; }
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
