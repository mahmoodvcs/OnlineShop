using MahtaKala.SharedServices;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class CategoryImageService : ImageServiceBase, ICategoryImageService
    {
        public CategoryImageService(ImagesPathStrategy pathStrategy,
            IConfiguration configuration,
            IPathService pathService) : base(pathStrategy, pathService)
        {
            ImagesPath = configuration.GetSection("AppSettings")["CategoryImagesPath"];
        }

        public string GetImageUrl(long id, string name)
        {
            name = name.ToLower();
            if (name.StartsWith("http://") || name.StartsWith("https://"))
                return name;
            return $"{pathService.AppBaseUrl}/Image/Category?id={id}&name={name}";

        }
    }
}
