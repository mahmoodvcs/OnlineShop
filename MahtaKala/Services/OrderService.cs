﻿using MahtaKala.Entities;
using MahtaKala.Entities.Migrations;
using MahtaKala.GeneralServices.Payment;
using MahtaKala.SharedServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class OrderService
    {
        public const int DeliveryPrice = 100000;

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

        public async Task AddToCart(int productPriceId, int count = 1)
        {
            var cart = GetCartQuery();
            var cartItem = await cart.FirstOrDefaultAsync(c => c.ProductPriceId == productPriceId);
            if (cartItem == null)
            {
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
            else
            {
                cartItem.Count += count;
            }
            await db.SaveChangesAsync();
        }

        private IQueryable<ShoppingCart> GetCartQuery()
        {
            var query = db.ShoppingCarts.Include(a => a.ProductPrice.Product).AsQueryable();
            if (User == null)
                query = query.Where(c => c.SessionId == currentUserService.AnonymousSessionId && c.UserId == null);
            else
                query = query.Where(c => c.UserId == User.Id);
            return query;
        }

        public async Task<int> GetShoppingCartCount()
        {
            return await GetCartQuery().CountAsync();
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

        public async Task<Order> Checkout(long addressId)
        {
            if (User == null)
                throw new InvalidOperationException("User is not logged in.");
            var order = new Order();
            order.State = OrderState.Initial;
            order.UserId = User.Id;
            order.AddressId = addressId;
            order.CheckOutData = DateTime.Now;
            db.Orders.Add(order);
            var cartItems = await GetCartItems();
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


        public decimal CalculateTotalPrice(Order order)
        {
            return order.Items.Sum(a => a.UnitPrice * a.Quantity) + GetDeliveryPrice(order);
        }
        public async Task<Entities.Payment> InitPayment(Order order, string returnUrl)
        {
            var payment = new Entities.Payment()
            {
                Amount = order.TotalPrice,
                Order = order,
                State = PaymentState.Registerd
            };
            db.Payments.Add(payment);
            await db.SaveChangesAsync();
            payment.PayToken = await bankService.GetToken(payment, returnUrl);
            order.State = OrderState.CheckedOut;
            await db.SaveChangesAsync();
            return payment;
        }

        //public async Task<Entities.Payment> Paid(string bankReturnBody)
        //{

        //}

        public decimal GetDeliveryPrice(Order order)
        {
            return DeliveryPrice;
        }

    }
}
