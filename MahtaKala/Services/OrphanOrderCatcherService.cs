using MahtaKala.Entities;
using MahtaKala.Infrustructure;
using MahtaKala.Infrustructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace MahtaKala.Services
{
	public class OrphanOrderCatcherService
	{
		//private readonly SingletonDataContext db;
		private readonly DataContext db;
		private readonly OrderService orderService;
		private readonly ILogger<OrphanOrderCatcherService> logger;
		// One hour after the order has been checked out, if it is not in a conclusive state, it's considered "orphan", and would be cancelled.
		private readonly TimeSpan DiscardOrderThreshold = new TimeSpan(1, 0, 0);
		
		public OrphanOrderCatcherService(
			//SingletonDataContext dataContext,
			DataContext dataContext,
			OrderService orderService,
			ILogger<OrphanOrderCatcherService> logger)
		{
			db = dataContext;
			this.orderService = orderService;
			this.logger = logger;
		}

		public async Task RoundUpOrphans()
		{
			var orphanStates = orderService.QuantitySubtractedOrderStates.Except(orderService.SuccessfulOrderStates).ToList();
			var orphanDateThreshold = DateTime.Now.Subtract(DiscardOrderThreshold);	// Orders with checkout date prior to one hour ago
			var orphanOrderIds = db.Orders.Where(x => x.CheckOutDate < orphanDateThreshold 
				&& orphanStates.Contains(x.State)).Select(x => x.Id).ToList();
			foreach (var orphanId in orphanOrderIds)
			{
				try
				{
					var order = new Order() { Id = orphanId };
					logger.LogWarning($"OrphanOrderCatcherService - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Calling RollBack on Order {orphanId}...");
					await orderService.RollbackOrder(orphanId);
					logger.LogWarning($"OrphanOrderCatcherService - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Order with id {orphanId} have been successfully rolled back!");
				}
				catch (Exception e)
				{
					var message = $"OrphanOrderCatcherService - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - " +
						$"Exception thrown while dealing with orphan order with id {orphanId}. Now the exception's message: {e.Message}";
					while (e.InnerException != null)
					{
						e = e.InnerException;
						message += Environment.NewLine + " --- End of outer exception message. Going in one level..." + Environment.NewLine +
							"Inner exception message is: " + e.Message;
					}
					logger.LogError(message);
				}

			}
		}

//        #region CopiedFromOrderServiceEXACTLY
//        public readonly OrderState[] SuccessfulOrderStates =
//{
//            OrderState.Paid,
//            OrderState.Sent,
//            OrderState.Delivered
//        };
//        public readonly OrderState[] QuantitySubtractedOrderStates =
//        {
//            OrderState.Initial,
//            OrderState.CheckedOut,
//            OrderState.Paid,
//            OrderState.Sent,
//            OrderState.Delivered
//        };

//        public async Task RollbackOrder(long orderId)
//        {
//            var order = new Order() { Id = orderId };
//            await RollbackQuantity(order);
//        }

//        async Task RollbackQuantity(Order origOrder)
//        {
//            var order = await db.Orders.Include(o => o.Items).ThenInclude(a => a.ProductPrice).ThenInclude(a => a.Product).FirstAsync(a => a.Id == origOrder.Id);
//            if (order == null)
//                throw new BadRequestException($"Invalid order Id! Order with Id {origOrder.Id} does not exist!");
//            if (SuccessfulOrderStates.Contains(order.State))
//                throw new BadRequestException($"Invalid order state: Id: {origOrder.Id} - State: {origOrder.State}");
//            if (order.State == OrderState.Canceled)
//                throw new BadRequestException($"Invalid order state: Id: {origOrder.Id} - State: {origOrder.State}");
//            using var transaction = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(30), TransactionScopeAsyncFlowOption.Enabled);

//            order.State = OrderState.Canceled;
//            var productIds = order.Items.Select(a => a.ProductPrice.ProductId).ToList();
//            var quantities = await db.ProductQuantities.FromSqlRaw<ProductQuantity>($"SELECT* FROM product_quantities pq WHERE pq.product_id in ({string.Join(',', productIds)}) FOR UPDATE").ToListAsync();

//            foreach (var item in order.Items)
//            {
//                var quantity = quantities.FirstOrDefault(a => a.ProductId == item.ProductPrice.ProductId);
//                bool itWasZeroBeforeRollBack = (quantity.Quantity == 0);
//                quantity.Quantity += item.Quantity;
//                if (item.ProductPrice.Product.Status == ProductStatus.NotAvailable && quantity.Quantity > 0 && itWasZeroBeforeRollBack)
//                {
//                    item.ProductPrice.Product.Status = ProductStatus.Available;
//                }
//            }

//            await db.SaveChangesAsync();
//            transaction.Complete();
//        }
//        #endregion CopiedFromOrderServiceEXACTLY
    }
}
