using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.Models;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MahtaKala.Controllers
{

    public class ProfileController : SiteControllerBase<ProfileController>
    {
        public ProfileController(
          DataContext dataContext,
          ILogger<ProfileController> logger) : base(dataContext, logger)
        {
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

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(User.FirstName) ||
                string.IsNullOrEmpty(User.LastName) ||
                string.IsNullOrEmpty(User.MobileNumber) ||
                string.IsNullOrEmpty(User.Username) ||
                string.IsNullOrEmpty(User.EmailAddress) ||
                string.IsNullOrEmpty(User.NationalCode))
            {
                ViewData["EditRequired"] = true;
            }
            else
            {
                ViewData["EditRequired"] = false;
            }
            return View();
        }

        public IActionResult Wishlist()
        {
            var lst = db.Wishlists.Include(a => a.Product.Prices).Where(a => a.UserId == UserId).ToList();
            return View(lst);
        }

        public IActionResult BuyHistory()
        {
            var data = db.Orders.Where(o => (o.State == OrderState.Paid ||
                                                o.State == OrderState.Delivered ||
                                                o.State == OrderState.Sent) && o.UserId == UserId).ToList()
               .Select(a => new BuyHistoryModel
               {
                   Id = a.Id,
                   Price = (long)a.TotalPrice,
                   CheckoutDate = GetPersianDate(a.CheckOutData),
                   State = TranslateExtentions.GetTitle(a.State)
               }).ToList();
            return View(data);
        }

        public async Task<IActionResult> ProfileEdit()
        {
            var user = await db.Users.FirstOrDefaultAsync(a => a.Id == UserId);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ProfileEdit(User vm)
        {
            if(!ModelState.IsValid)
            {
                return Json(new { success = false, msg = "لطفا اطلاعات را به صورت صحیح وارد کنید" });
            }
            if (string.IsNullOrEmpty(vm.FirstName))
            {
                return Json(new { success = false, msg = "لطفا نام را وارد کنید" });
            }
            if (string.IsNullOrEmpty(vm.LastName))
            {
                return Json(new { success = false, msg = "لطفا نام خانوادگی را وارد کنید" });
            }
            User user = await db.Users.FirstOrDefaultAsync(a => a.Id == vm.Id);
            user.FirstName = vm.FirstName;
            user.EmailAddress = vm.EmailAddress;
            user.LastName = vm.LastName;
            user.NationalCode = vm.NationalCode;
            await db.SaveChangesAsync();
            return Json(new { success = true, msg = "ویرایش اطلااعات با موفقیت انجام شد",name = user.FullName() });
        }



        [NonAction]
        string GetPersianDate(DateTime d)
        {
            PersianCalendar pc = new PersianCalendar();
            return $"{pc.GetYear(d)}/{pc.GetMonth(d)}/{pc.GetDayOfMonth(d)} - {d.ToString("hh:mm")}";
        }
    }
}