﻿using MahtaKala.Entities;
using MahtaKala.SharedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
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
                return "/img/logo3.png";
            if (name.StartsWith("http://", StringComparison.OrdinalIgnoreCase) 
                || name.StartsWith("https://", StringComparison.OrdinalIgnoreCase) 
                || name.Contains("image/category?id=", StringComparison.OrdinalIgnoreCase))
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
