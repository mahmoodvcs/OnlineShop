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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Z.EntityFramework.Plus;

namespace MahtaKala.Controllers
{
    public class AccountController : SiteControllerBase<AccountController>
    {

        private readonly IUserService userService;
        private readonly ISMSService smsService;
        private readonly IConfiguration configuration;

        public AccountController(
            ISMSService smsService,
            IUserService userService,
            DataContext dataContext,
            IConfiguration configuration,
            ILogger<AccountController> logger, IHttpContextAccessor contextAccessor) : base(dataContext, logger)
        {
            this.smsService = smsService;
            this.userService = userService;
            this.configuration = configuration;
            this.contextAccessor = contextAccessor;
        }

        private readonly IHttpContextAccessor contextAccessor;
        public HttpContext Current => contextAccessor.HttpContext;

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string sessionId = Current.Session.Id;
            var user = await db.Users.Where(u => u.Username == model.UserName).FirstAsync();
            if (user == null || !user.VerifyPassword(model.Password))
            {
                ModelState.AddModelError(string.Empty, "نام کاربری و یا رمز عبور اشتباه است.");
                return View(model);
            }
            var userService = new UserService(db, configuration, smsService);
            var authResp = await userService.Authenticate(user, GetIpAddress(), UserClient.WebSite);

            Response.Cookies.Append("MahtaAuth", authResp.JwtToken, new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                HttpOnly = true
            });

            db.ShppingCarts.Where(x => x.SessionId == sessionId).Update(x => new ShppingCart() { UserId = user.Id, SessionId = null });

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()
        {
            Response.Cookies.Delete("MahtaAuth");
            return RedirectToAction("Index", "Home");
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

            string sessionId = Current.Session.Id;
            var authResp = await userService.Authenticate(user, GetIpAddress(), UserClient.WebSite);
            db.ShppingCarts.Where(x => x.SessionId == sessionId).Update(x => new ShppingCart() { UserId = user.Id, SessionId = null });
            Response.Cookies.Append("MahtaAuth", authResp.JwtToken, new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                HttpOnly = true
            });

            return Redirect("~/");
        }

        public IActionResult ResultRequest()
        {
            return PartialView();
        }

        public IActionResult SignOut()
        {
            Response.Cookies.Delete("MahtaAuth");
            return RedirectToAction("index","home");
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