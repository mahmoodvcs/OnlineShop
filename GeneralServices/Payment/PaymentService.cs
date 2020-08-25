using MahtaKala.Entities;
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

        public PaymentService(DataContext dataContext, IBankPaymentService bankService)
        {
            this.dataContext = dataContext;
            this.bankService = bankService;
        }
        public async Task<(string internalPayUrl, Entities.Payment payment)> InitPayment(Order order)
        {
            var payment = new Entities.Payment()
            {
                Amount = order.TotalPrice,
                Order = order,
                State = PaymentState.Registerd
            };
            dataContext.Payments.Add(payment);
            await dataContext.SaveChangesAsync();
            payment.PayToken = await bankService.GetToken(payment);
            await dataContext.SaveChangesAsync();
            string payUrl = "";//TODO: PaymentController
            return (payUrl, payment);
        }
    }
}
