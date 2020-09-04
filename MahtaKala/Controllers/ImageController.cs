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
        private readonly ICategoryImageService categoryImageService;

        public ImageController(
            DataContext context,
            ILogger<ImageController> logger,
            IProductImageService productImageService,
            ICategoryImageService categoryImageService
            ) : base(context, logger)
        {
            this.productImageService = productImageService;
            this.categoryImageService = categoryImageService;
        }

        [ResponseCache(VaryByQueryKeys = new string[] { "id", "name" }, Duration = 100)]
        public FileContentResult Product(long id, string name)
        {
            var img = productImageService.GetImage(id, name);
            if (img == null)
            {
                img = new byte[0];
            }
            return File(img, "image/jpeg");
        }
        [ResponseCache(VaryByQueryKeys = new string[] { "id", "name" }, Duration = 100)]
        public FileContentResult Category(long id, string name)
        {
            var img = categoryImageService.GetImage(id, name);
            if (img == null)
            {
                img = new byte[0];
            }

            return File(img, "image/jpeg");
        }
    }
}
