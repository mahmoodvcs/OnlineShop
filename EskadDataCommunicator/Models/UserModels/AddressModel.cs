using MahtaKala.Entities;
using MahtaKala.Infrustructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.UserModels
{
    public class AddressModel : ILocationModel
    {
        private string _postalCode;
        public long Id { get; set; }
        public long City { get; set; }
        public long Province { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string Postal_Code 
        {
            set
            {
                _postalCode = value.ToEnglishNumber();
            }
            get
            {
                return _postalCode;
            }
        }
        public double Lat { get; set; }
        public double Lng { get; set; }

        public AddressModel() { }
        public AddressModel(UserAddress userAddress)
        {
            Id = userAddress.Id;
            Title = userAddress.Title;
            City = userAddress.CityId;
            Province = userAddress.City == null ? 0 : userAddress.City.ProvinceId;
            Details = userAddress.Details;
            Postal_Code = userAddress.PostalCode;
            Lat = userAddress.Lat;
            Lng = userAddress.Lng;
        }
    }
}
