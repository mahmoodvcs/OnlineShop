using MahtaKala.Entities;
using MahtaKala.SharedServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices.Payment
{
    public class PaymentService
    {
        private readonly DataContext dataContext;
        private readonly IBankPaymentService bankService;
        private readonly IPathService pathService;

        public PaymentService(DataContext dataContext, IBankPaymentService bankService, IPathService pathService)
        {
            this.dataContext = dataContext;
            this.bankService = bankService;
            this.pathService = pathService;
        }
        public async Task<Entities.Payment> InitPayment(Order order, string returnUrl)
        {
            var payment = new Entities.Payment()
            {
                Amount = order.TotalPrice,
                Order = order,
                State = PaymentState.Registerd
            };
            dataContext.Payments.Add(payment);
            await dataContext.SaveChangesAsync();
            payment.PayToken = await bankService.GetToken(payment, returnUrl);
            await dataContext.SaveChangesAsync();
            return payment;
        }

        //public async Task<Entities.Payment> Paid(string bankReturnBody)
        //{

        //}
    }
}
