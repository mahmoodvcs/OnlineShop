using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MahtaKala.Entities;
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
        Task<AuthenticateResponse> Authenticate(User user, string ipAddress);
        Task<AuthenticateResponse> RefreshToken(string token, string ipAddress);
        bool RevokeToken(string token, string ipAddress);
        User GetById(int id);
        Task Update(User user);
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
            context.RefreshTokens.Add(newRefreshToken);
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
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Type.ToString()),
                    new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
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
    }
}