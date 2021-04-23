using MahtaKala.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices.Delivery
{
    public class YarBoxDeliveryService : IDeliveryService
    {
        private readonly DataContext db;
        private readonly SettingsService settingsService;
        private readonly ILogger<YarBoxDeliveryService> logger;

        public YarBoxDeliveryService(
            DataContext dataContext,
            SettingsService settingsService,
            ILogger<YarBoxDeliveryService> logger
            )
        {
            this.db = dataContext;
            this.settingsService = settingsService;
            this.logger = logger;
        }

        const string APIAddress = "https://api.yarbox.co/api/v3/";
        const string APIKey = @"eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiMDkxMjgzNTMyMzAiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3NlcmlhbG51bWJlciI6IjcwYmEwMDE5MzdlNDRhNWRiNGRmN2JlNDhlM2MxOTM1IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy91c2VyZGF0YSI6IjMzMjI0OTA5LThmMDAtZWIxMS05MGIyLTBjYzQ3YTMwZTY1NyIsIm5iZiI6MTYwMTE5MDMzNiwiZXhwIjoxNjAzNzgyMzM2LCJpc3MiOiJodHRwOi8vbG9jYWxob3N0LyIsImF1ZCI6IkFueSJ9.LKhj6f9QglVfc-LFOJq0NcbCXmMRf9QqPvKSS7K_2Fw";
        const long DELIVERY_COMPANY_PAYMENT_PARTY_ID = 7;
        const string DELIVERY_COMPANY_NAME = "یارباکس";

        public async Task InitDelivery(long orderId)
        {
            var items = await db.OrderItems.Where(a => a.OrderId == orderId)
                .Select(a => new
                {
                    a.Order.AddressId,
                    Address = a.Order.Address.Details,
                    City = a.Order.Address.City.Name,
                    Province = a.Order.Address.City.Province.Name,
                    a.Order.UserId,
                    a.Order.User.FirstName,
                    a.Order.User.LastName,
                    a.Order.User.MobileNumber,
                    a.Quantity,
                    a.Id
                }).ToListAsync();

            DeliveryRequest req = new DeliveryRequest()
            {
                origin = new DeliveryOrigin
                {
                    latitude = "0",
                    longitude = "0",
                    senderPhoneNumber = "0",
                    street = "0"
                },
                count = items.Sum(a => a.Quantity),
                content = "",
                destination = new DeliveryDestination
                {
                    city = items.First().City,
                    province = items.First().Province,
                    receiverName = items.First().FirstName + " " + items.First().LastName,
                    receiverPhoneNumber = items.First().MobileNumber,
                    street = items.First().Address
                }
            };
            DeliveryResponse response;
            try
            {
                response = await SendRequest(req);
            }
            catch (Exception ex)
            {
                throw new Exception("خطا در ثبت بار.", ex);
            }
            if (response == null)
            {
                throw new Exception("خطا در ثبت بار.", 
                    new Exception("DeliveryResponse object from YarBox service came back null!"));
            }
            if (string.IsNullOrEmpty(response.packkey))
            {
                throw new Exception("خطا در ثبت بار: " + response.message);
            }

            var itemIds = items.Select(a => a.Id);
            Entities.Delivery delivery = new Entities.Delivery
            {
                Request = JsonConvert.SerializeObject(req),
                OrderItemIds = JsonConvert.SerializeObject(itemIds),
                PackKey = response.packkey,
                TrackNo = response.id,
                UserId = items.First().UserId,
            };
            db.Deliveries.Add(delivery);

            var orderItems = db.OrderItems.Where(o => itemIds.Any(x => x == o.Id)).ToList();
            foreach (var item in orderItems)
            {
                item.Delivery = delivery;
            }
            await db.SaveChangesAsync();
            return;
        }

        private async Task<DeliveryResponse> SendRequest(DeliveryRequest req)
        {
            var ret = await Post("company/MultiplePacks", JsonConvert.SerializeObject(req));
            logger.LogWarning("Delivery response: " + ret);
            var response = JsonConvert.DeserializeObject<DeliveryResponse>(ret);
            return response;
        }

        private async Task<string> Post(string service, string body)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "bearer " + APIKey);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(APIAddress + service, content);
            var resStr = await response.Content.ReadAsStringAsync();
            return resStr;
        }

        public string GetShabaId()
        {
            var deliveryCompanyPaymentPartyEntity = db.PaymentParties.Where(x => x.Id == DELIVERY_COMPANY_PAYMENT_PARTY_ID).FirstOrDefault();
            if (deliveryCompanyPaymentPartyEntity == null)
			{
                deliveryCompanyPaymentPartyEntity = db.PaymentParties.Where(x => x.Name.Trim().Equals(DELIVERY_COMPANY_NAME.Trim())).SingleOrDefault();
                if (deliveryCompanyPaymentPartyEntity == null)
				{
                    logger.LogError($"None of the parties in payment_parties table of the database corresponds to the information regarding the delivery company, as recorded here," +
                        $" niether to the Id of {DELIVERY_COMPANY_PAYMENT_PARTY_ID}, nor the name of '{DELIVERY_COMPANY_NAME}'! " +
                        $" Something has changed in the database, but NOT in here, which, makes it awkward! Call the admin!");
                    throw new Exception("خطا در دریافت شماره شبای شرکت حمل و نقل (دلیوری کالا)! لطفن با ادمین سیستم تماس بگیرید.");
                }
			}
			else
			{
                if (!deliveryCompanyPaymentPartyEntity.Name.Trim().ToLower().Equals(DELIVERY_COMPANY_NAME.Trim().ToLower()))
				{
                    logger.LogError($"There's some discrepancy between the information recorded in the database, and the info recorded here in the code," +
                        $" as the party who delivers the parcels, and, to whom the delivery fee should be paid!" +
                        $" In here, it's indicated that, in table payment_parties, the record having an Id of {DELIVERY_COMPANY_PAYMENT_PARTY_ID}," +
                        $" and bearing the name '{DELIVERY_COMPANY_NAME}', is the one to whom the delivery fee should go!" +
                        $" But, right now, in the db, the Id {DELIVERY_COMPANY_PAYMENT_PARTY_ID} corresponds with the name '{deliveryCompanyPaymentPartyEntity.Name}'" +
                        $" which does not match with what is expected here! Call someone right now!");
                    throw new Exception("خطا در دریافت شماره شبای شرکت حمل و نقل (دلیوری کالا)! لطفن با ادمین سیستم تماس بگیرید.");
				}
			}
            return deliveryCompanyPaymentPartyEntity.ShabaId;
            //return "IR360640012399674119600002";
            // The above "returh" statement, which is directly returning a hard-coded shaba_id, has the old one! Now it's changed to "IR870180000000000376114376"!
            // As this is not good practice - because, in the least, you have to update the software itself each time you need to change the shaba_id to which the 
            // delivery fee is paid - we now define and id, and a name, from which we can extract the shaba_id belonging to the delivery company.
            // Maybe, it would be to check only for the Id, and get the name of the company from the database, too! Maybe!
        }

        public string GetName()
        {
            return DELIVERY_COMPANY_NAME;
        }
    }


    class DeliveryRequest
    {
        public DeliveryDestination destination { get; set; }
        public DeliveryOrigin origin { get; set; }
        public string receiveType => "doorTodoor";
        public bool isPacking => false;
        public int insurancePrice => 0;
        public string content { get; set; }
        public int postPackWeight => count * 5;
        public int count { get; set; }
    }

    class DeliveryDestination
    {
        public string receiverPhoneNumber { get; set; }
        public string receiverName { get; set; }
        public int portId => 0;
        public string province { get; set; }
        public string city { get; set; }
        public string street { get; set; }

    }

    class DeliveryOrigin
    {
        public string senderPhoneNumber { get; set; }
        public string province => "تهران";
        public string city => "تهران";
        public string street { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }

    class DeliveryResponse
    {
        public string packkey { get; set; }
        public string message { get; set; }
        public string id { get; set; }
    }
}
