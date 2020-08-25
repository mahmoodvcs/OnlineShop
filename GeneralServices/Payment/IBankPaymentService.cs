using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices.Payment
{
    public interface IBankPaymentService
    {
        Task<string> GetToken(Entities.Payment payment);
        string GetPayUrl(Entities.Payment payment);
    }
}
