using MahtaKala.Entities;
using MahtaKala.SharedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices.SMS
{
    public class PayamSMSV2 : SMSServiceBase, ISMSService
    {
        //public const string OrderDeliveryCodeReceived = "کد تأیید تحویل سفارش دریافت شد: {0}.";
        //public const string InvalidDeliveryCodeReceived = "کد ارسال شده معتبر نیست: {0}.";

        public PayamSMSV2(ILogger<PayamSMSV2> logger, DataContext db)
        {
            this.logger = logger;
            this.db = db;
        }

        const string OrganizationName = "kaspian556";
        const string UserName = "kaspian556";
        const string Password = "123456987";
        const string SenderNumber = "982000446";//"982000446000";
        const string ReceiverNumber = "982000556";
        private readonly ILogger<PayamSMSV2> logger;
        private readonly DataContext db;

        public override async Task<bool> Send(string number, string message)
        {
            PayamSMSV2Service.SMSAPIPortTypeClient cl = new PayamSMSV2Service.SMSAPIPortTypeClient();
            var result = await cl.SendAsync(OrganizationName, UserName, Password, SenderNumber, message, new string[] { number });
            logger.LogDebug($"SMS Sent to {number}. response: {result[0].ID}");
            if (long.TryParse(result[0].ID, out _))
                return true;
            throw new Exception("Error Sending SMS. Code: " + result[0].ID);
        }

        public override async Task ReadReceivedSMSs()
        {
            var lastId = db.ReceivedSMSs.Max(a => a.OperatorId);
            PayamSMSV2Service.SMSAPIPortTypeClient cl = new PayamSMSV2Service.SMSAPIPortTypeClient();
            var data = await cl.ViewReceiveAsync(OrganizationName, UserName, Password, ReceiverNumber, lastId);
            if (data.Length == 0)
                return;
            if(data.Length == 1 && data[0].Body == null && data[0].ID.StartsWith("E"))
            {
                var err = data[0].ID;
                if (err == "E7") //No new messages
                    return;
            }
            foreach (var item in data)
            {
                ReceivedSMS sms = new ReceivedSMS()
                {
                    Message = item.Body,
                    Date = Utilities.ParseDateTime(item.Date),
                    OperatorId = item.ID,
                    Sender = item.From
                };
                db.ReceivedSMSs.Add(sms);
                //SMSManager.SMSReceived(sms.Sender, sms.Message, sms.Date);
                // TODO: TEMP...

                // endof TODO: TEMP...
            }
            await db.SaveChangesAsync();
            await ReadReceivedSMSs();
        }

        //public const string OrderDeliveryCodeReceived = "کد تأیید تحویل سفارش دریافت شد: {0}.";
        //public const string InvalidDeliveryCodeReceived = "خطا! کد ارسال شده معتبر نیست: {0}.";

        //public async Task SMSReceived(string sender, string body, DateTime receiveDate)
        //{
        //    //var db = MyServiceProvider.ResolveService<DataContext>();
        //    //var smsService = MyServiceProvider.ResolveService<ISMSService>();
        //    //var orderService = MyServiceProvider.ResolveService<OrderService>();
        //    var order = db.Orders.FirstOrDefault(x => x.TrackNo.ToLower().Equals(body.ToLower()));

        //    string error;
        //    if (order == null)
        //    {
        //        error = "سفارشی با این کد در سیستم ثبت نشده است.";
        //        //smsService.
        //        await Send(sender, string.Format(InvalidDeliveryCodeReceived, error));
        //        return;
        //    }

        //    try
        //    {
        //        orderService.SetOrderDelivered(order.Id).Wait();
        //        smsService.Send(sender, string.Format(OrderDeliveryCodeReceived, order.TrackNo));
        //    }
        //    catch (BadRequestException ex)
        //    {
        //        smsService.Send(sender, string.Format(InvalidDeliveryCodeReceived, ex.Message));
        //        throw;
        //    }
        //}


        //readonly Dictionary<string, string> ErrorCodes=new Dictionary<string, string>
        //{
        //    {"E223","شماره اختصاصی وارد شده نامعتبر است" },

        //}
    }
}
