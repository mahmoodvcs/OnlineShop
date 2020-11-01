using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahtaKala.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using MahtaKala.Helpers;
using Microsoft.VisualBasic;

namespace MahtaKala.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string jwtSecret;

        public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            jwtSecret = configuration.GetSection("AppSettings")["JwtSecret"];
        }

        public async Task Invoke(HttpContext context, IUserService userService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                await attachUserToContext(context, userService, token);
            else
            {
                token = context.Request.Cookies["MahtaAuth"];
                if (token != null)
                    await attachUserToContext(context, userService, token);
            }

            await _next(context);
        }

        private async Task attachUserToContext(HttpContext context, IUserService userService, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(jwtSecret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = long.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                var user = userService.GetById(userId);
                if(user.Type != Entities.UserType.Customer)
                {
                    if (!await userService.IsValidToken(user, token, Util.GetIpAddress(context)))
                        return;
                }

                context.Items["User"] = user;

                //context.AuthenticateAsync();
                //context.User = new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity[]
                //{
                //    new ClaimsIdentity(jwtToken.Claims),
                //});

            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}