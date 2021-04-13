using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure
{
    public class CorsMiddleware
    {
        private readonly RequestDelegate _next;

        public CorsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            AddCorsResponseHeaders(httpContext);
            if(httpContext.Request.Method.ToUpper() == "OPTIONS")
            {
                httpContext.Response.StatusCode = 200;
                return;
            }
            await _next(httpContext);
            return;
        }

        const string OPTIONS = "OPTIONS";
        const string PUT = "PUT";
        const string POST = "POST";
        const string PATCH = "PATCH";
        static string[] AllowedVerbs = new[] { OPTIONS, PUT, POST, PATCH, "GET", "DELETE" };
        const string Origin = "Origin";
        const string AccessControlRequestMethod = "Access-Control-Request-Method";
        const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";
        const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";
        const string AccessControlMaxAge = "Access-Control-Max-Age";
        const string MaxAge = "86400";


        public static void AddCorsResponseHeaders(HttpContext context)
        {
            if (Array.Exists(AllowedVerbs, av => string.Compare(context.Request.Method, av, true) == 0))
            {
                var request = context.Request;
                var response = context.Response;
                var originArray = request.Headers[Origin];
                var accessControlRequestMethodArray = request.Headers[AccessControlRequestMethod];
                var accessControlRequestHeadersArray = request.Headers[AccessControlRequestHeaders];
                if (originArray.Count > 0)
                    response.Headers.Add(AccessControlAllowOrigin, originArray[0]);
                response.Headers.Add(AccessControlAllowCredentials, bool.TrueString.ToLower());

                if (accessControlRequestMethodArray.Count > 0)
                {
                    string accessControlRequestMethod = accessControlRequestMethodArray[0];
                    if (!string.IsNullOrEmpty(accessControlRequestMethod))
                    {
                        response.Headers.Add(AccessControlAllowMethods, accessControlRequestMethod);
                    }
                }
                if (accessControlRequestHeadersArray.Count > 0)
                {
                    string requestedHeaders = string.Join(", ", accessControlRequestHeadersArray);
                    if (!string.IsNullOrEmpty(requestedHeaders))
                    {
                        response.Headers.Add(AccessControlAllowHeaders, requestedHeaders);
                    }
                }
            }
        }

    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CorsMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorsMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorsMiddleware>();
        }
    }
}
