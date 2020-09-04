﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.Models;
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
                   State = Enum.GetName(typeof(OrderState), a.State)
               }).ToList();
            return View(data);
        }

        [NonAction]
        string GetPersianDate(DateTime d)
        {
            PersianCalendar pc = new PersianCalendar();
            return $"{pc.GetYear(d)}/{pc.GetMonth(d)}/{pc.GetDayOfMonth(d)} {d.TimeOfDay}";
        }
    }
}