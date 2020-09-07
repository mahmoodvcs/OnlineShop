using MahtaKala.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.ViewComponents
{
    public class WishListSummaryViewComponent : ViewComponent
    {
        private DataContext _db;
        public WishListSummaryViewComponent(DataContext db)
        {
            _db = db;
        }

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
            int count = 0;
            if (User != null)
            {
                count = await _db.Wishlists.Where(a => a.UserId == User.Id).CountAsync();
            }
            var vm = (User != null, count);
            return View(vm);
        }
    }
}
