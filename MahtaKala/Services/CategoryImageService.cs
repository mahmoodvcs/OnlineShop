using MahtaKala.Entities;
using MahtaKala.SharedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class CategoryImageService : ImageServiceBase, ICategoryImageService
    {
        public CategoryImageService(ImagesPathStrategy pathStrategy,
            IConfiguration configuration,
            ILogger<ImageServiceBase> logger,
            IPathService pathService) : base(pathStrategy, pathService, logger)
        {
            ImagesPath = configuration.GetSection("AppSettings")["CategoryImagesPath"];
        }

        public string GetImageUrl(long id, string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            name = name.ToLower();
            if (name.StartsWith("http://") || name.StartsWith("https://") || name.Contains("image/category?id="))
                return name;
            return $"/Image/Category?id={id}&name={name}";
        }

        public void FixImageUrls(Category c)
        {
            c.Image = GetImageUrl(c.Id, c.Image);
        }
        public void FixImageUrls(IEnumerable<Category> cs)
        {
            foreach (var item in cs)
            {
                item.Image = GetImageUrl(item.Id, item.Image);
            }
        }
    }
}
