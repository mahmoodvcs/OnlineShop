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

namespace MahtaKala.Controllers
{
    public class PaymentController : MahtaControllerBase<PaymentController>
    {
        private readonly PaymentService paymentService;
        private readonly IBankPaymentService bankPaymentService;
        private readonly IPathService pathService;

        public PaymentController(
            DataContext dataContext,
            ILogger<PaymentController> logger,
            PaymentService paymentService,
            IBankPaymentService bankPaymentService,
            IPathService pathService) : base(dataContext, logger)
        {
            this.paymentService = paymentService;
            this.bankPaymentService = bankPaymentService;
            this.pathService = pathService;
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
                //if (source == "api")
                    return View("PaidFromApi", payment);
                //else
                //    return View(payment);
            }
        }

    }
}
