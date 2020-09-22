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
        [StringLength(30)]
        [Display(Name = "شماره حساب")]
        public string AccountNumber { get; set; }
        [StringLength(50)]
        public string AccountBankName { get; set; }

        public User User { get; set; }
        public long? UserId { get; set; }

    }
}
