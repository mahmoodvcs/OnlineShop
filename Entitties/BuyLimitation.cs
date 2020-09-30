using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class BuyLimitation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(500)]
        [Display(Name = "عنوان")]
        public string Name { get; set; }
        [Display(Name = "حداقل تعداد خرید")]
        public int? MinBuyQuota { get; set; }
        [Display(Name = "حداکثر تعداد خرید")]
        public int? MaxBuyQuota { get; set; }
        [Display(Name = "بازه ی زمانی اعمال محدودیت  خرید")]
        public int? BuyQuotaDays { get; set; }
        [Display(Name = "فروش فقط در شهر")]
        public long? CityId { get; set; }
        public City City { get; set; }
        [Display(Name = "فروش فقط در استان")]
        public long? ProvinceId { get; set; }
        public Province Province { get; set; }

    }
}
