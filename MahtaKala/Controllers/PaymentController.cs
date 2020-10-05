using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.GeneralServices.Payment;
using MahtaKala.GeneralServices.Payment.PardakhtNovin;
using MahtaKala.Helpers;
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
            if (Request.Host.Host == "localhost" && bankPaymentService is TestBasnkService)
            {
                return View("TestPay", payment);
            }
            return View(new PayModel
            {
                Token = payment.PayToken,
                BankPostUrl = bankPaymentService.GetPayUrl(payment)
            });
        }

        //public async Task<ActionResult> TestPay(int amount)
        //{
        //    var myAddress = db.Addresses.FirstOrDefault(a => a.UserId == UserId);
        //    var prod = await db.Products.Include(a=>a.Prices).FirstAsync();
        //    Order order = new Order
        //    {
        //        Address = myAddress,
        //        UserId = UserId,
        //        Items = new List<OrderItem>
        //        {
        //            new OrderItem
        //            {
        //                ProductPriceId = prod.Prices[0].Id,
        //                Quantity = 1,
        //                UnitPrice = amount
        //            }
        //        },
        //        TotalPrice = amount
        //    };
        //    db.Add(order);
        //    await db.SaveChangesAsync();
        //    var pay = await orderService.InitPayment(order, pathService.AppBaseUrl + "/Payment/CallBackPay");
        //    return Redirect(pathService.AppBaseUrl + $"/Payment/Pay?pid={pay.Id}&uid={pay.UniqueId}&source=api");
        //}

        //[HttpPost]
        //public async Task<string> TestShare([FromQuery]string trackNo, [FromBody]List<ScatteredSettlementDetails> shares)
        //{
        //    var payment = await db.Payments.FirstOrDefaultAsync(a=> a.TrackingNumber == trackNo);
        //    var res = await ((PardakhtNovinService)bankPaymentService).SharePayment(payment, shares);
        //    return res;
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
                    Util.GetPersianDate(payment.Order.ApproximateDeliveryDate));
                var number = db.Users.Where(a => a.Id == payment.Order.UserId).Select(a => a.MobileNumber).Single();
                SMSService.Send(number, message);
            }
        }

        //public async Task<string> ShareTest(long id)
        //{
        //    var ser = (PardakhtNovinService)bankPaymentService;
        //    var pay = db.Payments.Find(id);
        //    var res = await ser.Share(pay, new List<ProductPaymentParty>
        //    {
        //        new ProductPaymentParty
        //        {
        //            PaymentParty=new PaymentParty
        //            {
        //                ShabaId="IR470550330280003044776001"
        //            },
        //            Percent=5
        //        },
        //        new ProductPaymentParty
        //        {
        //            PaymentParty=new PaymentParty
        //            {
        //                ShabaId="IR040700058800114181383001"
        //            },
        //            Percent=20
        //        }

        //    });

        //    return res;
        //}

    }
}
