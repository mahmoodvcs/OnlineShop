using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly IUrlHelper urlHelper;

        public ProductImageService(IUrlHelper urlHelper)
        {
            this.urlHelper = urlHelper;
        }

        public string GetImageUrl(long productId, string name)
        {
            name = name.ToLower();
            if (name.StartsWith("http://") || name.StartsWith("https://"))
                return name;
            return urlHelper.Action("Product", "Image", new { id = productId, name = name });    
        }

        public string SaveImage()
    }
}
