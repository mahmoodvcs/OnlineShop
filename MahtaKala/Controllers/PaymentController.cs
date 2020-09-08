using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.GeneralServices.Payment;
using MahtaKala.Infrustructure;
using MahtaKala.Models.Payment;
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
        private ISMSService SMSService { get; set; }

        public PaymentController(
            DataContext dataContext,
            ILogger<PaymentController> logger,
            IBankPaymentService bankPaymentService,
            IPathService pathService,
            ISMSService smsService) : base(dataContext, logger)
        {
            this.bankPaymentService = bankPaymentService;
            this.pathService = pathService;
            this.SMSService = smsService;
        }

        public async Task<ActionResult> Pay(long pid, string uid)
        {
            var payment = await db.Payments.FindAsync(pid);
            if (payment.UniqueId != uid)//TOSO:Double check
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
            using (var reader = new StreamReader(Request.Body))
            {
                //reader.BaseStream.Seek(0, SeekOrigin.Begin);
                var body = await reader.ReadToEndAsync();

                Payment payment = await bankPaymentService.Paid(body);
                SendPaymentSMS(payment);
                //if (source == "api")
                return View("PaidFromApi", payment);
                //else
                //    return View(payment);
            }
        }

        [HttpPost]
        public async Task<ActionResult> CallBackPay()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                Payment payment = await bankPaymentService.Paid(body);
                if (payment.State == PaymentState.Succeeded)
                {
                    await db.ShoppingCarts.Where(a => a.UserId == payment.Order.UserId).DeleteAsync();
                    SendPaymentSMS(payment);
                }
                return View(payment);
            }
        }

        private void SendPaymentSMS(Payment payment)
        {
            if (payment.State == PaymentState.Succeeded && payment.Order.State == OrderState.Paid)
            {
                string message = "مهتاکالا: پرداخت با موفقیت انجام شد. کد ره گیری:" +
                    payment.TrackingNumber +
                    " تاریخ تخمینی ارسال: " +
                    payment.Order.SentDateTime.ToString();
                SMSService.Send(User.MobileNumber, Messages.Messages.Signup.LoginOTPMessage);
            }
        }

    }
}
