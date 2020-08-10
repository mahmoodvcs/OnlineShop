using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MahtaKala.Entities;
using MahtaKala.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MahtaKala.Services
{
    public interface IUserService
    {
        Task<AuthenticateResponse> Authenticate(User user, string ipAddress);
        Task<AuthenticateResponse> RefreshToken(string token, string ipAddress);
        bool RevokeToken(string token, string ipAddress);
        IEnumerable<User> GetAll();
        User GetById(int id);
    }

    public class UserService : IUserService
    {
        private DataContext context;
        private readonly string jwtSecret;

        public UserService(
            DataContext context,
            IConfiguration configuration)
        {
            this.context = context;
            jwtSecret = configuration.GetSection("AppSettings")["JwtSecret"];
        }

        public async Task<AuthenticateResponse> Authenticate(User user, string ipAddress)
        {
            // authentication successful so generate jwt and refresh tokens
            var jwtToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken(ipAddress);

            // save refresh token
            user.RefreshTokens.Add(refreshToken);
            context.Update(user);
            await context.SaveChangesAsync();

            return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
        }

        public async Task<AuthenticateResponse> RefreshToken(string token, string ipAddress)
        {
            var user = await context.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            // return null if no user found with token
            if (user == null) return null;

            var refreshToken = await context.RefreshTokens.SingleOrDefaultAsync(x => x.UserId == user.Id && x.Token == token);

            // return null if token is no longer active
            if (!refreshToken.IsActive) return null;

            // replace old refresh token with a new one and save
            var newRefreshToken = GenerateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            context.Update(user);
            await context.SaveChangesAsync();

            // generate new jwt
            var jwtToken = GenerateJwtToken(user);

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

        public IEnumerable<User> GetAll()
        {
            return context.Users;
        }

        public User GetById(int id)
        {
            return context.Users.Find(id);
        }

        // helper methods

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Claims = new Dictionary<string, object>()
                {
                    { "UserType" , user.Type },
                    { "FirstName", user.FirstName },
                    { "LastName", user.LastName }
                }
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
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }
    }
}