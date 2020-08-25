using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.Helpers;
using MahtaKala.Models;
using MahtaKala.Models.UserModels;
using MahtaKala.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MahtaKala.Controllers
{
    public class AccountController : Controller
    {

        protected readonly DataContext db;
        private readonly IUserService userService;
        private readonly ISMSService smsService;
        public AccountController(ISMSService smsService, DataContext context, IUserService userService)
        {
            this.smsService = smsService;
            this.db = context;
            this.userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult FirstRequest()
        {
            SignupRequest vm = new SignupRequest();
            return PartialView(vm);
        }

        [HttpPost]
        public async Task<IActionResult> FirstRequest(SignupRequest vm)
        {
            var number = Util.NormalizePhoneNumber(vm.Mobile);
            if (!System.Text.RegularExpressions.Regex.IsMatch(number, @"^(\+98|0)?9\d{9}$"))
            {
                return Json(new { success = false, msg = "لطفا موبایل را به صورت صحیح وارد نمایید" });
            }
            var user = await db.Users.FirstOrDefaultAsync(a => a.MobileNumber == number);
            if (user == null)
            {
                user = Entities.User.Create(number, UserType.Customer);
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }

            var code = await smsService.SendOTP(number, Messages.Messages.Signup.LoginOTPMessage);
            UserActivationCode userCode = new UserActivationCode()
            {
                UserId = user.Id,
                Code = code,
                IssueTime = DateTime.Now,
                ExpireTime = DateTime.Now.AddMinutes(5),
            };
            db.UserActivationCodes.Add(userCode);
            await db.SaveChangesAsync();
            return Json(new { success = true, msg = "ارسال با موفقیت انجام شد", id = userCode.Id });
        }

        public IActionResult Confirm(int id)
        {
            VerifyRequest vm = new VerifyRequest();
            vm.Id = id;
            return PartialView(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(VerifyRequest vm)
        {
            var userCodes = db.UserActivationCodes.FirstOrDefault(a => a.Id == vm.Id);
            var user = await db.Users.FirstOrDefaultAsync(a => a.Id == userCodes.UserId);
            if (user == null)
            {
                return Json(new { success = false, msg = "درخواست نا معتبر می باشد" });
            }
            if (userCodes.Code != vm.Code)
            {
                return Json(new { success = false, msg = "کد ارسالی نامعتبر می باشد" });
            }
            if (userCodes.ExpireTime < DateTime.Now)
            {
                return Json(new { success = false, msg = "زمان ثبت درخواست به اتمام رسیده است" });
            }

            var authResp = await userService.Authenticate(user, GetIpAddress());


            Response.Cookies.Append("MahtaAuth", authResp.JwtToken, new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                HttpOnly =true
            });

            return Json(new { Success = true });
        }

        public IActionResult ResultRequest()
        {
            return PartialView();
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }

    }
}