using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models
{
    public class CheckOutVM
    {
        public UserDataVM UserData { get; set; }
        public UserAddress UserAddress { get; set; }
        public int CartItemCount { get; set; }
        public string Cost { get; set; }
        public string PostCost { get; set; }
        public string FinalCost { get; set; }
        public bool IsNewAddress { get; set; }
    }
    public class UserDataVM
    {
        [StringLength(255)]
        public string FirstName { get; set; }
        [StringLength(255)]
        public string LastName { get; set; }
        [StringLength(10)]
        public string NationalCode { get; set; }
        [StringLength(255)]
        public string EmailAddress { get; set; }
        public long? AddressId { get; set; }
    }
}
