using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Messages
{
	public static partial class Messages
	{
		public static class UserErrors
		{
			public const string AddressInput_POBox_NotDigits = "کد پستی را به صورت 10 رقم وارد نمایید";
			public const string AddressInput_POBox_Empty = "لطفا کد پستی را وارد نمایید";
			public const string AddressInput_Title_Empty = "لطفا عنوان را وارد نمایید";
			public const string AddressInput_City_Empty = "لطفا شهر را انتخاب نمایید";
			public const string AddressInput_AddressText_Empty = "لطفا آدرس را وارد نمایید";

			public const string AddressNotSelected = "لطفا آدرس را انتخاب نمایید";
			public const string Checkout_CartEmpty = "سبد خرید خالی می باشد";
			public const string UserFirstNameEmpty = "لطفا نام را وارد کنید";
			public const string UserFirstNameContainsDigits = "لطفا برای نام از حروف استفاده نمایید";
			public const string UserLastNameEmpty = "لطفا نام خانوادگی را وارد کنید";
			public const string UserLastNameContainsDigits = "لطفا برای نام خانوادگی از حروف استفاده نمایید";
			public const string UserEmailNotCorrect = "لطفا ایمیل را به صورت صحیح وارد کنید";
			
			//public const string Checkout_UserNationalCodeEmpty = "لطفا کد ملی خود را وارد کنید";
			public const string NationalCode_Empty = "لطفا کد ملی خود را صحیح وارد نمایید";
			public const string NationalCode_IncorrectLength = "طول کد ملی باید ده کاراکتر باشد";
			public const string NationalCode_ContainsNonDigits = "کد ملی تشکیل شده از ده رقم عددی می‌باشد؛ لطفا کد ملی را صحیح وارد نمایید";
			public const string NationalCode_Incorrect = "کد ملی صحیح نیست";

			public const string ConfirmPhoneNumber_UserNotFound = "درخواست نا معتبر می باشد";
			public const string ConfirmPhoneNumber_InvalidCodeEntered = "کد ارسالی نامعتبر می باشد";
			public const string ConfirmPhoneNumber_CodeHasBeenExpired = "زمان ثبت درخواست به اتمام رسیده است";
			public const string PhoneNumber_Incorrect = "لطفا موبایل را به صورت صحیح وارد نمایید";
			public const string PhoneNumber_Duplicate = "شماره موبایل تکراری!";

			public const string Login_InvalidUserNameOrPassword = "نام کاربری و یا رمز عبور اشتباه است.";
		}
	}
}
