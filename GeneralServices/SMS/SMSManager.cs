using MahtaKala.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices.SMS
{
    public class SMSManager
    {
        private readonly IEnumerable<ISMSProcessor> SMSProcessors;
		private readonly ISMSService smsService;
		private readonly DataContext db;
		private readonly ILogger<SMSManager> logger;

		public const string TEMP_MARK = "$Markham(#_>";
		public const string OrderDeliveryCodeReceived = "کد تأیید تحویل سفارش دریافت شد: {0}.";
		public const string InvalidDeliveryCodeReceived = "خطا! کد ارسال شده معتبر نیست: {0}.";
		//public IReadOnlyList<Type> SMSProcessorServiceTypes { get; } = new List<Type>();

		//private IHttpContextAccessor contextAccessor;
		//      private HttpContext Current => contextAccessor.HttpContext;

		public SMSManager(IEnumerable<ISMSProcessor> processors
			, ISMSService smsSrvc
			, DataContext dataContext
			, ILogger<SMSManager> logger)
		{
            //contextAccessor = accessor;
            SMSProcessors = processors;
			smsService = smsSrvc;
			db = dataContext;
			this.logger = logger;
		}

		//public void SetHttpContextAccessor(IHttpContextAccessor accessor)
		//{
		//    throw new NotImplementedException();
		////    contextAccessor = accessor;
		//}

		//public static void RegisterProcessor(ISMSProcessor processor)
		//public void RegisterProcessorType(Type processorType)
		//{
		//    if (!typeof(ISMSProcessor).IsAssignableFrom(processorType))
		//        return;
		//    ((List<Type>)SMSProcessorServiceTypes).Add(processorType);
		//    //var processor = (ISMSProcessor)Current.RequestServices.GetService(processorType);
		//    //((List<ISMSProcessor>)SMSPorcessors).Add(processor);
		//}

		public async Task FetchAndProcessReceivedSMSes()
		{
			var smses = await smsService.GetReceivedSMSes();
			foreach (var sms in smses)
			{
				db.ReceivedSMSs.Add(sms);
				SMSReceived(sms.Sender, sms.Message, sms.Date);
				await db.SaveChangesAsync();
			}

		}

        private void SMSReceived(string sender, string body, DateTime receiveDate)
        {
			//foreach (var processorType in SMSProcessorServiceTypes)
			//{
			//             IProvider<processorType>
			//             //var processor = (ISMSProcessor)Current.RequestServices.GetService(processorType);
			//             try
			//             {
			//                 processor.SMSReceived(sender, body, receiveDate);
			//             }
			//             catch (Exception e)
			//             {
			//                 // TODO: log
			//                 throw e;
			//             }
			//}
			foreach (var sp in SMSProcessors)
			{
				try
				{
					string trackingNumber = sp.SMSReceived(sender, body, receiveDate);
					if (!string.IsNullOrWhiteSpace(trackingNumber))
						smsService.Send(sender, string.Format(OrderDeliveryCodeReceived, trackingNumber));
					else
					{
						smsService.Send(sender, string.Format(InvalidDeliveryCodeReceived, "خطای نامعلوم!"));
					}
				}
				catch (Exception ex)
				{
					string wholeError = ex.Message;
					var exceptionDigger = ex;
					while (exceptionDigger.InnerException != null)
					{
						exceptionDigger = exceptionDigger.InnerException;
						wholeError += exceptionDigger.Message;
					}
					logger.LogError($"SMSManager.SMSReceived - Exception: {wholeError}");
					while (exceptionDigger != null && !exceptionDigger.Message.StartsWith(TEMP_MARK))
					{
						exceptionDigger = exceptionDigger.InnerException;
					}
					//if (recursiveException.Message.StartsWith(TEMP_MARK))
					string userError = body + ": ";
					if (exceptionDigger != null)
					{
						if (exceptionDigger.Message.StartsWith(TEMP_MARK))
							userError += exceptionDigger.Message.Substring(TEMP_MARK.Length, exceptionDigger.Message.Length - TEMP_MARK.Length);
						else
							userError += ex.Message;
						smsService.Send(sender, string.Format(InvalidDeliveryCodeReceived, userError));
					}
					else
					{
						smsService.Send(sender, string.Format(InvalidDeliveryCodeReceived, userError + ex.Message));
					}
				}
			}
		}
    }
}
