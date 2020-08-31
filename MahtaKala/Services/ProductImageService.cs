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
    public class ProductImageService : IProductImageService
    {
        private readonly ImagesPathStrategy pathStrategy;
        private readonly IPathService pathService;

        public string ProductsImagesPath { get; }

        public ProductImageService(
            ImagesPathStrategy pathStrategy,
            IConfiguration configuration,
            IPathService pathService)
        {
            this.pathStrategy = pathStrategy;
            this.pathService = pathService;
            ProductsImagesPath = configuration.GetSection("AppSettings")["ProductsImagesPath"];
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
        public byte[] GetImage(long productId, string name)
        {
            if (name.StartsWith("http://") || name.StartsWith("https://"))
                return null;
            return File.ReadAllBytes(pathStrategy.GetPath(ProductsImagesPath, productId, name));
        }

        public async Task SaveImage(long id, string name, Stream stream)
        {
            byte[] bs = new byte[4096];
            int read;

            using (var output = new FileStream(pathStrategy.GetPath(ProductsImagesPath, id, name), FileMode.Create, FileAccess.Write))
            {
                while ((read = await stream.ReadAsync(bs, 0, bs.Length)) > 0)
                {
                    await output.WriteAsync(bs, 0, read);
                }
            }
        }

        public void DeleteImage(long id, string fileName)
        {
            File.Delete(pathStrategy.GetPath(ProductsImagesPath, id, fileName));
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

        public IEnumerable<string> GetImageUrls(long productId, IEnumerable<string> names)
        {
            return names.Select(a => GetImageUrl(productId, a));
        }
    }
}
