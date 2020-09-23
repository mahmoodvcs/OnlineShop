using MahtaKala.SharedServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public abstract class ImageServiceBase
    {
        protected readonly ImagesPathStrategy pathStrategy;
        protected readonly IPathService pathService;
        private readonly ILogger<ImageServiceBase> logger;

        public string ImagesPath { get; protected set; }
        public ImageServiceBase(
            ImagesPathStrategy pathStrategy,
            IPathService pathService,
            ILogger<ImageServiceBase> logger)
        {
            this.pathStrategy = pathStrategy;
            this.pathService = pathService;
            this.logger = logger;
        }

        public byte[] GetImage(long productId, string name)
        {
            if (name.StartsWith("http://") || name.StartsWith("https://"))
                return null;
            var path = pathStrategy.GetPath(ImagesPath, productId, name);
            if (!File.Exists(path))
            {
                logger.LogError("Image file does not exit: " + path);
                return null;
            }
            return File.ReadAllBytes(path);
        }

        public async Task SaveImage(long id, string name, Stream stream)
        {
            byte[] bs = new byte[4096];
            int read;

            using (var output = new FileStream(pathStrategy.GetPath(ImagesPath, id, name), FileMode.Create, FileAccess.Write))
            {
                while ((read = await stream.ReadAsync(bs, 0, bs.Length)) > 0)
                {
                    await output.WriteAsync(bs, 0, read);
                }
            }
        }

        public void DeleteImage(long id, string fileName)
        {
            File.Delete(pathStrategy.GetPath(ImagesPath, id, fileName));
        }

        public void DeleteImages(long id)
        {
            Directory.Delete(pathStrategy.GetPath(ImagesPath, id, null), true);
        }


    }
}
