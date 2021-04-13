using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public static class MyAppContext
    {
        static IHttpContextAccessor contextAccessor;

        public static void SetHttpContextAccessor(IHttpContextAccessor contextAccessor)
        {
            MyAppContext.contextAccessor = contextAccessor;
        }

        public static HttpContext Current => contextAccessor.HttpContext;

        public static TService ResolveService<TService>()
        {
            return (TService)Current.RequestServices.GetService(typeof(TService));
        }
    }
}
