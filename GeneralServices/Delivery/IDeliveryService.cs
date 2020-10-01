using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices
{
    public interface IDeliveryService
    {
        Task InitDelivery(Seller seller, long[] orderItemIds);
    }
}
