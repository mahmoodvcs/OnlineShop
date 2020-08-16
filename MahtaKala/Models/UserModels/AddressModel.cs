using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.UserModels
{
    public class AddressModel : ILocationModel
    {
        public long Id { get; set; }
        public long City { get; set; }
        public long Province { get; set; }
        public string Details { get; set; }
        public string Postal_Code { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
