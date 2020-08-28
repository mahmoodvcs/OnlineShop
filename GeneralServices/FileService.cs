using MahtaKala.SharedServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MahtaKala.GeneralServices
{
    public interface IFileService
    {

        void SaveFile(byte[] file, string path, string fileName);
        byte[] GetFile(string path, string fileName);
        void DeleteFile(string path, string fileName);
    }
    public class FileService : IFileService
    {
        private IPathService PathService { get; set; }
        public FileService(IPathService pathService)
        {
            PathService = pathService;
        }
        public void SaveFile(byte[] file, string path, string fileName)
        {
            string absolutePath = GetAbsolutePath(path);
            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }
            File.WriteAllBytes(Path.Combine(absolutePath, fileName), file);
        }

        public byte[] GetFile(string path, string fileName)
        {
            return File.ReadAllBytes(Path.Combine(GetAbsolutePath(path), fileName));
        }

        public void DeleteFile(string path, string fileName)
        {
            File.Delete(Path.Combine(GetAbsolutePath(path), fileName));
        }




        private string GetAbsolutePath(string path)
        {
            if (path.StartsWith("~"))
            {
                return Path.Combine(PathService.AppRoot, path.Substring(2));
            }
            return path;
        }
    }
}
