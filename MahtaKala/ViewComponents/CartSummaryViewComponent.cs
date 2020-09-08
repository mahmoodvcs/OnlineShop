using MahtaKala.Controllers;
using MahtaKala.Entities;
using MahtaKala.Infrustructure;
using MahtaKala.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private DataContext _db;
        public CartSummaryViewComponent(DataContext db, IHttpContextAccessor contextAccessor,
             OrderService orderService)
        {
            _db = db;
            this.contextAccessor = contextAccessor;
            this.orderService = orderService;
        }

        private readonly IHttpContextAccessor contextAccessor;
        private readonly OrderService orderService;
        private User user;
        public new User User
        {
            get
            {
                if (user == null)
                {
                    user = (User)HttpContext.Items["User"];
                }
                return user;
            }
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var count = await orderService.GetShoppingCartCount();
            return View(count);
        }
    }
}
