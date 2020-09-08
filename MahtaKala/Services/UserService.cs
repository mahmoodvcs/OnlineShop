using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.GeneralServices;
using MahtaKala.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MahtaKala.Services
{
    public interface IUserService
    {
        Task<AuthenticateResponse> Authenticate(User user, string ipAddress, UserClient client);
        Task<AuthenticateResponse> RefreshToken(string token, string ipAddress);
        bool RevokeToken(string token, string ipAddress);
        User GetById(long id);
        Task Update(User user);
        void CreateAdminUserIfNotExist();
        void Logout();
    }

    public class UserService : IUserService
    {
        private DataContext context;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ISMSService smsService;
        private readonly string jwtSecret;

        public UserService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ISMSService smsService)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            this.smsService = smsService;
            jwtSecret = configuration.GetSection("AppSettings")["JwtSecret"];
        }

        public async Task<AuthenticateResponse> Authenticate(User user, string ipAddress, UserClient client)
        {
            if (user.Type != UserType.Customer)
                return await AuthenticateStaff(user, ipAddress);

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = GenerateJwtToken(user, client);
            RefreshToken refreshToken = null;
            if (client == UserClient.Api)
            {
                refreshToken = GenerateRefreshToken(ipAddress);

                // save refresh token
                user.RefreshTokens.Add(refreshToken);
                context.Update(user);
                await context.SaveChangesAsync();
            }
            else
            {
                httpContextAccessor.HttpContext.Response.Cookies.Append(AuthCookieName, jwtToken, new CookieOptions
                {
                    Expires = GetTokenExpirationTime(user, client),
                    HttpOnly = true
                });
            }
            return new AuthenticateResponse(user, jwtToken, refreshToken?.Token);
        }

        DateTime GetTokenExpirationTime(User user, UserClient client)
        {
            TimeSpan span;
            if (user.Type != UserType.Customer)
                span = TimeSpan.FromDays(1);
            else if (client == UserClient.WebSite)
                span = TimeSpan.FromDays(365);
            else
                span =TimeSpan.FromMinutes(15);
            return DateTime.Now + span;
        }

        public async Task<AuthenticateResponse> AuthenticateStaff(User user, string ipAddress)
        {
            DateTime expireTime = GetTokenExpirationTime(user, UserClient.WebSite);
            var jwtToken = GenerateJwtToken(user, UserClient.WebSite);

            //context.RefreshTokens.Add(new Entities.RefreshToken
            //{
            //    UserId = user.Id,
            //    Token = jwtToken,
            //    Expires = expireTime,
            //    Created= DateTime.Now,
            //    CreatedByIp = ipAddress
            //});
            //context.RefreshTokens.
            //await context.SaveChangesAsync();

            httpContextAccessor.HttpContext.Response.Cookies.Append(AuthCookieName, jwtToken, new CookieOptions
            {
                HttpOnly = true,
            });

            return new AuthenticateResponse(user, jwtToken, null);
        }

        public async Task<AuthenticateResponse> RefreshToken(string token, string ipAddress)
        {
            var refreshToken = await context.RefreshTokens.SingleOrDefaultAsync(x => x.Token == token);

            // return null if token is no longer active
            if (refreshToken == null || !refreshToken.IsActive)
                return null;

            var user = context.Users.Find(refreshToken.UserId);

            // replace old refresh token with a new one and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            var newRefreshToken = GenerateRefreshToken(ipAddress);
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);
            await context.SaveChangesAsync();

            // generate new jwt
            var jwtToken = GenerateJwtToken(user, UserClient.Api);

            return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
        }

        public bool RevokeToken(string token, string ipAddress)
        {
            var user = context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            // return false if no user found with token
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            context.Update(user);
            context.SaveChanges();

            return true;
        }

        public User GetById(long id)
        {
            return context.Users.Find(id);
        }

        // helper methods

        private string GenerateJwtToken(User user, UserClient client)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Type.ToString()),
                    new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
                }),
                Expires = GetTokenExpirationTime(user, client).ToUniversalTime(),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddYears(1),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }

        public async Task Update(User user)
        {
            context.Users.Attach(user);
            await context.SaveChangesAsync();
        }

        public const string AuthCookieName = "MahtaAuth";
        public Cookie GetAuthCookie(User user, string jwtToken)
        {
            Cookie cookie = new Cookie(AuthCookieName, jwtToken);
            cookie.HttpOnly = true;
            cookie.Expires = DateTime.Now.AddYears(1);
            return cookie;
        }

        public void CreateAdminUserIfNotExist()
        {
            var user = context.Users.FirstOrDefault(a => a.Username.ToLower() == "admin");
            if (user == null)
            {
                user = User.Create("admin", "123456", null, UserType.Admin);
                context.Users.Add(user);
                context.SaveChanges();
            }
            else
            {
                if (user.Type != UserType.Admin)
                    throw new Exception("Admin user found. But it's type is not Admin.");
            }
        }

        public void Logout()
        {
            httpContextAccessor.HttpContext.Response.Cookies.Delete(AuthCookieName);
        }
    }

    public enum UserClient
    {
        Api,
        WebSite
    }
}