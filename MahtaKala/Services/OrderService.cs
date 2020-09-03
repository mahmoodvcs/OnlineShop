using MahtaKala.Entities;
using MahtaKala.SharedServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class OrderService
    {
        private readonly ICurrentUserService currentUserService;
        private readonly DataContext db;

        public OrderService(
            ICurrentUserService currentUserService,
            DataContext dataContext
            )
        {
            this.currentUserService = currentUserService;
            this.db = dataContext;
        }
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
            foreach (var o in list.Where(o=>o.State == OrderState.CheckedOut))
            {
                o.State = OrderState.Canceled;
            }
            await db.SaveChangesAsync();

            return order;
        }
}
}
