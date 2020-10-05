using MahtaKala.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices.Payment
{
    public class TestBasnkService : IBankPaymentService
    {
        private DataContext db;

        public TestBasnkService(DataContext dataContext)
        {
            this.db = dataContext;
        }

        public string GetPayUrl(Entities.Payment payment)
        {
            return "";   
        }

        public async Task<string> GetToken(Entities.Payment payment, string returnUrl)
        {
            return "";
        }

        public async Task<Entities.Payment> Paid(string bankReturnBody)
        {
            var dic = bankReturnBody.Split('&').ToDictionary(a => a.Split('=')[0].ToLower(), a => a.Split('=')[1]);
            long pid = long.Parse(dic["id"]);
            var payment = db.Payments.Include(a => a.Order).FirstOrDefault(a => a.Id == pid);
            payment.State = dic["status"] == "ok" ? PaymentState.Succeeded : PaymentState.Failed;
            if (payment.State == PaymentState.Succeeded)
                payment.Order.State = OrderState.Paid;
            await db.SaveChangesAsync();
            return payment;
        }

        public Task SharePayment(Entities.Payment payment, List<PaymentShareDataItem> items)
        {
            throw new NotImplementedException();
        }
    }
}
