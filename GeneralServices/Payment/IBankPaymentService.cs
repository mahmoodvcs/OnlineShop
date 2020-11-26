using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices.Payment
{
    public interface IBankPaymentService
    {
        string MoveAlong(string terminal, string buyId, long amount, string date, string time, string returnUrl, string securityKey);
        Task<string> GetToken(Entities.Payment payment, string returnUrl);
        string GetPayUrl(Entities.Payment payment);
        Task<Entities.Payment> Paid(string bankReturnBody);

        Task SharePayment(Entities.Payment payment, List<PaymentShareDataItem> items);
    }

    public class PaymentShareDataItem
    {
        public string ShabaId { get; set; }
        public decimal Amount { get; set; }
        public string Name { get; set; }
        public long ItemId { get; set; }
        public PayFor PayFor { get; set; }  
    }

}
