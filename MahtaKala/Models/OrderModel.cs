using MahtaKala.Entities;
using MahtaKala.Helpers;
using MahtaKala.Models.CustomerModels;
using MahtaKala.Models.UserModels;
using MahtaKala.Services;
using MahtaKala.SharedServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models
{
    public class OrderModel
    {
        public long Id { get; set; }
        public string CheckoutDate { get; set; }
        public string ApproximateDeliveryDate { get; set; }
        public string SendDate { get; set; }
        public string ActualDeliveryDate { get; set; }
        public decimal Price { get; set; }
		public decimal DeliveryPrice { get; set; }
		public string FirstName { get; set; }
        public string LastName { get; set; }
        public string State { get; set; }
        public string DeliveryTrackNo { get; set; }
        public long? Address_Id { get; set; }
        public AddressModel Address { get; set; }
        public IList<OrderItemModel> OrderItems { get; set; }

		public static async Task<List<OrderModel>> Get(IQueryable<Order> ordersQuery, IProductImageService productImageService)
        {
            var data = await ordersQuery.Select(a => new
            {
                a.Id,
                Price = a.TotalPrice,
                a.DeliveryPrice,
                a.CheckOutDate,
                a.ApproximateDeliveryDate,
                a.ActualDeliveryDate,
                a.SendDate,
                a.State,
                DeliveryTrackNo = a.TrackNo,
                a.DelivererNo,
                a.User.FirstName,
                a.User.LastName,
                a.AddressId,
                a.Address,
                items = a.Items.Select(i=>new
                {
                    i.Id,
                    i.FinalPrice,
                    i.UnitPrice,
                    i.ProductPrice.ProductId,
                    i.ProductPrice.Product.Title,
                    i.ProductPrice.Product.Code,
                    i.ProductPrice.Product.Thubmnail,
                    i.Quantity,
                    i.State,
                })
            }).ToListAsync();

            return data.Select(a => new OrderModel
            {
                ActualDeliveryDate = Util.GetPersianDate(a.ActualDeliveryDate),
                ApproximateDeliveryDate = Util.GetPersianDate(a.ApproximateDeliveryDate),
                CheckoutDate = Util.GetPersianDate(a.CheckOutDate),
                DeliveryTrackNo = a.DeliveryTrackNo,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Id = a.Id,
                Price = a.Price,
                DeliveryPrice = a.DeliveryPrice,
                SendDate = Util.GetPersianDate( a.SendDate),
                State = TranslateExtentions.GetTitle(a.State),
                Address_Id = a.AddressId,
                Address = a.Address == null ? null :
                new AddressModel(a.Address),
                //{
                //    Id = a.Address.Id,
                //    Title = a.Address.Title,
                //    City = a.Address.CityId,
                //    Province = a.Address.City == null ? 0 : a.Address.City.ProvinceId,
                //    Details = a.Address.Details,
                //    Postal_Code = a.Address.PostalCode,
                //    Lat = a.Address.Lat,
                //    Lng = a.Address.Lng
                //},
                OrderItems = a.items.Select(i=>new OrderItemModel
                {
                    Code = i.Code,
                    Title = i.Title,
                    DiscountedPrice = i.FinalPrice,
                    UnitPrice = i.UnitPrice,
                    Image = productImageService.GetImageUrl(i.ProductId, i.Thubmnail),
                    OrderItemId = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            }).ToList();
        }
    }
}
