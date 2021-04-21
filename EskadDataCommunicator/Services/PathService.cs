using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class PathService : IPathService
    {
        public PathService(IWebHostEnvironment env, 
            IHttpContextAccessor contextAccessor, 
            IConfiguration configuration)
        {
            AppRoot = env.ContentRootPath;
            this.contextAccessor = contextAccessor;
            this.configuration = configuration;
        }
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IConfiguration configuration;

        public HttpContext Current => contextAccessor.HttpContext;
        public string AppRoot { get; }

        string _siteUrl;
        public string AppBaseUrl
        {
            get
            {
                if (_siteUrl == null)
                {
                    _siteUrl = configuration.GetSection("AppSettings")["AppBaseUrl"];
                    if (string.IsNullOrEmpty(_siteUrl))
                    {
                        _siteUrl = $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";
                    }
                }
                return _siteUrl;
            }
        }
    }
}
