using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure
{
    public class AppSettings
    {
        public AppSettings(IConfiguration configuration, IWebHostEnvironment env)
        {

            StaticFilesPath = Path.Combine(env.WebRootPath, configuration.GetSection("AppSettings")["StaticFilesPath"]);
            ProductImagesPath = Path.Combine(StaticFilesPath, "ProductImages");

        }
        public string JwtSecret { get; set; }
        public string StaticFilesPath { get; private set; }
        public string ProductImagesPath { get; private set; }
    }
}
