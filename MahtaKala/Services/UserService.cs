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
using Z.EntityFramework.Plus;

namespace MahtaKala.Services
{
    public interface IUserService
    {
        Task<AuthenticateResponse> Authenticate(User user, string ipAddress, UserClient client);
        Task<AuthenticateResponse> RefreshToken(string token, string ipAddress);
        bool RevokeToken(string token, string ipAddress);
        User GetById(long id);
        Task<User> GetByIdAsync(long id);
        Task Update(User user);
        void CreateAdminUserIfNotExist();
        void Logout();
        Task<bool> IsValidToken(User user, string token, string ipAddress);
    }

    public class UserService : IUserService
    {
        private DataContext db;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ISMSService smsService;
        private readonly SettingsService settingsService;
        private readonly string jwtSecret;

        public UserService(
            DataContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ISMSService smsService,
            SettingsService settingsService)
        {
            this.db = context;
            this.httpContextAccessor = httpContextAccessor;
            this.smsService = smsService;
            this.settingsService = settingsService;
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
                db.Update(user);
                await db.SaveChangesAsync();
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

        private async Task AddUserSession(User user, string jwtToken, string ipAddress)
        {
            UserSession session = new UserSession()
            {
                IPAddress = ipAddress,
                LastActivityDate = DateTime.Now,
                LoginDate = DateTime.Now,
                Token = jwtToken,
                UserId = user.Id
            };
            db.UserSessions.Add(session);
            await db.SaveChangesAsync();
        }

        public async Task<bool> IsValidToken(User user, string token, string ipAddress)
        {
            var query = db.UserSessions.Where(a => a.UserId == user.Id && a.Token == token);
            if (settingsService.UserSessionBasedOnIP)
            {
                query = query.Where(a => a.IPAddress == ipAddress);
            }
            var session = await query.FirstOrDefaultAsync();
            if (session == null)
                return false;

            if(session.LastActivityDate < DateTime.Now.AddMinutes(-UserSesionMinutes))
            {
                db.Remove(session);
                await db.SaveChangesAsync();
                return false;
            }
            session.LastActivityDate = DateTime.Now;
            await db.SaveChangesAsync();
            return true;
        }

        private async Task HandleMaxActiveSessions(User user)
        {
            if (user.Type == UserType.Customer)
                return;
            var max = settingsService.MaxNumberOfActiveUserSessions - 1;
            await db.UserSessions.Where(a => a.UserId == user.Id).OrderByDescending(a=>a.LastActivityDate).Skip(max).DeleteAsync();
        }

        private const int UserSesionMinutes = 20;

        DateTime GetTokenExpirationTime(User user, UserClient client)
        {
            TimeSpan span;
            if (client == UserClient.WebSite)
                span = TimeSpan.FromDays(365);
            else
                span =TimeSpan.FromMinutes(15);
            return DateTime.Now + span;
        }

        public async Task<AuthenticateResponse> AuthenticateStaff(User user, string ipAddress)
        {
            DateTime expireTime = GetTokenExpirationTime(user, UserClient.WebSite);
            var jwtToken = GenerateJwtToken(user, UserClient.WebSite);

            await HandleMaxActiveSessions(user);
            await AddUserSession(user, jwtToken, ipAddress);

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
            var refreshToken = await db.RefreshTokens.SingleOrDefaultAsync(x => x.Token == token);

            // return null if token is no longer active
            if (refreshToken == null || !refreshToken.IsActive)
                return null;

            var user = db.Users.Find(refreshToken.UserId);

            // replace old refresh token with a new one and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            var newRefreshToken = GenerateRefreshToken(ipAddress);
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);
            await db.SaveChangesAsync();

            // generate new jwt
            var jwtToken = GenerateJwtToken(user, UserClient.Api);

            return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
        }

        public bool RevokeToken(string token, string ipAddress)
        {
            var user = db.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            // return false if no user found with token
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            db.Update(user);
            db.SaveChanges();

            return true;
        }

        public User GetById(long id)
        {
            return db.Users.Find(id);
        }

        public async Task<User> GetByIdAsync(long id)
		{
            return await db.Users.FindAsync(id);
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
            db.Users.Attach(user);
            await db.SaveChangesAsync();
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
            var user = db.Users.FirstOrDefault(a => a.Username.ToLower() == "admin" || a.MobileNumber.Contains("9357597227"));
            if (user == null)
            {
                user = User.Create("admin", "damnit", null, UserType.Admin);
                user.MobileNumber = "09357597227";
                db.Users.Add(user);
                db.SaveChanges();
            }
            else
            {
                if (user.Type != UserType.Admin)
                {
                    user.Type = UserType.Admin;
                    db.SaveChanges();
                    //throw new Exception("Admin user found. But it's type is not Admin.");
                }
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