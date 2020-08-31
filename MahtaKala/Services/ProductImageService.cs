using MahtaKala.Entities;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class ProductImageService : ImageServiceBase, IProductImageService
    {

        public ProductImageService(
            ImagesPathStrategy pathStrategy,
            IConfiguration configuration,
            IPathService pathService)
            :base(pathStrategy, pathService)
        {
            ImagesPath = configuration.GetSection("AppSettings")["ProductImagesPath"];
        }

        public string GetImageUrl(long productId, string name)
        {
            name = name.ToLower();
            if (name.StartsWith("http://") || name.StartsWith("https://"))
                return name;
            return $"{pathService.AppBaseUrl}/Image/Product?id={productId}&name={name}";
        }
        public string GetThumbnailUrl(Product p)
        {
            return GetImageUrl(p.Id, p.Thubmnail);
        }

        public IEnumerable<string> GetImageUrls(Product p)
        {
            if (p.ImageList == null)
                return null;
            return p.ImageList.Select(a => GetImageUrl(p.Id, a));
        }
        public void FixImageUrls(Product p)
        {
            p.Thubmnail = GetImageUrl(p.Id, p.Thubmnail);
            if (p.ImageList != null)
            {
                var list = new List<string>();
                foreach (var item in p.ImageList)
                {
                    list.Add(GetImageUrl(p.Id, item));
                }
                p.ImageList = list;
            }
        }
        public void FixImageUrls(IEnumerable<Product> list)
        {
            foreach (var p in list)
            {
                FixImageUrls(p);
            }
        }

        public IEnumerable<string> GetImageUrls(long productId, IEnumerable<string> names)
        {
            return names.Select(a => GetImageUrl(productId, a));
        }
    }
}
