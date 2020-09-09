using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.GeneralServices.Payment;
using MahtaKala.Infrustructure;
using MahtaKala.Models.Payment;
using MahtaKala.Services;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace MahtaKala.Controllers
{
    public class PaymentController : MahtaControllerBase<PaymentController>
    {
        private readonly IBankPaymentService bankPaymentService;
        private readonly IPathService pathService;
        private readonly OrderService orderService;

        private ISMSService SMSService { get; set; }

        public PaymentController(
            DataContext dataContext,
            ILogger<PaymentController> logger,
            IBankPaymentService bankPaymentService,
            IPathService pathService,
            OrderService orderService,
            ISMSService smsService) : base(dataContext, logger)
        {
            this.bankPaymentService = bankPaymentService;
            this.pathService = pathService;
            this.orderService = orderService;
            this.SMSService = smsService;
        }

        public async Task<ActionResult> Pay(long pid, string uid)
        {
            var payment = await db.Payments.FindAsync(pid);
            if (payment.UniqueId != uid)//TODO:Double check
            {

            }

            if (!payment.IsPayable)
            {
                //TODO: Debug log
                throw new Exception(ServiceMessages.Payment.PaymentIsNotPayable);
            }

            payment.State = PaymentState.SentToBank;
            await db.SaveChangesAsync();
            return View(new PayModel
            {
                Token = payment.PayToken,
                BankPostUrl = bankPaymentService.GetPayUrl(payment)
            });
        }

        //public async Task<ActionResult> Test()
        //{
        //    var myAddress = db.Addresses.FirstOrDefault(a => a.UserId == UserId);
        //    var prod = await db.Products.FirstAsync();
        //    Order order = new Order
        //    {
        //        Address = myAddress,
        //        OrrderDate = DateTime.Now,
        //        UserId = UserId,
        //        Items = new List<OrderItem>
        //        {
        //            new OrderItem
        //            {
        //                ProductPriceId = prod.Id,
        //                Quantity = 1,
        //                UnitPrice = 1000
        //            }
        //        },
        //        TotalPrice = 1000
        //    };
        //    db.Add(order);
        //    await db.SaveChangesAsync();
        //    var pay = await paymentService.InitPayment(order, pathService.AppBaseUrl + "/Payment/Paid?source=api");
        //    return Redirect(pathService.AppBaseUrl + $"/Payment/Pay?pid={pay.Id}&uid={pay.UniqueId}&source=api");
        //}

        [HttpPost]
        public async Task<ActionResult> Paid()//didn't work [FromQuery]string source)
        {
            var payment = await DoPaymentCallbackOperations();
            return View("PaidFromApi", payment);
        }

        [HttpPost]
        public async Task<ActionResult> CallBackPay()
        {
            var payment = await DoPaymentCallbackOperations();
            return View(payment);
        }

        private async Task<Payment> DoPaymentCallbackOperations()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                Payment payment = await bankPaymentService.Paid(body);
                await orderService.Paid(payment);
                if (payment.State == PaymentState.Succeeded)
                    SendPaymentSMS(payment);
                return payment;
            }
        }

        private void SendPaymentSMS(Payment payment)
        {
            if (payment.State == PaymentState.Succeeded && payment.Order.State == OrderState.Paid)
            {
                string message = string.Format(Messages.Messages.Order.OrderPaymentSuccessMessage,
                    payment.TrackingNumber,
                    payment.Order.ApproximateDeliveryDate?.ToString("yyyy/MM/dd HH:mm"));
                var number = db.Users.Where(a => a.Id == payment.Order.UserId).Select(a => a.MobileNumber).Single();
                SMSService.Send(number, message);
            }
        }

    }
}
