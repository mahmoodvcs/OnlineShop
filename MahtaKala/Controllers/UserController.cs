using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.Helpers;
using MahtaKala.Models;
using MahtaKala.Models.UserModels;
using MahtaKala.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MahtaKala.Controllers
{
    [ApiController()]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1")]
    public class UserController : Controller
    {

        private readonly ISMSService smsService;
        private readonly DataContext db;
        private readonly IUserService userService;
        public UserController(ISMSService smsService,
            DataContext context,
            IUserService userService)
        {
            this.smsService = smsService;
            this.db = context;
            this.userService = userService;
        }

        [HttpPost]

        public async Task<IActionResult> Signup([FromBody]SignupRequest signupRequest)
        {
            var number = Util.NormalizePhoneNumber(signupRequest.Mobile);
            bool newUser = false;
            var user = await db.Users.FirstOrDefaultAsync(a => a.MobileNumber == number);
            if (user == null)
            {
                newUser = true;
                user = Entities.User.Create(number, UserType.Customer);
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }

            var code = await smsService.SendOTP(number, "کد ورود به مهتا کالا:");
            UserActivationCode userCode = new UserActivationCode()
            {
                UserId = user.Id,
                Code = code,
                IssueTime = DateTime.Now,
                ExpireTime = DateTime.Now.AddMinutes(5),
            };
            db.UserActivationCodes.Add(userCode);
            await db.SaveChangesAsync();

            return StatusCode(newUser ? 201 : 200);
        }

        [HttpPost]
        public async Task<IActionResult> Verify([FromBody]VerifyRequest verifyRequest)
        {
            var number = Util.NormalizePhoneNumber(verifyRequest.Mobile);

            var user = await db.Users.FirstOrDefaultAsync(a => a.MobileNumber == number);
            if (user == null)
            {
                return StatusCode(401);
            }

            var userCodes = await db.UserActivationCodes.Where(a => a.UserId == user.Id).ToListAsync();
            if (!userCodes.Any())
            {
                return StatusCode(401);
            }

            if (!userCodes.Any(a => a.ExpireTime > DateTime.Now && a.Code == verifyRequest.Code))
            {
                return StatusCode(401);
            }

            var tokens = await userService.Authenticate(user, GetIpAddress());


            return Json(new
            {
                refresh = tokens.RefreshToken,
                access = tokens.JwtToken,
                user = new
                {
                    mobile = number
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Refresh([FromBody]RefreshRequest refreshRequest)
        {
            var tokens = await userService.RefreshToken(refreshRequest.Refresh, GetIpAddress());
            if (tokens == null)
            {
                return StatusCode(401);
            }

            return Json(new
            {
                access = tokens.JwtToken,
                refresh = tokens.RefreshToken
            });
        }

        [Authorize]
        public IActionResult Logout()
        {
            return StatusCode(200);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile([FromBody]ProfileModel profileModel)
        {
            var user = (User)HttpContext.Items["User"];
            user.FirstName = profileModel.Name;
            user.LastName = profileModel.Family;
            user.NationalCode = profileModel.National_Code;
            user.EmailAddress = profileModel.EMail;
            await userService.Update(user);
            return StatusCode(200);
        }






        #region Private Methods

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }

        #endregion Private Methods

    }
}