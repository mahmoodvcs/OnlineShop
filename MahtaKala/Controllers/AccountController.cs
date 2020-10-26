using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus.Extensions;
using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure;
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
        private readonly OrderService orderService;

        public AccountController(
            ISMSService smsService,
            IUserService userService,
            DataContext dataContext,
            IConfiguration configuration,
            OrderService orderService,
            ILogger<AccountController> logger, IHttpContextAccessor contextAccessor) : base(dataContext, logger)
        {
            this.smsService = smsService;
            this.userService = userService;
            this.configuration = configuration;
            this.orderService = orderService;
            this.contextAccessor = contextAccessor;
        }

        private readonly IHttpContextAccessor contextAccessor;

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

            var user = await db.Users.Where(u => u.Username == model.UserName).FirstOrDefaultAsync();
            if (user == null || !user.VerifyPassword(model.Password))
            {
                ModelState.AddModelError(string.Empty, Messages.Messages.UserErrors.Login_InvalidUserNameOrPassword);
                return View(model);
            }
            await userService.Authenticate(user, GetIpAddress(), UserClient.WebSite);

            await orderService.MigrateAnonymousUserShoppingCart(user.Id);
            
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()
        {
            userService.Logout();
            return RedirectToAction("Index", "Home");
        }

        TimeSpan GetSignupWaitMinuts()
        {
            var attempts = HttpContext.Session.GetInt32("SignupAttemps");
            var lastSignup = HttpContext.Session.Get<DateTime?>("SignupLastAttemp");
            if (lastSignup != null)
            {
                double waitMins;
                if (attempts < 4)
                    waitMins = 1;
                else
                    waitMins = Math.Pow(2, attempts.Value - 3);
                if (lastSignup.Value.AddMinutes(waitMins) > DateTime.Now)
                    return lastSignup.Value.AddMinutes(waitMins) - DateTime.Now;
            }
            return TimeSpan.Zero;
        }
        void SetSignupAttemp()
        {
            var attempts = HttpContext.Session.GetInt32("SignupAttemps");
            attempts = (attempts ?? 0) + 1;
            HttpContext.Session.Set<DateTime?>("SignupLastAttemp", DateTime.Now);
            HttpContext.Session.SetInt32("SignupAttemps", attempts.Value);
        }

        public IActionResult FirstRequest(string number)
        {
            SignupRequest vm = new SignupRequest();
            vm.Mobile = number;
            var wait = GetSignupWaitMinuts();
            if (wait > TimeSpan.Zero)
            {
                vm.Error = string.Format(Messages.Messages.Signup.MaxSignupAttemptReached, wait.Minutes, wait.Seconds);
            }
            return PartialView(vm);
        }

        [HttpPost]
        public async Task<IActionResult> FirstRequest(SignupRequest vm)
        {
            var wait = GetSignupWaitMinuts();
            if (wait > TimeSpan.Zero)
                return Json(new { success = false, msg = string.Format(Messages.Messages.Signup.MaxSignupAttemptReached, wait.Minutes, wait.Seconds) });

            var number = Util.NormalizePhoneNumber(vm.Mobile);
            if (!System.Text.RegularExpressions.Regex.IsMatch(number, @"^(\+98|0)?9\d{9}$"))
            {
                return Json(new { success = false, msg = Messages.Messages.UserErrors.PhoneNumber_Incorrect });
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
                ExpireTime = DateTime.Now.AddMinutes(2),
                AdditionalData = number
            };
            db.UserActivationCodes.Add(userCode);
            await db.SaveChangesAsync();

            SetSignupAttemp();

            return Json(new { success = true, msg = Messages.Messages.UserMessages.Request_Submit_Successful, id = userCode.Id });
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
                return Json(new { success = false, msg = Messages.Messages.UserErrors.ConfirmPhoneNumber_UserNotFound });
            }
            if (userCodes.Code != vm.Code)
            {
                return Json(new { success = false, msg = Messages.Messages.UserErrors.ConfirmPhoneNumber_InvalidCodeEntered });
            }
            if (userCodes.ExpireTime < DateTime.Now)
            {
                return Json(new { success = false, msg = Messages.Messages.UserErrors.ConfirmPhoneNumber_CodeHasBeenExpired });
            }

            var authResp = await userService.Authenticate(user, GetIpAddress(), UserClient.WebSite);

            await orderService.MigrateAnonymousUserShoppingCart(user.Id);

            return Json(new { success = true });
        }


        [HttpPost]
        public async Task<IActionResult> ResendCode(int id)
        {
            var userActivationCode = db.UserActivationCodes.Include("User").FirstOrDefault(a => a.Id == id);
            var code = await smsService.SendOTP(userActivationCode.User.MobileNumber, Messages.Messages.Signup.LoginOTPMessage);
            userActivationCode.Code = code;
            userActivationCode.ExpireTime = DateTime.Now.AddMinutes(2);
            await db.SaveChangesAsync();
            return Json(new { success = true });
        }

        public IActionResult ResultRequest()
        {
            return PartialView();
        }

        public IActionResult SignOut()
        {
            userService.Logout();
            return RedirectToAction("index", "home");
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