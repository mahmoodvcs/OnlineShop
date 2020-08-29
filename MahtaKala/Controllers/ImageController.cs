using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MahtaKala.Controllers
{
    public class ImageController : ApiControllerBase<ImageController>
    {
        private string ProductsImagesPath { get; set; }
        private IFileService FileService { get; set; }
        public ImageController(
            DataContext context,
            ILogger<ImageController> logger,
            IConfiguration configuration,
            IFileService fileService
            ) : base(context, logger)
        {
            ProductsImagesPath = configuration.GetSection("AppSettings")["ProductsImagesPath"];
            this.FileService = fileService;
        }
        public FileContentResult Product(long id, string name)
        {
            var path = Path.Combine(ProductsImagesPath, id.ToString());
            return base.File(FileService.GetFile(path, name), "image/jpeg");
        }
    }
}
