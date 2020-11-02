using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Infrustructure.Extensions;
using MahtaKala.Models;
using MahtaKala.Models.UserModels;
using MahtaKala.Services;
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
		  ILogger<ProfileController> logger,
		  IProductImageService productImageService,
		  ISMSService smsService
		  ) : base(dataContext, logger)
		{
			this.productImageService = productImageService;
			this.smsService = smsService;
		}
		private User user;
		private readonly IProductImageService productImageService;
		private readonly ISMSService smsService;

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
				string.IsNullOrEmpty(User.EmailAddress))
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
			var lst = db.Wishlists.Include(a => a.Product.Prices).Where(a => a.UserId == UserId).OrderByDescending(x => x.Id).ToList();
			return View(lst);
		}

		public async Task<IActionResult> BuyHistory()
		{
			var query = db.Orders.Where(o => (o.State == OrderState.Paid ||
												o.State == OrderState.Delivered ||
												o.State == OrderState.Sent) && o.UserId == UserId)
								.OrderByDescending(x => x.CheckOutDate);
			var data = await OrderModel.Get(query, productImageService);
			return View(data);
		}

		public async Task<IActionResult> ProfileEdit()
		{
			var user = await db.Users.FirstOrDefaultAsync(a => a.Id == UserId);
			var addresses = await db.Addresses.Include(a => a.City).Where(a => a.UserId == UserId && !a.Disabled).ToListAsync();
			UserDataVM vm = new UserDataVM
			{
				EmailAddress = user.EmailAddress,
				FirstName = user.FirstName,
				LastName = user.LastName,
				NationalCode = user.NationalCode,
				Addresses = addresses,
				Mobile = user.MobileNumber
			};
			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> ProfileEdit(UserDataVM vm)
		{
			if (string.IsNullOrEmpty(vm.FirstName))
				return Json(new { success = false, msg = Messages.Messages.UserErrors.UserFirstNameEmpty });
			if (Util.IsAnyNumberInString(vm.FirstName))
				return Json(new { success = false, msg = Messages.Messages.UserErrors.UserFirstNameContainsDigits });
			if (string.IsNullOrEmpty(vm.LastName))
				return Json(new { success = false, msg = Messages.Messages.UserErrors.UserLastNameEmpty });
			if (Util.IsAnyNumberInString(vm.LastName))
				return Json(new { success = false, msg = Messages.Messages.UserErrors.UserLastNameContainsDigits });
			if (!string.IsNullOrEmpty(vm.EmailAddress))
			{
				if (!Util.IsValidEmailaddress(vm.EmailAddress))
					return Json(new { success = false, msg = Messages.Messages.UserErrors.UserEmailNotCorrect });
			}

			if (!string.IsNullOrEmpty(vm.NationalCode))
			{
				string msg;
				if (!Util.IsCheckNationalCode(vm.NationalCode, out msg))
					return Json(new { success = false, msg });
			}

			User user = await db.Users.FirstOrDefaultAsync(a => a.Id == UserId);
			user.FirstName = vm.FirstName;
			user.EmailAddress = vm.EmailAddress;
			user.LastName = vm.LastName;
			user.NationalCode = vm.NationalCode;
			//user.MobileNumber = vm.Mobile;
			//if (!user.MobileNumber.Equals(vm.Mobile))
			//{
			//	int code = await smsService.SendOTP(vm.Mobile, Messages.Messages.ConfirmPhoneNumberMessage);
			//	while (code == 0)
			//	{
			//		code = await smsService.SendOTP(vm.Mobile, Messages.Messages.ConfirmPhoneNumberMessage);
			//	}
			//	var confirmationCode = new UserActivationCode()
			//	{
			//		UserId = user.Id,
			//		Code = code,
			//		IssueTime = DateTime.Now,
			//		ExpireTime = DateTime.Now.AddMinutes(2),
			//		AdditionalData = vm.Mobile
			//	};
			//	await db.SaveChangesAsync();
			//}
			//else
			//{
			await db.SaveChangesAsync();
			return Json(new { success = true, msg = Messages.Messages.UserMessages.Edit_Successful, name = user.FullName() });
			//}
		}

		public IActionResult EditPhoneNumber(string number)
		{
			var user = db.Users.FirstOrDefault(x => x.Id == User.Id);
			//if (user == null)
			//{ }
			var requestModel = new SignupRequest()
			{ Mobile = user.MobileNumber };
			if (!string.IsNullOrWhiteSpace(number))
			{
				requestModel.Mobile = number;
			}
			return View(requestModel);
		}

		[HttpPost]
		public async Task<IActionResult> EditPhoneNumber(SignupRequest requestModel)
		{
			var number = Util.NormalizePhoneNumber(requestModel.Mobile);
			if (!System.Text.RegularExpressions.Regex.IsMatch(number, @"^(\+98|0)?9\d{9}$"))
			{
				return Json(new { success = false, msg = Messages.Messages.UserErrors.PhoneNumber_Incorrect });
			}
			var user = await db.Users.FirstOrDefaultAsync(x => x.Id == User.Id);
			if (number.Equals(user.MobileNumber))
			{
				return Json(new { success = false, msg = Messages.Messages.UserErrors.PhoneNumber_Duplicate });
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

			return Json(new { success = true, msg = Messages.Messages.UserMessages.Request_Submit_Successful, id = userCode.Id });
		}

		public IActionResult ConfirmPhoneNumber(int id)
		{
			var confirmRequestModel = new VerifyRequest();
			confirmRequestModel.Id = id;
			return PartialView(confirmRequestModel);
		}

		[HttpPost]
		public async Task<IActionResult> ConfirmPhoneNumber(VerifyRequest verifyRequest)
		{
			var userCode = db.UserActivationCodes.FirstOrDefault(a => a.Id == verifyRequest.Id);
			var user = await db.Users.FirstOrDefaultAsync(a => a.Id == userCode.UserId);
			if (user == null)
			{
				return Json(new { success = false, msg = Messages.Messages.UserErrors.ConfirmPhoneNumber_UserNotFound });
			}
			if (userCode.Code != verifyRequest.Code)
			{
				return Json(new { success = false, msg = Messages.Messages.UserErrors.ConfirmPhoneNumber_InvalidCodeEntered });
			}
			if (userCode.ExpireTime < DateTime.Now)
			{
				return Json(new { success = false, msg = Messages.Messages.UserErrors.ConfirmPhoneNumber_CodeHasBeenExpired });
			}

			user.MobileNumber = userCode.AdditionalData;
			db.Entry(user).State = EntityState.Modified;
			await db.SaveChangesAsync();

			return Json(new { success = true });
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
			if (string.IsNullOrWhiteSpace(address.Title))
				return Json(new { success = false, msg = Messages.Messages.UserErrors.AddressInput_Title_Empty });
			if (string.IsNullOrWhiteSpace(address.PostalCode))
				return Json(new { success = false, msg = Messages.Messages.UserErrors.AddressInput_POBox_Empty });
			address.PostalCode = address.PostalCode.Trim();
			if (address.PostalCode.Length != 10 || !(address.PostalCode.ContainsOnlyDigits()))
				return Json(new { success = false, msg = Messages.Messages.UserErrors.AddressInput_POBox_NotDigits });
			if (address.CityId == 0)
				return Json(new { success = false, msg = Messages.Messages.UserErrors.AddressInput_City_Empty });
			if (string.IsNullOrWhiteSpace(address.Details))
				return Json(new { success = false, msg = Messages.Messages.UserErrors.AddressInput_AddressText_Empty });
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
			ModelState.Clear();
			return Json(new { success = true, msg = Messages.Messages.UserMessages.Edit_Successful });
			//return RedirectToAction("ProfileEdit");
		}

		public IActionResult RemoveAddress(long id)
		{
			var p = db.Addresses.FirstOrDefault(a => a.Id == id);
			if (db.Orders.Where(a => a.AddressId == id).Any())
				p.Disabled = true;
			else
				db.Addresses.Remove(p);
			db.SaveChanges();
			return RedirectToAction("ProfileEdit");
		}
	}
}