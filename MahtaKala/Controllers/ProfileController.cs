﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure.Exceptions;
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
            var addresses = await db.Addresses.Include(a => a.City).Where(a => a.UserId == UserId).ToListAsync();
            UserDataVM vm = new UserDataVM
            {
                EmailAddress = user.EmailAddress,
                FirstName = user.FirstName,
                LastName = user.LastName,
                NationalCode = user.NationalCode,
                Addresses = addresses
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ProfileEdit(UserDataVM vm)
        {
            if (string.IsNullOrEmpty(vm.FirstName))
            {
                return Json(new { success = false, msg = "لطفا نام را وارد کنید" });
            }
            if (string.IsNullOrEmpty(vm.LastName))
            {
                return Json(new { success = false, msg = "لطفا نام خانوادگی را وارد کنید" });
            }
            if (!string.IsNullOrEmpty(vm.EmailAddress) && !Util.IsValidEmailaddress(vm.EmailAddress))
            {
                return Json(new { success = false, msg = "لطفا ایمیل را به صورت صحیح وارد کنید" });
            }


            User user = await db.Users.FirstOrDefaultAsync(a => a.Id == UserId);
            user.FirstName = vm.FirstName;
            user.EmailAddress = vm.EmailAddress;
            user.LastName = vm.LastName;
            user.NationalCode = vm.NationalCode;
            await db.SaveChangesAsync();
            return Json(new { success = true, msg = "ویرایش اطلااعات با موفقیت انجام شد", name = user.FullName() });
        }


        public async Task<IActionResult> AddressEdit(long Id)
        {
            if (Id == 0)
            {
                return View();
            }
            else
            {
                UserAddress address = await db.Addresses.Include(a => a.City).Where(a => a.Id == Id).FirstOrDefaultAsync();
                if (address == null)
                {
                    throw new EntityNotFoundException<UserAddress>(Id);
                }
                return View(address);
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddressEdit(UserAddress address)
        {
            if (address.Id == 0)
            {
                db.Addresses.Add(address);
            }
            else
            {
                db.Entry(address).State = EntityState.Modified;
            }
            address.UserId = UserId;
            await db.SaveChangesAsync();
            return RedirectToAction("ProfileEdit");
        }



        [NonAction]
        string GetPersianDate(DateTime d)
        {
            PersianCalendar pc = new PersianCalendar();
            return $"{pc.GetYear(d)}/{pc.GetMonth(d)}/{pc.GetDayOfMonth(d)} - {d.ToString("hh:mm")}";
        }
    }
}