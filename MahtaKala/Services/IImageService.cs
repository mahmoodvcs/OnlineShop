using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public interface IImageService
    {
        byte[] GetImage(long id, string name);
        string GetImageUrl(long id, string name);
        Task SaveImage(long id, string fileName, Stream stream);
        void DeleteImage(long id, string fileName);
    }
}
