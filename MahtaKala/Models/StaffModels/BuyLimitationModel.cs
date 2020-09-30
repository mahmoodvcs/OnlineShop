using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.StaffModels
{
    public class BuyLimitationModel : BuyLimitation
    {
        public BuyLimitationModel() { }
        public BuyLimitationModel(BuyLimitation limitation)
        {
            Id = limitation.Id;
            Name = limitation.Name;
            MinBuyQuota = limitation.MinBuyQuota;
            MaxBuyQuota = limitation.MaxBuyQuota;
            BuyQuotaDays = limitation.BuyQuotaDays;
            CityId = limitation.CityId;
            ProvinceId = limitation.ProvinceId;
        }

        public string City { get; set; }
        public string Province { get; set; }
        public string LimitDays { get; set; }
    }
}
