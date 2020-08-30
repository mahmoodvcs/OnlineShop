using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class ImagesPathStrategy
    {
        private readonly IPathService pathService;

        public ImagesPathStrategy(IPathService pathService)
        {
            this.pathService = pathService;
        }
        public string GetPath(string rootPath, long id, string name)
        {
            if (rootPath.StartsWith("~/"))
            {
                rootPath = Path.Combine(pathService.AppRoot, rootPath.Remove(0, 2));
            }

            if (id > 999999)
            {
                return GetPath(Path.Combine(rootPath, "b"), id / 1000000, name);
            }

            var s = id.ToString("000000");
            var p = int.Parse(s.Substring(0, 2)).ToString();
            var path = Path.Combine(rootPath, p);

            p = int.Parse(s.Substring(2, 2)).ToString();
            path = Path.Combine(rootPath, p);
            p = int.Parse(s.Substring(4, 2)).ToString();
            path = Path.Combine(rootPath, p);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Path.Combine(path, name);
        }
    }
}
