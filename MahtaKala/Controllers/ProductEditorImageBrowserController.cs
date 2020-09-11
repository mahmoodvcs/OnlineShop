using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using MahtaKala.GeneralServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MahtaKala.Controllers
{
    public class ProductEditorImageBrowserController : EditorImageBrowserController
    {
        private string ProductsImagesPath { get; set; }
        private string ProductsEditorImagesFolder { get; set; }
        private IFileService FileService { get; set; }
        private string Id { get; set; }


        public ProductEditorImageBrowserController(
            IHostingEnvironment hostingEnvironment,
            IConfiguration configuration,
            IFileService fileService
            )
            : base(hostingEnvironment)
        {
            FileService = fileService;
            ProductsImagesPath = configuration.GetSection("AppSettings")["ProductImagesPath"];
            ProductsEditorImagesFolder = configuration.GetSection("AppSettings")["ProductsEditorImagesFolder"];

        }

        /// <summary>
        /// Gets the base paths from which content will be served.
        /// </summary>
        public override string ContentPath => FileService.GetAbsolutePath(Path.Combine(ProductsImagesPath, ProductsEditorImagesFolder, Id));
        [HttpPost]
        public string Address(string Id)
        {
            CreateFolder(Id);
            return ContentPath;
        }

        
        [HttpPost]
        public override JsonResult Read(string Id)
        {
            CreateFolder(Id);
            var files = Directory.GetFiles(ContentPath);
            List<Dictionary<string, object>> datas = new List<Dictionary<string, object>>();
            //List<FileInfoJson> data = new List<FileInfoJson>();
            foreach(var file in files)
            {
                var fi = new FileInfo(file);
                var data = new Dictionary<string, object>();
                data.Add("name", fi.Name);
                data.Add("size", fi.Length);
                data.Add("type", "f");
                datas.Add(data);
                //data.Add(new FileInfoJson()
                //{
                //    Name = fi.Name,
                //    Size = fi.Length,
                //    Type = "f"
                //});
            }
            return Json(datas);
        }

        [HttpPost]
        public override ActionResult Create(string Id, FileBrowserEntry entry)
        {
            CreateFolder(Id);
            return base.Create(ContentPath, entry);
        }


        [HttpPost]
        public override ActionResult Destroy(string Id, FileBrowserEntry entry)
        {
            CreateFolder(Id);
            base.Destroy(ContentPath, entry);
            return Read(Id);
        }

        [HttpPost]
        public override ActionResult Upload(string Id, IFormFile file)
        {
            CreateFolder(Id);
            base.Upload(ContentPath, file);
            return Read(Id);
        }
        
        public override IActionResult Thumbnail(string Id)
        {
            CreateFolder(Id);
            return Json(base.Thumbnail(ContentPath));
        }



        private void CreateFolder(string Id)
        {
            this.Id = Id;
            var path = ContentPath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        class FileInfoJson
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public long Size { get; set; }
        }
    }
}
