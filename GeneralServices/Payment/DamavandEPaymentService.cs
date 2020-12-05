using MahtaKala.Entities;
using MahtaKala.GeneralServices.Payment.Models;
using MahtaKala.SharedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices.Payment
{
	public class DamavandEPaymentService : IBankPaymentService
	{
		private const string TERMINUL_NUMBER = "70000234";
		private const string MERCHAND_NUMBER = "075735191000001";
		private const string SECURITY_KEY = "B3A22799F6F2C5D17C7FEB2DC04550192616D749";
		private const string REGISTERED_MERCHANT_WEBSITE = "https://mahtakala.ir";
		private const string REGISTERED_MERCHANT_IP = "91.99.73.126";

		private readonly IPathService pathService;
		private readonly DataContext dbContext;
		private readonly ILogger<IBankPaymentService> logger;

		private const string PAYMENT_REQUEST_URL = "https://ecd.shaparak.ir/ipg_ecd/PayRequest";
		private const string PAY_URL = "https://ecd.shaparak.ir/ipg_ecd/PayStart";
		private const string PAYMENT_CONFIRMATION_URL = "https://ecd.shaparak.ir/ipg_ecd/PayConfirmation";
		private const string PAYMENT_ROLLBACK_URL = "https://ecd.shaparak.ir/ipg_ecd/PayReverse";

		public DamavandEPaymentService(DataContext dbContext, IPathService pathService, ILogger<IBankPaymentService> logger)
		{
			this.dbContext = dbContext;
			this.pathService = pathService;
			this.logger = logger;
		}

		public string GetPayUrl(Entities.Payment payment)
		{
			return PAY_URL;
		}

		public string GetTokenSynchronous(Entities.Payment payment, string returnUrl)
		{
			if (payment == null)
				return "";
			// The formula is: checksum = sha1(TerminalNumber + BuyID + Amount.ToString() + Date + Time + RedirectURL + Key)
			//	P.S. here, the plus means "string concatenation"
			string dateFormatted = payment.RegisterDate.Date.ToString("yyyy/MM/dd");
			string timeFormatted = payment.RegisterDate.ToString("HH:mm");
			string checksumSeedString = TERMINUL_NUMBER + payment.Id.ToString() + ((long)payment.Amount).ToString() + dateFormatted + timeFormatted + returnUrl + SECURITY_KEY;
			string checksum = HashSHa1(checksumSeedString);

			var modelToBeSent = new DamavandPayRequestModel
			{
				TerminalNumber = TERMINUL_NUMBER,
				BuyID = payment.Id.ToString(),
				Amount = (long)payment.Amount,
				Date = dateFormatted,
				Time = timeFormatted,
				RedirectURL = returnUrl,
				Language_StronglyTyped = DamavandSupportedLanguage.fa,
				CheckSum = checksum
			};

			var webClient = new WebClient();

			webClient.Encoding = System.Text.Encoding.UTF8;
			webClient.Headers[HttpRequestHeader.ContentType] = "application/json;charset=utf-8";
			var modelToSendRaw = JsonConvert.SerializeObject(modelToBeSent);

			var tokenRequestRawResult = webClient.UploadString(PAYMENT_REQUEST_URL, modelToSendRaw);

			var tokenRequestResult = JsonConvert.DeserializeObject<DamavandIPGResult>(tokenRequestRawResult);
			if (tokenRequestResult.State == (int)DamavandIPGResultState.Failed)
			{
				logger.LogError($"Request for token faild! Sent model is as follows: {modelToSendRaw}");
				logger.LogError($"Also, here's their response: {tokenRequestRawResult}");
				throw new Exception($"Error in receiving token from the bank! Error data is as follows: ErrorCode: {tokenRequestResult.ErrorCode} - ErrorDescription: {tokenRequestResult.ErrorDescription}");
			}

			return tokenRequestResult.Res;
		}

		public async Task<string> GetToken(Entities.Payment payment, string returnUrl)
		{
			return await Task.Run(() => GetTokenSynchronous(payment, returnUrl));
		}

		public async Task<Entities.Payment> Paid(string bankReturnBody)
		{
			var dictionary = bankReturnBody.Split('&').ToDictionary(a => a.Split('=')[0].ToLower(), a => a.Split('=')[1]);
			var paymentRequestResult = new DamavandBeginPaymentResult();
			paymentRequestResult.BuyID = dictionary["buyid"];
			paymentRequestResult.Token = dictionary["token"];

			int paymentId = int.Parse(paymentRequestResult.BuyID);
			var payment = await dbContext.Payments.Include(x => x.Order).Where(x => x.Id == paymentId).FirstOrDefaultAsync();
			//bool paymentSuccessful = true;
			if (payment == null)
			{
				logger.LogError($"Invalid Payment.Id {paymentId}. Does not exist.");
				var roolbackResult = RollPaymentBack(paymentRequestResult.Token);
				var alternativePayment = await dbContext.Payments.Include(x => x.Order)
					.Where(x => x.PayToken.Equals(paymentRequestResult.Token)).FirstOrDefaultAsync();
				if (alternativePayment != null)
				{
					payment = alternativePayment;
					payment.State = PaymentState.Failed;
					payment.Order.State = OrderState.Canceled;
					await dbContext.SaveChangesAsync();
					return payment;
				}
				else
				{
					throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
				}
				//paymentSuccessful = false;
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
			}
			if (payment.PayToken != paymentRequestResult.Token)
			{
				logger.LogError($"Payment token does not match. Payment.Id: {paymentId}. Ours: '{payment.PayToken}'. Bank: '{paymentRequestResult.Token}'");
				//var rollbackResult = RollPaymentBack(paymentRequestResult.Token);
				var rollbackResult = RollPaymentBack(payment.PayToken);
				payment.State = PaymentState.Failed;
				payment.Order.State = OrderState.Canceled;
				await dbContext.SaveChangesAsync();
				return payment;
				//paymentSuccessful = false;
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
			}
			int stateValue;
			if (!int.TryParse(dictionary["state"], out stateValue))
			{

				logger.LogError($"Incorrect \"State\" value received from the bank! It's supposed to be \"0\" or \"1\" (as opposed to {dictionary["state"]})");
				var roolbackResult = RollPaymentBack(paymentRequestResult.Token);
				payment.State = PaymentState.Failed;
				payment.Order.State = OrderState.Canceled;
				await dbContext.SaveChangesAsync();
				return payment;
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
				//paymentSuccessful = false;
			}
			paymentRequestResult.State = stateValue;
			if (paymentRequestResult.State != 1)
			{
				logger.LogError($"Payment not successful. pid: {payment.Id} - State: {paymentRequestResult.State}"
					+ $" - ErrorCode: {paymentRequestResult.ErrorCode} - ErrorDescription: {paymentRequestResult.ErrorDescription}");
				payment.State = PaymentState.Failed;
				payment.Order.State = OrderState.Canceled;
				dbContext.SaveChanges();
				return payment;
			}
			if (!dictionary.ContainsKey("referencenumber"))
			{
				logger.LogError($"Payment not successful! Key \"referencenumber\" not present in the request body!");
				var rollbackResult = RollPaymentBack(paymentRequestResult.Token);
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
				payment.State = PaymentState.Failed;
				payment.Order.State = OrderState.Canceled;
				await dbContext.SaveChangesAsync();
				return payment;
			}
			if (!dictionary.ContainsKey("trackingnumber"))
			{
				logger.LogError($"Payment not successful! Key \"trackingnumber\" not present in the request body!");
				var rollbackResult = RollPaymentBack(paymentRequestResult.Token);
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
				payment.State = PaymentState.Failed;
				payment.Order.State = OrderState.Canceled;
				await dbContext.SaveChangesAsync();
				return payment;
			}
			if (!dictionary.ContainsKey("amount"))
			{
				logger.LogError($"Payment not successful! Key \"amount\" not present in the request body!");
				var rollbackResult = RollPaymentBack(paymentRequestResult.Token);
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
				payment.State = PaymentState.Failed;
				payment.Order.State = OrderState.Canceled;
				await dbContext.SaveChangesAsync();
				return payment;
			}

			paymentRequestResult.ReferenceNumber = dictionary["referencenumber"];
			paymentRequestResult.TrackingNumber = dictionary["trackingnumber"];
			string amountStr = dictionary["amount"];
			//paymentRequestResult.Amount = long.Parse(amountStr);

			if (!long.TryParse(amountStr, out long receivedAmountValue))
			{
				logger.LogError($"Invalid \"amount\" string received from the bank! It should be an integer " +
					$"(a number, greater than zero, without any decimal points). The vlaue received from the bank is: {amountStr}");
				var roolbackResult = RollPaymentBack(paymentRequestResult.Token);
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
				payment.State = PaymentState.Failed;
				payment.Order.State = OrderState.Canceled;
				await dbContext.SaveChangesAsync();
				return payment;
			}

			paymentRequestResult.Amount = receivedAmountValue;
			payment.ReferenceNumber = paymentRequestResult.ReferenceNumber;
			payment.TrackingNumber = paymentRequestResult.TrackingNumber;

			if (paymentRequestResult.Amount != (long)payment.Amount || payment.State != PaymentState.SentToBank)
			{
				//TechnoIPGWSClient ipgw = new TechnoIPGWSClient();

				//WSContext wsContext = new WSContext();

				//wsContext.UserId = username;
				//wsContext.Password = password;
				//ReverseMerchantTransParam param = new ReverseMerchantTransParam();
				//param.RefNum = refNum;
				//param.Token = token;
				//param.WSContext = wsContext;
				//var result = await ipgw.ReverseMerchantTransAsync(param);
				// TODO: Implement and call the function which cancels the payment process, i.e. calls 
				//		PayReverse service of ecd(Damavand), and returns the money to the user's bank account
				var roolbackResult = RollPaymentBack(paymentRequestResult.Token);
				if (paymentRequestResult.Amount != (long)payment.Amount)
				{
					logger.LogError($"Incorrect \"Amount\" value received from the bank! Amount value received from the bank: " +
						$"{paymentRequestResult.Amount} - Amount value recorded in Payment object: {payment.Amount}");
				}
				if (payment.State != PaymentState.SentToBank)
				{
					logger.LogError($"Payment object has an incorrect \"State\" value! It's value should be \"SentToBank\", but, it's not! The current value: {payment.State}");
				}
				payment.State = PaymentState.Failed;
				payment.Order.State = OrderState.Canceled;
				await dbContext.SaveChangesAsync();
				return payment;
			}
			else
			{
				payment.State = PaymentState.PaidNotVerified;
				await dbContext.SaveChangesAsync();
				var confirmationResult = ConfirmPayment(paymentRequestResult.Token);
				if (confirmationResult.State == (int)DamavandIPGResultState.Succeeded)
				{
					payment.State = PaymentState.Succeeded;
					payment.Order.State = OrderState.Paid;
				}
				else
				{
					logger.LogError($"Payment confirmation not successful! PaymentId: {payment.Id}" +
						$" - Confirmation Process Info; State: {(int)confirmationResult.State} " +
						$"- ErrorCode: {confirmationResult.ErrorCode} " +
						$"- ErrorDescription: {confirmationResult.ErrorDescription}");
				}
			}
			await dbContext.SaveChangesAsync();
			return payment;
		}

		public Task SharePayment(Entities.Payment payment, List<PaymentShareDataItem> items)
		{
			throw new NotImplementedException();
		}

		private DamavandIPGResult ConfirmPayment(string token)
		{
			//try
			//{
			var modelToSend = new { Token = token };

			var webClient = new WebClient();
			webClient.Encoding = System.Text.Encoding.UTF8;
			webClient.Headers[HttpRequestHeader.ContentType] = "application/json;charset=utf-8";

			var modelToSendRawString = JsonConvert.SerializeObject(modelToSend);

			var confirmationResultRawString = webClient.UploadString(PAYMENT_CONFIRMATION_URL, modelToSendRawString);

			var result = JsonConvert.DeserializeObject<DamavandIPGResult>(confirmationResultRawString);
			return result;
			//}
			//catch (Exception)
			//{

			//}
		}

		private DamavandIPGResult RollPaymentBack(string token)
		{
            //try
            //{
            var modelToSend = new { Token = token };

            var webClient = new WebClient();
            webClient.Encoding = System.Text.Encoding.UTF8;
            webClient.Headers[HttpRequestHeader.ContentType] = "application/json;charset=utf-8";
            var modelToSendRaw = JsonConvert.SerializeObject(modelToSend);
            var resultRawString = webClient.UploadString(PAYMENT_ROLLBACK_URL, modelToSendRaw);
            var result = JsonConvert.DeserializeObject<DamavandIPGResult>(resultRawString);

            return result;
            //}
            //catch (Exception)
            //{
            //}
		}

		private string HashSHa1(string input)
		{
			using (SHA1Managed sha1 = new SHA1Managed())
			{
				var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
				var sb = new StringBuilder(hash.Length * 2);

				foreach (byte b in hash)
				{
					// can be "x2" if you want lowercase
					sb.Append(b.ToString("X2"));
				}

				return sb.ToString();
			}
		}
	}
}
