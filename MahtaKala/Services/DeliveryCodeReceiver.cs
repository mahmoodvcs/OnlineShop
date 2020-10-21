using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.GeneralServices.SMS;
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
            var order = db.Orders.FirstOrDefault(x => x.TrackNo.ToLower().Equals(body.ToLower()));
            string error;
            if (order == null)
                error = "خطا! سفارشی با این کد در سیستم ثبت نشده است.";
            if (order.State == OrderState.Paid || order.State == OrderState.Sent)
            {
                order.State = OrderState.Delivered;
                db.SaveChanges();
                Task.Run(async () =>
                {
                    await MyServiceProvider.ResolveService<OrderService>().ShareOrderPayment(order.Id, true);
                });
                smsService.Send(sender, OrderDeliveryCodeReceived);
                return;
            }
            if (order.State == OrderState.Delivered)
            {
                error = "خطا! این کد قبلاً دریافت شده است.";
            }
            if (order.State == OrderState.Initial || order.State == OrderState.CheckedOut)
            {
                error = "خطا! هزینه ی سفارش پرداخت نشده است.";
            }
            else
                error = "خطا! این سفارش قبلاً لغو شده است.";

            smsService.Send(sender, string.Format(InvalidDeliveryCodeReceived, error));
        }
    }
}
