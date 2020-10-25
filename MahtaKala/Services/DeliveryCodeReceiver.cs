using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.GeneralServices.SMS;
using MahtaKala.Infrustructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class DeliveryCodeReceiver : ISMSProcessor
    {
        public DeliveryCodeReceiver()
        {
        }

        public const string OrderDeliveryCodeReceived = "کد تأیید تحویل سفارش دریافت شد: {0}.";
        public const string InvalidDeliveryCodeReceived = "کد ارسال شده معتبر نیست: {0}.";

        public void SMSReceived(string sender, string body, DateTime receiveDate)
        {
            var db = MyServiceProvider.ResolveService<DataContext>();
            var smsService = MyServiceProvider.ResolveService<ISMSService>();
            var orderService = MyServiceProvider.ResolveService<OrderService>();
            var order = db.Orders.FirstOrDefault(x => x.TrackNo.ToLower().Equals(body.ToLower()));

            string error;
            if (order == null)
            {
                error = "خطا! سفارشی با این کد در سیستم ثبت نشده است.";
                smsService.Send(sender, string.Format(InvalidDeliveryCodeReceived, error));
                return;
            }

            try
            {
                orderService.SetOrderDelivered(order.Id).Wait();
            }
            catch(BadRequestException ex)
            {
                smsService.Send(sender, string.Format(InvalidDeliveryCodeReceived, ex.Message));
                throw;
            }
        }
    }
}
