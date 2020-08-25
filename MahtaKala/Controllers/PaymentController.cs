using MahtaKala.GeneralServices.Payment;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Controllers
{
    public class PaymentController :Controller
    {
        public async Task<ActionResult> Pay()
        {
            ICKPaymentService paymentService = new ICKPaymentService();
            var token = await paymentService.GetToken(1000);
            return View();
        }
    }
}
