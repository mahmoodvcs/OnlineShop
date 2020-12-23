using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure.ActionFilter;
using MahtaKala.Models.StaffModels;
using MahtaKala.Services;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Controllers.Staff
{
    [Route("~/Staff/Orders/[action]")]
    public class OrdersController : SiteControllerBase<OrdersController>
    {
        private readonly OrderService orderService;

        long _sellerId = 0;
        async Task<long> GetSellerId()
        {
            return await db.Sellers.Where(a => a.UserId == UserId).Select(a => a.Id).FirstOrDefaultAsync();
        }
        async Task<Seller> GetSeller()
        {
            return await db.Sellers.Where(a => a.UserId == UserId).FirstOrDefaultAsync();
        }

        public OrdersController(DataContext dataContext, ILogger<OrdersController> logger,
            OrderService orderService)
            : base(dataContext, logger)
        {
            this.orderService = orderService;
        }

        [Authorize(UserType.Admin, UserType.Seller)]
        public ActionResult Items()
        {
            return View("~/Views/Staff/Orders/Items.cshtml");
        }

        [Authorize(UserType.Admin, UserType.Seller)]
        public async Task<IActionResult> GetItems([DataSourceRequest] DataSourceRequest req, int? stateFilter)
        {
            var query = db.Orders
                .Where(o => o.State == OrderState.Paid ||
                        o.State == OrderState.Delivered ||
                        o.State == OrderState.Sent)
                .SelectMany(a => a.Items);

            if (base.User.Type != UserType.Admin)
            {
                if (base.User.Type == UserType.Seller)
                {
                    var sellerId = await GetSellerId();
                    query = query.Where(a => a.ProductPrice.Product.SellerId == sellerId);
                }
            }

            if (stateFilter != null)
            {
                query = query.Where(a => a.State == (OrderItemState)stateFilter);
            }

            var items = from item in query
                        orderby item.Order.CheckOutDate descending, item.Id
                        select new
                        {
                            item.Quantity,
                            item.Order.CheckOutDate,
                            item.Id,
                            item.OrderId,
                            item.ProductPrice.Product.Title,
                            item.UnitPrice,
                            item.ProductPrice.ProductId,
                            item.Order.UserId,
                            item.State,
                            item.ProductPrice.Product.Code
                        };

            var data = await items.ToDataSourceResultAsync(req, a => new OrderItemModel
            {
                Count = a.Quantity,
                Date = Util.GetPersianDate(a.CheckOutDate),
                Id = a.Id,
                OrderId = a.OrderId,
                Price = a.UnitPrice,
                Product = a.Title,
                ProductId = a.ProductId,
                UserId = a.UserId,
                State = TranslateExtentions.GetTitle(a.State),
                ProductCode = a.Code
            });
            return KendoJson(data);
        }

        [AjaxAction]
        public async Task<IActionResult> ConfirmPacked(long[] ids)
        {
            await orderService.SetItemsPacked(ids);
            return Success();
        }

        [AjaxAction]
        public async Task<IActionResult> ConfirmSent(long[] ids)
        {
            await orderService.ChangeOrderItemsState(ids, OrderItemState.Sent, await GetSellerId());
            return Success();
        }

        public async Task<ActionResult> OrderDetails(long id)
        {
            var order = await db.Orders
                .Include(a => a.Items).ThenInclude(a => a.ProductPrice).ThenInclude(a => a.Product).ThenInclude(a => a.Supplier)
                .Include(a => a.Items).ThenInclude(a => a.ProductPrice).ThenInclude(a => a.Product).ThenInclude(a => a.Seller)
                .Include(a => a.User)
                .Include(a => a.Address)
                .FirstOrDefaultAsync(a => a.Id == id);
            return View("~/Views/Staff/Orders/OrderDetails.cshtml", order);
        }
    }
}
