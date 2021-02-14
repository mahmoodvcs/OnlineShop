﻿using MahtaKala.Entities;
using MahtaKala.GeneralServices.Payment.Models;
using MahtaKala.GeneralServices.Payment.PardakhtNovin;
using MahtaKala.SharedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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

		const string ShareApiAddress = "http://178.252.189.82:9000/api/SettlementRequest";
		const string inqueiryAddress = "http://178.252.189.82:9000/api/InquiryTransactionSettlement";
		const string shareUsername = "mahtaws";
		const string sharePassword = "Ab123456";

		private async Task<string> Post(string address, string body)
		{
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));
			//client.DefaultRequestHeaders.Add("Content-Type", "application/json");
			client.DefaultRequestHeaders.Add("username", shareUsername);
			client.DefaultRequestHeaders.Add("password", sharePassword);

			var content = new StringContent(body, Encoding.UTF8, "application/json");
			var response = await client.PostAsync(address, content);
			var resStr = await response.Content.ReadAsStringAsync();
			return resStr;
		}

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
			var resultTokenLowerCase = paymentRequestResult.Token.ToLower();

			int paymentId = int.Parse(paymentRequestResult.BuyID);
			var payment = await dbContext.Payments.Include(x => x.Order).Where(x => x.Id == paymentId).FirstOrDefaultAsync();
			if (payment == null)
			{
				logger.LogError($"Invalid Payment.Id {paymentId}. Does not exist.");
				var rollBackResult = RollPaymentBack(paymentRequestResult.Token);
				var alternativePayment = await dbContext.Payments.Include(x => x.Order)
					.Where(x => x.PayToken.ToLower().Equals(resultTokenLowerCase)).FirstOrDefaultAsync();
				if (alternativePayment != null && rollBackResult.State == 1)
				{
					payment = alternativePayment;
					await PaymentFailureImminent(payment, dbContext);
					//payment.State = PaymentState.Failed;
					//payment.Order.State = OrderState.Canceled;
					//await dbContext.SaveChangesAsync();
					return payment;
				}
				else
				{
					throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
				}
				//paymentSuccessful = false;
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
			}
			if (payment.PayToken.ToLower() != resultTokenLowerCase)
			{
				logger.LogError($"Payment token does not match. Payment.Id: {paymentId}. Ours: '{payment.PayToken}'. Bank: '{paymentRequestResult.Token}'");
				var rollbackResult = RollPaymentBack(paymentRequestResult.Token);
				//var rollbackResult = RollPaymentBack(payment.PayToken);
				if (dbContext.Payments.Any(x => x.PayToken.ToLower().Equals(resultTokenLowerCase)))
				{
					var paymentObjectBasedOnToken = dbContext.Payments.Include(x => x.Order).First(x => x.PayToken.ToLower().Equals(resultTokenLowerCase));
					//var paymentBasedOnBuyId = payment;
					payment = paymentObjectBasedOnToken;
				}
				else
				{
					rollbackResult = RollPaymentBack(payment.PayToken);
				}
				if (rollbackResult.State == 1)
				{
					await PaymentFailureImminent(payment, dbContext);
				}
				//payment.State = PaymentState.Failed;
				//payment.Order.State = OrderState.Canceled;
				//await dbContext.SaveChangesAsync();
				return payment;
				//paymentSuccessful = false;
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
			}
			int stateValue;
			string stateString;
			if (!dictionary.TryGetValue("state", out stateString) || !int.TryParse(stateString, out stateValue))
			{
				string logMessage = $"Incorrect \"State\" value received from the bank! No matter the outcome of the payment process, " +
					$"this request body is supposed to contain a \"state\" key with its value either as \"0\" or \"1\"! ";
				if (!dictionary.ContainsKey("state"))
				{
					logMessage += $"But, the request body does not contain a \"state\" key whatsoever!";
				}
				else
				{
					logMessage += $"But, the value for \"state\" key in the request body is {stateString}, which, is not parsable as an integer value!";
				}
				logger.LogError(logMessage);

				var rollBackResult = RollPaymentBack(paymentRequestResult.Token);
				if (rollBackResult.State == 1)
				{
					await PaymentFailureImminent(payment, dbContext);
				}
				//payment.State = PaymentState.Failed;
				//payment.Order.State = OrderState.Canceled;
				//await dbContext.SaveChangesAsync();
				return payment;
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
				//paymentSuccessful = false;
			}
			paymentRequestResult.State = stateValue;
			if (paymentRequestResult.State != 1)
			{
				logger.LogError($"Payment not successful. pid: {payment.Id} - State: {paymentRequestResult.State}"
					+ $" - ErrorCode: {paymentRequestResult.ErrorCode} - ErrorDescription: {paymentRequestResult.ErrorDescription}");
				//payment.State = PaymentState.Failed;
				//payment.Order.State = OrderState.Canceled;
				//dbContext.SaveChanges();
				await PaymentFailureImminent(payment, dbContext);
				return payment;
			}
			if (!dictionary.ContainsKey("referencenumber"))
			{
				logger.LogError($"Payment not successful! Key \"referencenumber\" not present in the request body!");
				var rollbackResult = RollPaymentBack(paymentRequestResult.Token);
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
				//payment.State = PaymentState.Failed;
				//payment.Order.State = OrderState.Canceled;
				//await dbContext.SaveChangesAsync();
				if (rollbackResult.State == 1)
				{
					await PaymentFailureImminent(payment, dbContext);
				}
				return payment;
			}
			if (!dictionary.ContainsKey("trackingnumber"))
			{
				logger.LogError($"Payment not successful! Key \"trackingnumber\" not present in the request body!");
				var rollbackResult = RollPaymentBack(paymentRequestResult.Token);
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
				//payment.State = PaymentState.Failed;
				//payment.Order.State = OrderState.Canceled;
				//await dbContext.SaveChangesAsync();
				if (rollbackResult.State == 1)
				{
					await PaymentFailureImminent(payment, dbContext);
				}
				return payment;
			}
			if (!dictionary.ContainsKey("amount"))
			{
				logger.LogError($"Payment not successful! Key \"amount\" not present in the request body!");
				var rollbackResult = RollPaymentBack(paymentRequestResult.Token);
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
				//payment.State = PaymentState.Failed;
				//payment.Order.State = OrderState.Canceled;
				//await dbContext.SaveChangesAsync();
				if (rollbackResult.State == 1)
				{
					await PaymentFailureImminent(payment, dbContext);
				}
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
				var rollbackResult = RollPaymentBack(paymentRequestResult.Token);
				//throw new Exception(ServiceMessages.Payment.InvalidBankResponse);
				//payment.State = PaymentState.Failed;
				//payment.Order.State = OrderState.Canceled;
				//await dbContext.SaveChangesAsync();
				if (rollbackResult.State == 1)
				{
					await PaymentFailureImminent(payment, dbContext);
				}
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
				var rollbackResult = RollPaymentBack(paymentRequestResult.Token);
				if (paymentRequestResult.Amount != (long)payment.Amount)
				{
					logger.LogError($"Incorrect \"Amount\" value received from the bank! Amount value received from the bank: " +
						$"{paymentRequestResult.Amount} - Amount value recorded in Payment object: {payment.Amount} - PaymentId: {payment.Id}");
				}
				if (payment.State != PaymentState.SentToBank)
				{
					logger.LogError($"Payment object has an incorrect \"State\" value! It's value should be \"SentToBank\", " +
						$"but, it's not! The current value: {payment.State} - PaymentId: {payment.Id}");
				}
				//payment.State = PaymentState.Failed;
				//payment.Order.State = OrderState.Canceled;
				//await dbContext.SaveChangesAsync();
				if (rollbackResult.State == 1)
				{
					await PaymentFailureImminent(payment, dbContext);
				}
				//payment.State = PaymentState.Failed;
				//payment.Order.State = OrderState.Canceled;
				//await dbContext.SaveChangesAsync();
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

		private async Task PaymentFailureImminent(Entities.Payment payment, DataContext dbContext)
		{
			if (payment.State != PaymentState.Failed && payment.State != PaymentState.Succeeded)
			{
				payment.State = PaymentState.Failed;
				//payment.Order.State = OrderState.Canceled;
				await dbContext.SaveChangesAsync();
			}
			else
			{
				if (payment.State == PaymentState.Succeeded)
				{
					var errorMessage = $"Payment with Id {payment.Id} seems to have been rolled-back, but, its state is already recorded as \"Succeeded\"!";
					logger.LogError(errorMessage);
					throw new Exception(errorMessage);
				}
			}
		}

		public async Task SharePayment(Entities.Payment payment, List<PaymentShareDataItem> items)
		{
			SettlementRequest request = new SettlementRequest()
			{
				referenceNumber = payment.ReferenceNumber,//new string('0', 12 - payment.PSPReferenceNumber.Length) + payment.PSPReferenceNumber,
				scatteredSettlement = items.GroupBy(a => a.ShabaId).Select(a => new ScatteredSettlementDetails
				{
					settlementIban = a.Key,
					shareAmount = (int)a.Sum(c => c.Amount)
				}).ToList()
			};
			// TODO: Each day, only 10 "shaba_id"s can be sent for settlement, and there should be a database table for recording the left-overs
			//if (request.scatteredSettlement.Count > 10)
			//{
			//	var req1 = new SettlementRequest()
			//	{ 
			//		referenceNumber = request.referenceNumber,
			//		scatteredSettlement = request.scatteredSettlement.GetRange(0, 10)
			//	};
			//	var req2 = new SettlementRequest()
			//	{
			//		referenceNumber = request.referenceNumber,
			//		scatteredSettlement = request.scatteredSettlement.GetRange(9, request.scatteredSettlement.Count - 10)
			//	};
			//	// and the rest of the thing, which includes planning (and implementing that plan) for the "List<PaymentSHareDataItem> items" 
			//	// variable, and how it should be splitted
			//}
			var date = DateTime.Now;
			var psItems = items.Select(a => new PaymentSettlement
			{
				Amount = (int)a.Amount,
				Date = date,
				Name = a.Name,
				OrderId = payment.OrderId,
				PaymentId = payment.Id,
				ShabaId = a.ShabaId,
				Status = PaymentSettlementStatus.Sent,
				PayFor = a.PayFor,
				ItemId = a.ItemId
			}).ToList();

			await dbContext.PaymentSettlements.AddRangeAsync(psItems);
			await dbContext.SaveChangesAsync();

			var reqSgtring = "[" + System.Text.Json.JsonSerializer.Serialize(request) + "]";
			logger.LogWarning($"Damavand.SharePayment - Sending request string: {reqSgtring}");
			var resStr = await Post(ShareApiAddress, reqSgtring);
			logger.LogWarning($"Damavand.SharePayment - Request sent. Result string: {resStr}");
			var responses = System.Text.Json.JsonSerializer.Deserialize<SettlementRequestResponse[]>(resStr);

			//if (responses.Length != request.scatteredSettlement.Count)
			//{
			//	logger.LogError($"Payment share response count mismatch. PaymentId: {payment.Id} - Request: {reqSgtring} \r\n\r\nResponse: {resStr}");
			//	throw new Exception($"Payment share response count mismatch. PaymentId: {payment.Id}");
			//}

			if (responses.Length == request.scatteredSettlement.Count)
			{
				for (int i = 0; i < responses.Length; i++)
				{
					foreach (var item in psItems.Where(a => a.ShabaId == request.scatteredSettlement[i].settlementIban))
					{
						psItems[i].Status = responses[i].status == StatusType.Succeed ? PaymentSettlementStatus.Succeeded : PaymentSettlementStatus.Failed;
						psItems[i].Response = responses[i].message;
					}
				}
			}
			else
			{
				if (responses.Length == 1)
				{
					if (responses[0].success && responses[0].status == StatusType.Succeed)
					{
						logger.LogWarning($"Damavand.SharePayment - PaymentId: {payment.Id} - Settlement request result was successful (but there's only one!");
						foreach (var item in psItems)
						{
							item.Status = PaymentSettlementStatus.Succeeded;
							item.Response = responses[0].message;
						}
					}
					else
					{
						// TODO: Throw Exception?
						logger.LogError($"Damavand.SharePayment - PaymentId: {payment.Id} - Settlement request failed! Response Success: {responses[0].success} " +
							$"- Response Status: {responses[0].status} - Response Message: {responses[0].message}");
						foreach (var item in psItems)
						{
							item.Status = PaymentSettlementStatus.Failed;
							item.Response = responses[0].message;
						}
					}
				}
				else
				{
					string errorMessage = $"Damavand.SharePayment PaymentId: {payment.Id} - Response count invalid! Number of response objects is {responses.Length}," +
						$" which is not one, and also, is not the same as request object count! The whole response is: {resStr}";
					logger.LogError(errorMessage);
					throw new Exception(errorMessage);
				}
			}
			await dbContext.SaveChangesAsync();
			logger.LogWarning("Damavand.SharePayment - Sharing process reached its end!");
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
