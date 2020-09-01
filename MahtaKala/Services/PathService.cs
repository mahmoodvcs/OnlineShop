using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class PathService : IPathService
    {
        public PathService(IWebHostEnvironment env, IHttpContextAccessor contextAccessor)
        {
            AppRoot = env.ContentRootPath;
            this.contextAccessor = contextAccessor;
        }
        private readonly IHttpContextAccessor contextAccessor;

        public HttpContext Current => contextAccessor.HttpContext;
        public string AppRoot { get; }

        string _siteUrl;
        public string AppBaseUrl
        {
            get
            {
                if (_siteUrl == null)
                {
                    _siteUrl = $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";
                }
                return _siteUrl;
            }
        }
    }
}
