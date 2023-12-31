﻿using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.GeneralServices.SMS;
using MahtaKala.Infrustructure;
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
        //private readonly SingletonDataContext db;
        private readonly DataContext db;
        private readonly ISMSService smsService;
        private readonly OrderService orderService;
        public DeliveryCodeReceiver(
            //SingletonDataContext dbContext, 
            DataContext dbContext,
            ISMSService smsSrvc, 
            OrderService orderSrvc)
        {
            db = dbContext;
            smsService = smsSrvc;
            orderService = orderSrvc;
        }

        //public const string OrderDeliveryCodeReceived = "کد تأیید تحویل سفارش دریافت شد: {0}.";
        //public const string InvalidDeliveryCodeReceived = "خطا! کد ارسال شده معتبر نیست: {0}.";

        public string SMSReceived(string sender, string body, DateTime receiveDate)
        {
            //var db = MyServiceProvider.ResolveService<DataContext>();
            //var smsService = MyServiceProvider.ResolveService<ISMSService>();
            //var orderService = MyServiceProvider.ResolveService<OrderService>();
            var order = db.Orders.FirstOrDefault(x => x.TrackNo.ToLower().Equals(body.ToLower()));

            string error;
            if (order == null)
            {
                error = "سفارشی با این کد در سیستم ثبت نشده است.";
                //string userMessage = string.Format(InvalidDeliveryCodeReceived, error);

                throw new BadRequestException(SMSManager.TEMP_MARK + error);
                //smsService.Send(sender, string.Format(InvalidDeliveryCodeReceived, error));
                //return;
            }

			//try
			//{
			orderService.SetOrderDelivered(order.Id).Wait();
			return order.TrackNo;
				//smsService.Send(sender, string.Format(OrderDeliveryCodeReceived, order.TrackNo));
			//}
            //catch(BadRequestException ex)
            //{
            //    smsService.Send(sender, string.Format(InvalidDeliveryCodeReceived, ex.Message));
            //    throw;
            //}
        }
    }
}
