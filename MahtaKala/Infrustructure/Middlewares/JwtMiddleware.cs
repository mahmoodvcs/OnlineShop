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
using System.Security.Cryptography;
using System.Text.Json;

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

        public async Task HandleTokenTest(string token)
        {
            var tokenPartsBase64 = token.Split('.');
            var tokenPartsBytes = tokenPartsBase64.Select(x => Convert.FromBase64String(x)).ToArray();
            var headerString = Encoding.UTF8.GetString(tokenPartsBytes[0]);
            var payloadString = Encoding.UTF8.GetString(tokenPartsBytes[1]);
            var signature = tokenPartsBytes[2];
            var hasher = new HMACSHA256(Encoding.UTF8.GetBytes("123"));
            var generatedSignature = hasher.ComputeHash(Encoding.UTF8.GetBytes(tokenPartsBase64[0] + "." + tokenPartsBase64[1]));
            Console.WriteLine($"Received Signature - length: {signature.Length}");
            Console.WriteLine($"Generated Signature - length: {generatedSignature.Length}");
            if (signature.Length != generatedSignature.Length)
			{
                Console.WriteLine("So far, the two signatures have different lengths! This is gonna be a looooong night!");
			}
			else
			{
                Console.WriteLine("Congrats! The first check seems to be ok! Now, let's check the whole thing, and see what happens! Keep your fingers crossed!");
			}
            bool tokenSignatureIsValid = true;
            for (int i = 0; i < Math.Min(signature.Length, generatedSignature.Length); i++)
            {
                if (signature[i] != generatedSignature[i])
				{
                    tokenSignatureIsValid = false;
                    break;
                }
            }
            if (tokenSignatureIsValid)
			{
                Console.WriteLine("Yaaaayyyyy!");
			}
			else
			{
                Console.WriteLine("Fuck it man! Fuck it!");
			}
        }

        private async Task attachUserToContext(HttpContext context, IUserService userService, string token)
        {
			try
			{
                var tokenPartsBase64 = token.Split('.');
                var headerBytes = Base64UrlTextEncoder.Decode(tokenPartsBase64[0]);
                var payloadBytes = Base64UrlTextEncoder.Decode(tokenPartsBase64[1]);
                var headerString = Encoding.UTF8.GetString(headerBytes);
                var payloadString = Encoding.UTF8.GetString(payloadBytes);
                var signature = tokenPartsBase64[2];
                var hasher = new HMACSHA256(Encoding.UTF8.GetBytes("123"));
                var generatedSignatureBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(tokenPartsBase64[0] + "." + tokenPartsBase64[1]));
                var generatedSignature = Base64UrlTextEncoder.Encode(generatedSignatureBytes);
                if (signature.Equals(generatedSignature))
				{
                    //var payload = JsonSerializer.Deserialize(payloadString);
				}
            }
            catch (Exception e)
			{

			}
			return;
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