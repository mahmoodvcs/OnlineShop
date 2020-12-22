using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.GeneralServices.Payment;
using MahtaKala.GeneralServices.Payment.PardakhtNovin;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure;
using MahtaKala.Infrustructure.Exceptions;
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
using System.Text;
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
            if (bankPaymentService is TestBasnkService)
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

        public async Task<string> report()
        {
            return await ((PardakhtNovinService)bankPaymentService).GetReport();
        }

        [HttpPost]
        public async Task<ActionResult> Paid()//didn't work [FromQuery]string source)
        {
            var payment = await DoPaymentCallbackOperations();
            var paymentModel = new BackFromPaymentVM(payment);
            // This is just a precaution, because normally, the Payment object should contain the source from which 
            // the payment process is originated, which makes the query parameter "source" redundant!
            //if (!string.IsNullOrWhiteSpace(source))
            //{
            //    source = source.Trim().ToLower();
            //    if (source.Equals("website"))
            //        paymentModel.PaymentSourceUsed = SourceUsedForPayment.WebSite;
            //    else if (source.Equals("mobileapp"))
            //        paymentModel.PaymentSourceUsed = SourceUsedForPayment.MobileApp;
            //}
            paymentModel.BaseUrl = pathService.AppBaseUrl;
            return View("PaidFromApi", paymentModel);
        }

        [HttpPost]
        public async Task<ActionResult> CallBackPay()
        {
            var payment = await DoPaymentCallbackOperations();
            return View(payment);
        }

   //     [HttpGet]
   //     public async Task<IActionResult> TempPaymentTestmentForMentKishdeh()
   //     {
   //         if (!User.NationalCode.Contains("0079645488"))
   //             return Ok("Ah ah! No way, dude! Don't be a wise-ass!");
   //         var product = await db.Products.Include(x => x.Prices).Where(x => x.Status == ProductStatus.Available 
   //                                                     && x.Quantities.Count > 0 && x.Quantities.First().Quantity > 0)
   //             //.OrderBy(x => x.Prices.First().Price)
   //             .FirstOrDefaultAsync();
   //         if (product == null)
   //             return Ok("There is no product with a quantity greater than 0! What's goin' on here?!");
   //         await orderService.EmptyCart();
   //         await orderService.AddToCart(product.Prices.First().Id);
   //         if (!db.Addresses.Any(x => x.UserId == User.Id && x.Disabled == false && !string.IsNullOrEmpty(x.Details)&& x.Details.Length > 1))
			//{
   //             var tehran = db.Cities.Where(x => x.Name.Contains("تهران")).FirstOrDefault();
   //             if (tehran == null)
   //                 tehran = db.Cities.Where(x => x.Name.Contains("تهرا") || x.Name.Contains("هران")).FirstOrDefault();
   //             if (tehran == null)
   //                 tehran = db.Cities.Where(x => !string.IsNullOrEmpty(x.Name) && x.ProvinceId > 1).FirstOrDefault();
   //             if (tehran == null)
   //                 logger.LogError("Go lug yourself!!");
   //             var newAddress = new UserAddress()
   //             {
   //                 Title = "Home Sweet/Crappy Home!",
   //                 Details = "توی خونه ش یه باغه! غذاش روی اجاقه! وااااای وای",
   //                 PostalCode = "1234567890",
   //                 CityId = tehran.Id,
   //                 Disabled = false,
   //                 UserId = User.Id
   //             };
   //             db.Addresses.Add(newAddress);
   //             await db.SaveChangesAsync();
			//}
   //         var addressId = db.Addresses
   //             .Where(x => x.UserId == User.Id 
   //                 && x.Disabled == false
   //                 && !string.IsNullOrEmpty(x.Details) 
   //                 && x.Details.Length > 1)
   //             .First().Id;
   //         var order = await orderService.Checkout(addressId);
   //         var returnUrl = pathService.AppBaseUrl + "/Payment/TempPaymentCallBack";//?source=api";
   //         var payment = await orderService.InitPayment(order, returnUrl, SourceUsedForPayment.MobileApp);
   //         string payUrl = pathService.AppBaseUrl + $"/Payment/Pay?pid={payment.Id}&uid={payment.UniqueId}";
   //         return Redirect(payUrl);
   //     }

        //[HttpPost]
        //public async Task<ActionResult> TempPaymentCallBack()
        //{
        //    using (var reader = new StreamReader(Request.Body))
        //    {
        //        var body = await reader.ReadToEndAsync();
        //        logger.LogError("doomsdaydevice - The request body is: " + body);
        //        Payment payment = await bankPaymentService.Paid(body);
        //        if (payment == null || payment.Id == 0)
        //            return Json(new
        //            {
        //                message = "The Payment object returned from the Paid method of the bankService seems to have a problem! Look at this for fox sake! :P",
        //                payment = payment
        //            });
        //        await orderService.Paid(payment);
        //        var model = new BackFromPaymentVM(payment);
        //        return View("PaidFromApi", model);
        //    }

        //}

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
                var order = db.Orders.FirstOrDefault(x => x.Id == payment.OrderId);
                if (order == null)
                {
                    throw new Exception($"function: PaymentController.SendPaymentSMS(Payment payment) - Order with id {payment.OrderId} was not found!");
                }
                var deliveryTrackingNumber = GenerateTrackingNumber(payment);
                order.TrackNo = deliveryTrackingNumber;
                db.SaveChanges();
                string message = string.Format(Messages.Messages.Order.OrderPaymentSuccessMessage,
                    deliveryTrackingNumber /*same as order.TrackNo*/,
                    Util.GetPersianDate(payment.Order.ApproximateDeliveryDate, true, true));
                var number = db.Users.Where(a => a.Id == payment.Order.UserId).Select(a => a.MobileNumber).Single();
                SMSService.Send(number, message);
            }
        }

        private string GenerateTrackingNumber(Payment payment)
        {
            if (payment.OrderId <= 0)
                throw new ApiException(400, $"Payment.OrderId has the value {payment.OrderId}, which is not valid!");
            var charSet = new char[] { '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 
                                       'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 
                                       'N', 'P', 'R', 'S', 'T', 'W', 'Y', 'Z' };
            var inNewBase = Util.ChangeNumberBaseFrom10(payment.OrderId, charSet);
            StringBuilder resultBuilder = new StringBuilder(inNewBase);
            if (resultBuilder.Length < 7)
            {
                resultBuilder.Append('X');
                var rand = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
                while (resultBuilder.Length < 7)
                {
                    resultBuilder.Append(charSet[rand.Next(0, charSet.Length)]);
                }
            }
            // Shuffling the tracking number will make it just a bit difficult to decipher the algorithm on the first glance!
            // p.s. We're not gonna do anything about the second glance! Sowwy!
            string rawResult = resultBuilder.ToString();
            resultBuilder.Clear();
            resultBuilder.Append(rawResult[1]);
            resultBuilder.Append(rawResult[5]);
            resultBuilder.Append(rawResult[0]);
            resultBuilder.Append(rawResult[2]);
            resultBuilder.Append(rawResult[6]);
            resultBuilder.Append(rawResult[4]);
            resultBuilder.Append(rawResult[3]);
            return resultBuilder.ToString();
        }

        private long PaymentIdFromTrackingNumber(string trackingNumber)
        {
            var charSet = new char[] { '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B',
                                       'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M',
                                       'N', 'P', 'R', 'S', 'T', 'W', 'Y', 'Z' };
            string unshuffled = "" + trackingNumber[2] 
                + trackingNumber[0] + trackingNumber[3] 
                + trackingNumber[6] + trackingNumber[5]
                + trackingNumber[1] + trackingNumber[4];
            if (unshuffled.Contains('X'))
                unshuffled = unshuffled.Substring(0, unshuffled.IndexOf('X'));
            var inBaseTen = Util.ChangeNumberBaseTo10(unshuffled, charSet);
            return inBaseTen;
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
