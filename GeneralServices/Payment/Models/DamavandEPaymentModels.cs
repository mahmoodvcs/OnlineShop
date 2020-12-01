using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MahtaKala.GeneralServices.Payment.Models
{
	public class DamavandPayRequestModel
	{
		private DamavandSupportedLanguage? _language;


		public string TerminalNumber { get; set; }
		/// <summary>
		/// Unicity should be forced on this property!
		/// </summary>
		[StringLength(40)]
		public string BuyID { get; set; }
		public long Amount { get; set; }
		/// <summary>
		/// Gregorian date in the format yyyy/mm/dd
		/// </summary>
		public string Date { get; set; }
		/// <summary>
		/// Time of day in the format HH:mm
		/// </summary>
		public string Time { get; set; }
		public string RedirectURL { get; set; }
		public DamavandSupportedLanguage Language_StronglyTyped
		{
			get
			{
				if (!_language.HasValue)
					return DamavandSupportedLanguage.fa;
				return _language.Value;
			}
			set
			{
				_language = value;
			}
		}
		/// <summary>
		/// Returns either "fa" or "en", depending on the language set through the property "Language_StronglyTyped".
		/// Default value is "fa".
		/// </summary>
		public string Language
		{
			get
			{
				return Language_StronglyTyped.ToString();
			}
			set
			{
				if (value.Trim().ToLower().Equals("fa"))
				{
					Language_StronglyTyped = DamavandSupportedLanguage.fa;
				}
				else if (value.Trim().ToLower().Equals("en"))
				{
					Language_StronglyTyped = DamavandSupportedLanguage.en;
				}
			}
		}
		public string CheckSum { get; set; }
	}

	public enum DamavandSupportedLanguage
	{
		[Description("فارسی/Persian")]
		fa = 0,
		[Description("انگلیسی/English")]
		en = 1
	}

	public enum DamavandIPGResultState
	{ 
		Failed = 0,
		Succeeded = 1
	}

	public class DamavandIPGResult
    {
        public DamavandIPGResult()
        {
            this.ErrorCode = string.Empty;
            this.ErrorDescription = string.Empty;
            this.Res = string.Empty;
            this.State = 0;
        }

        public int State { get; set; }
        public string Res { get; set; }
        public string ErrorDescription { get; set; }
        public string ErrorCode { get; set; }
    }

    public class DamavandBeginPaymentResult
    {
        public string Token { get; set; }
        public int State { get; set; }
        public string ErrorCode { get; set; }
        public string BuyID { get; set; }
        public string TrackingNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public string ErrorDescription { get; set; }
        public long Amount { get; set; }
    }
}
