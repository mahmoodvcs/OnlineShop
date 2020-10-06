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

        public async Task InitDelivery(long orderId)
        {
            var items = await db.OrderItems.Where(a => a.OrderId == orderId)
                .Select(a => new
                {
                    a.ProductPrice.Product.Seller,
                    a.Id
                })
                .GroupBy(a => a.Seller)
                .Select(a => new
                {
                    a.Key,
                    Ids = a.Select(z => z.Id).ToList()
                })
                .ToListAsync();

            foreach (var item in items)
            {
                await InitDelivery(item.Key, item.Ids.ToArray());
            }
        }

        public async Task InitDelivery(Seller seller, long[] orderItemIds)
        {
            var items = await db.OrderItems.Where(a => orderItemIds.Contains(a.Id))
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
            //var query = from item in db.OrderItems.Where(a => orderItemIds.Contains(a.Id))
            //            join order in db.Orders on item.OrderId equals order.Id
            //            join user in db.Users on order.UserId equals user.Id
            //            join address in db.Addresses on order.AddressId equals address.Id
            //            group item by new
            //            {
            //                Address = address.Details,
            //                user.MobileNumber,
            //                City = address.City.Name,
            //                Province = address.City.Province.Name,
            //                user.FirstName,
            //                user.LastName,
            //                UserId = user.Id,

            //            } into groups
            //            select new
            //            {
            //                Info = groups.Key,
            //                Count = groups.Sum(a => a.Quantity),
            //                //Ids = groups.Select(a => a.Id).ToList()
            //            };
            //var items = await query.ToListAsync();
            Dictionary<long, Entities.Delivery> itemDeliveries = new Dictionary<long, Entities.Delivery>();
            foreach (var group in items.GroupBy(a => new
            {
                a.UserId,
                a.AddressId
            }))
            {
                var info = group.First();
                DeliveryRequest req = new DeliveryRequest()
                {
                    origin = new DeliveryOrigin
                    {
                        latitude = seller.Lat?.ToString(),
                        longitude = seller.Lng?.ToString(),
                        senderPhoneNumber = seller.PhoneNumber,
                        street = seller.Address
                    },
                    count = group.Sum(a => a.Quantity),
                    content = "",
                    destination = new DeliveryDestination
                    {
                        city = info.City,
                        province = info.Province,
                        receiverName = info.FirstName + " " + info.LastName,
                        receiverPhoneNumber = info.MobileNumber,
                        street = info.Address
                    }
                };
                var response = await SendRequest(req);
                if (string.IsNullOrEmpty(response.packkey))
                {
                    throw new Exception("خطا در ثبت بار: " + response.message);
                }

                var itemIds = group.Select(a => a.Id);
                Entities.Delivery delivery = new Entities.Delivery
                {
                    Request = JsonConvert.SerializeObject(req),
                    OrderItemIds = JsonConvert.SerializeObject(itemIds),
                    SellerId = seller.Id,
                    PackKey = response.packkey,
                    TrackNo = response.id,
                    UserId = info.UserId,
                };
                db.Deliveries.Add(delivery);
                foreach (var id in itemIds)
                {
                    itemDeliveries.Add(id, delivery);
                }
            }

            var ids = itemDeliveries.Keys.ToList();
            var orderItems = db.OrderItems.Where(o => ids.Contains(o.Id)).ToList();
            foreach (var item in orderItems)
            {
                item.Delivery = itemDeliveries[item.Id];
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
            return "IR360640012399674119600002";
        }

        public string GetName()
        {
            return "یارباکس";
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
