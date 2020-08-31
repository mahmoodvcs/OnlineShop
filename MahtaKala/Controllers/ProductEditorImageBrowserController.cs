using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using MahtaKala.GeneralServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MahtaKala.Controllers
{
    public class ProductEditorImageBrowserController : EditorImageBrowserController
    {
        private const string contentFolderRoot = "shared/";
        private const string folderName = "Images/";
        private static readonly string[] foldersToCopy = new[] { "shared/images/employees" };
        private string ProductsImagesPath { get; set; }
        private string ProductsEditorImagesFolder { get; set; }
        private IFileService FileService { get; set; }


        public ProductEditorImageBrowserController(
            IHostingEnvironment hostingEnvironment,
            IConfiguration configuration,
            IFileService fileService
            )
            :base(hostingEnvironment)
        {
            FileService = fileService;
            ProductsImagesPath = configuration.GetSection("AppSettings")["ProductsImagesPath"];
            ProductsEditorImagesFolder = configuration.GetSection("AppSettings")["ProductsEditorImagesFolder"];

        }

        /// <summary>
        /// Gets the base paths from which content will be served.
        /// </summary>
        public override string ContentPath
        {
            get
            {
                return CreateUserFolder();
            }
        }
        private string CreateUserFolder()
        {
            //var path = Path.Combine(ProductsImagesPath, ProductsEditorImagesFolder, id.ToString());
            var virtualPath = Path.Combine(contentFolderRoot, "UserFiles", folderName);
            var path = HostingEnvironment.WebRootFileProvider.GetFileInfo(virtualPath).PhysicalPath;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                foreach (var sourceFolder in foldersToCopy)
                {
                    CopyFolder(HostingEnvironment.WebRootFileProvider.GetFileInfo(sourceFolder).PhysicalPath, path);
                }
            }
            return virtualPath;
        }

        private void CopyFolder(string source, string destination)
        {
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            foreach (var file in Directory.EnumerateFiles(source))
            {
                var dest = Path.Combine(destination, Path.GetFileName(file));
                System.IO.File.Copy(file, dest);
            }

            foreach (var folder in Directory.EnumerateDirectories(source))
            {
                var dest = Path.Combine(destination, Path.GetFileName(folder));
                CopyFolder(folder, dest);
            }
        }

        public JsonResult Read(string path, long Id)
        {
            return base.Read(path + "/" + Id.ToString());
        }
    }
}
