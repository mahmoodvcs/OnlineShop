using MahtaKala.Controllers;
using MahtaKala.Entities;
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
        public CartSummaryViewComponent(DataContext db, IHttpContextAccessor contextAccessor)
        {
            _db = db;
            this.contextAccessor = contextAccessor;
        }

        private readonly IHttpContextAccessor contextAccessor;
        public HttpContext Current => contextAccessor.HttpContext;
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
            int count;
            if (User != null)
            {
                count = await _db.ShppingCarts.Where(a => a.UserId == User.Id).SumAsync(a => a.Count);
            }
            else
            {
                string sessionId = Current.Session.Id;
                count = await _db.ShppingCarts.Where(a => a.SessionId == sessionId).SumAsync(a => a.Count);
            }
            return View(count);
        }
    }
}
