using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.Services;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MahtaKala.Controllers
{
    public class ImageController : ApiControllerBase<ImageController>
    {
        private readonly IProductImageService productImageService;
        private string ProductsImagesPath { get; set; }
        public ImageController(
            DataContext context,
            ILogger<ImageController> logger,
            IProductImageService productImageService 
            ) : base(context, logger)
        {
            this.productImageService = productImageService;
        }
        public FileContentResult Product(long id, string name)
        {
            return File(productImageService.GetImage(id, name), "image/jpeg");
        }
    }
}
