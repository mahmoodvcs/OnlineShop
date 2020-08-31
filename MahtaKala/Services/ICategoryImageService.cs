using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public interface ICategoryImageService : IImageService
    {
        void FixImageUrls(IEnumerable<Category> cs);
        void FixImageUrls(Category c);
    }

}
