using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.Helpers;
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
        public UserController(ISMSService smsService, DataContext context)
        {
            this.smsService = smsService;
            this.db = context;
        }

        [HttpPost]

        public async Task<IActionResult> Signup(string mobile)
        {
            var number = Util.NormalizePhoneNumber(mobile);
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

        //public async Task<IActionResult> Verify(int code, string mobile)
        //{
        //    var number = Util.NormalizePhoneNumber(mobile);

        //    using (var db = new DataContext())
        //    {
        //        var userId = await db.Users.Where(a => a.MobileNumber == number).Select(a => a.Id).FirstOrDefaultAsync();
        //        if (userId == 0)
        //        {
        //            return StatusCode(401);
        //        }

        //        var userCodes = db.UserActivationCodes.Select(a => a.UserId).ToList();
        //        if()

        //    }
        //}

        public string Index()
        {
            return "Hi";
        }
    }
}