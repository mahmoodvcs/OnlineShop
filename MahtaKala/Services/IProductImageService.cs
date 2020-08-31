using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public interface IProductImageService
    {
        byte[] GetImage(long productId, string name);
        string GetImageUrl(long productId, string name);
        IEnumerable<string> GetImageUrls(long productId, IEnumerable<string> names);
        Task SaveImage(long id, string fileName, Stream stream);
        void DeleteImage(long id, string fileName);
        void FixImageUrls(Product p);
        IEnumerable<string> GetImageUrls(Product p);
        string GetThumbnailUrl(Product p);
    }

}
