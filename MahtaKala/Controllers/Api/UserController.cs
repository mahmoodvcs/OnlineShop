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
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace MahtaKala.Controllers
{
    [ApiController()]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1")]
    public class UserController : ControllerBase
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

        /// <summary>
        /// Start the login proccess. Currently only supported method is OTP (One-time password) that will be sent to the users mobile phone.
        /// </summary>
        /// <param name="signupRequest">Contains the mobile phone number</param>
        /// <returns></returns>
        /// <response code="200">Success. The user is allready registered and the OTP is sent</response>
        /// <response code="201">Success. The user is new. The OTP is sent</response>
        [HttpPost]
        public async Task<IActionResult> Signup([FromBody] SignupRequest signupRequest)
        {
            var number = Util.NormalizePhoneNumber(signupRequest.Mobile);
            bool newUser = false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(number, @"^(\+98|0)?9\d{9}$"))
            {
                throw new Exception("Wrong Phone Entered.");
            }
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

        /// <summary>
        /// The second (last) phase of the login or signup proccess.
        /// </summary>
        /// <param name="verifyRequest">Contains the mobile number and the OTP</param>
        /// <returns>Access token, refresh token and the user info</returns>
        [HttpPost]
        public async Task<VerifyRespnse> Verify([FromBody] VerifyRequest verifyRequest)
        {
            var number = Util.NormalizePhoneNumber(verifyRequest.Mobile);

            var user = await db.Users.FirstOrDefaultAsync(a => a.MobileNumber == number);
            if (user == null)
            {
                Response.StatusCode = 401;
                return null;
            }

            var userCodes = await db.UserActivationCodes.Where(a => a.UserId == user.Id).ToListAsync();
            if (!userCodes.Any())
            {
                Response.StatusCode = 401;
                return null;
            }

            if (!userCodes.Any(a => a.ExpireTime > DateTime.Now && a.Code == verifyRequest.Code))
            {
                Response.StatusCode = 401;
                return null;
            }

            var tokens = await userService.Authenticate(user, GetIpAddress());

            return new VerifyRespnse
            {
                Refresh = tokens.RefreshToken,
                Access = tokens.JwtToken,
                User = new UserInfo
                {
                    Mobile = number
                }
            };
        }

        /// <summary>
        /// Creates a new refresh and access token for JWT authorization.
        /// </summary>
        /// <param name="refreshRequest">Contains the old refresh token</param>
        /// <returns>New tokens</returns>
        [HttpPost]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest refreshRequest)
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

        /// <summary>
        /// Used to logout the user. It does nothing on the server side. Because we use JWT for authentication, nothing is stored on the server.
        /// Client code should remove the JWT token from cookie or LocalStorage.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">The user was logged in. Clear the client token.</response>
        /// <response code="401">JWT token is not valid. Clear the client token anyway.</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(401)]
        [ProducesResponseType(200)]
        public void Logout()
        {
            //userService.RevokeToken()
            return;
        }

        /// <summary>
        /// Updates the user's profile information
        /// </summary>
        /// <param name="profileModel">User profile info</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile([FromBody] ProfileModel profileModel)
        {
            var user = (User)HttpContext.Items["User"];
            user.FirstName = profileModel.Name;
            user.LastName = profileModel.Family;
            user.NationalCode = profileModel.National_Code;
            user.EmailAddress = profileModel.EMail;
            await userService.Update(user);
            return StatusCode(200);
        }

        /// <summary>
        /// Returns the user's profile information
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public ProfileModel Profile()
        {
            var user = (User)HttpContext.Items["User"];
            return new ProfileModel
            {
                Name = user.FirstName,
                Family = user.LastName,
                National_Code = user.NationalCode,
                EMail = user.EmailAddress,
            };
        }

        /// <summary>
        /// Returns list of addresses for current user
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<List<AddressModel>> Address()
        {
            var list = db.Addresses.Where(a => a.UserId == UserId);
            return await list.Select(a => new AddressModel
            {
                Id = a.Id,
                City = a.CityId,
                Province = a.City.ProvinceId,
                Details = a.Details,
                Postal_Code = a.PostalCode,
                Lat = a.Location.Coordinate.Y,
                Lng = a.Location.X
            }).ToListAsync();
        }

        /// <summary>
        /// Updates or creates an address for a user
        /// </summary>
        /// <param name="addressModel"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<StatusCodeResult> Address(AddressModel addressModel)
        {
            UserAddress address;
            if (addressModel.Id == 0)
            {
                address = new UserAddress();
                address.UserId = UserId;
                db.Addresses.Add(address);
            }
            else
            {
                address = db.Addresses.Find(addressModel.Id);
                if (address == null)
                    throw new Exception("Address not found");
                if (address.UserId != UserId)
                    return StatusCode(403);
            }

            address.PostalCode = addressModel.Postal_Code;
            address.CityId = addressModel.City;
            address.Details = addressModel.Details;
            address.Location = GeoUtil.CreatePoint(addressModel);

            await db.SaveChangesAsync();
            return StatusCode(200);
        }

        /// <summary>
        /// Deletes an address for a user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        public async Task<StatusCodeResult> Address(IdModel model)
        {
            var address = db.Addresses.Find(model.Id);
            if (address == null)
                throw new InvalidOperationException("Address not found.");
            db.Addresses.Remove(address);
            await db.SaveChangesAsync();
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