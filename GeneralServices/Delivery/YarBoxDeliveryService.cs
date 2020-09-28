using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.GeneralServices.Delivery
{
    public class YarBoxDeliveryService : IDeliveryService
    {
        const string APIAddress = "https://api.yarbox.co/api/v3/";
    }


    class CargoRequest
    {
        public CargoDestination destination { get; set; }
        public CargoOrigin origin { get; set; }
        public string receiveType { get; set; }
        public bool isPacking { get; set; }
        public int insurancePrice { get; set; }
        public string content { get; set; }
        public int postPackWeight { get; set; }
        public int count { get; set; }
    }

    class CargoDestination
    {
        public string receiverPhoneNumber { get; set; }
        public string receiverName { get; set; }
        public int portId { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string street { get; set; }

    }

    class CargoOrigin
    {
        public string senderPhoneNumber { get; set; }
        public string province => "تهران";
        public string city => "تهران";
        public string street { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }
}
