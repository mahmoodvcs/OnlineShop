using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class Seller
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [StringLength(255)]
        [Display(Name = "نام")]
        public string Name { get; set; }
        [StringLength(50)]
        [Display(Name = "شماره شبا")]
        public string AccountNumber { get; set; }
        [Display(Name = "آدرس")]
        public string Address { get; set; }
        //[StringLength(50)]
        //public string AccountBankName { get; set; }
        [Display(Name = "عرض جغرافیایی")]
        public double? Lat { get; set; }
        [Display(Name = "طول جغرافیایی")]
        public double? Lng { get; set; }

        [StringLength(20)]
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        [Display(Name = "کاربر")]
        public User User { get; set; }
        [Display(Name = "کاربر")]
        public long? UserId { get; set; }

        [Display(Name = "نام سبد")]
        [StringLength(100)]
        public string Basket { get; set; }

        public IList<Product> Products { get; set; }

    }
}
