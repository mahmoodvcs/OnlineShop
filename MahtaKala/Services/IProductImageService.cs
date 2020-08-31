using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public interface IProductImageService : IImageService
    {
        IEnumerable<string> GetImageUrls(long productId, IEnumerable<string> names);
        void FixImageUrls(Product p);
        IEnumerable<string> GetImageUrls(Product p);
        string GetThumbnailUrl(Product p);
    }

}
