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
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Entities;
using Microsoft.EntityFrameworkCore;

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

        public async Task Invoke(HttpContext context, IUserService userService, DataContext dbContext)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                await attachUserToContext(context, userService, token, dbContext);
            else
            {
                token = context.Request.Cookies["MahtaAuth"];
                if (token != null)
                    await attachUserToContext(context, userService, token, dbContext);
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

        private async Task attachUserToContext(HttpContext context, IUserService userService, string token, DataContext dbContext)
        {
			try
			{
                string authorizatoinFailedMessage = "Authentication failed! Token invalid!";
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(token))
				{
                    throw new ApiException(401, authorizatoinFailedMessage);
				}
                var tokenPartsBase64 = token.Split('.');
                if (tokenPartsBase64.Length != 3)
				{
                    throw new ApiException(401, authorizatoinFailedMessage);
				}
                var headerBytes = Base64UrlTextEncoder.Decode(tokenPartsBase64[0]);
                var payloadBytes = Base64UrlTextEncoder.Decode(tokenPartsBase64[1]);
                var headerString = Encoding.UTF8.GetString(headerBytes);
                var payloadString = Encoding.UTF8.GetString(payloadBytes);
                var signature = tokenPartsBase64[2];
                var oneTwoThree = "123";    // This is just a chert temporary replacement! The following should use "jwtSecret" property, instead of this here oneTwoThree!
                var hasher = new HMACSHA256(Encoding.UTF8.GetBytes(oneTwoThree));
                var generatedSignatureBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(tokenPartsBase64[0] + "." + tokenPartsBase64[1]));
                var generatedSignature = Base64UrlTextEncoder.Encode(generatedSignatureBytes);
                if (!signature.Equals(generatedSignature))
				{
                    throw new ApiException(401, authorizatoinFailedMessage);
                    //var payload = JsonSerializer.Deserialize(payloadString);
				}
                var jwtToken = new JwtSecurityToken();
                //{"data":{"id":1000002,"phone_number":"09357597227"},"iat":1615767569,"exp":1615853969,"iss":"auth.mahtakala.ir","sub":"09357597227"}
                var payload = JsonSerializer.Deserialize<JwtPayloadModel>(payloadString);
                if (payload.Data.Id <= 0)
				{
                    // TODO: This here should be logged, that the Id value passed through token is invalid - or missing! Who knows!
                    throw new ApiException(401, authorizatoinFailedMessage);
				}
                payload.Data.PhoneNumber = payload.Data.PhoneNumber.Trim();
                if (!(payload.Data.PhoneNumber.StartsWith("09") && payload.Data.PhoneNumber.Length == 11))
				// TODO: The check above is incomplete! Replace it with a proper function which thoroughly validates the passed phone number!
				{
                    throw new ApiException(401, authorizatoinFailedMessage);
				}
                //var user = await dataContext.Users.Where(x => x.Id == payload.Data.Id).FirstOrDefaultAsync();
                var user = await userService.GetByIdAsync(payload.Data.Id);
                if (user == null)
                {
                    user = User.Create(payload.Data.PhoneNumber, UserType.Customer);
                    user.Id = payload.Data.Id;
                    await dbContext.SaveChangesAsync();
                }
				else
				{
                    if (!(user.MobileNumber.Substring(user.MobileNumber.Length - 10).Equals(payload.Data.PhoneNumber.Substring(payload.Data.PhoneNumber.Length - 10))))
					{
                        throw new Exception($"Inconsistency in user data between mahtakala database and auth database! User with id {user.Id} has two different phone numbers in mahtakala and auth! Look into it! It might be important, and, your own ass might be on the line! So, stay sharp, and keep your head in the game!");
					}
				}
				if (user.Type != Entities.UserType.Customer)
				{
					if (!await userService.IsValidToken(user, token, Util.GetIpAddress(context)))
						return;
				}

				context.Items["User"] = user;
            }
            catch (Exception e)
			{

			}
			//return;
   //         try
   //         {
   //             var tokenHandler = new JwtSecurityTokenHandler();
   //             var key = Encoding.ASCII.GetBytes(jwtSecret);
   //             tokenHandler.ValidateToken(token, new TokenValidationParameters
   //             {
   //                 ValidateIssuerSigningKey = true,
   //                 IssuerSigningKey = new SymmetricSecurityKey(key),
   //                 ValidateIssuer = false,
   //                 ValidateAudience = false,
   //             }, out SecurityToken validatedToken);

   //             var jwtToken = (JwtSecurityToken)validatedToken;
   //             var userId = long.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
   //             var user = userService.GetById(userId);
   //             if(user.Type != Entities.UserType.Customer)
   //             {
   //                 if (!await userService.IsValidToken(user, token, Util.GetIpAddress(context)))
   //                     return;
   //             }

   //             context.Items["User"] = user;

   //             //context.AuthenticateAsync();
   //             //context.User = new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity[]
   //             //{
   //             //    new ClaimsIdentity(jwtToken.Claims),
   //             //});

   //         }
   //         catch
   //         {
   //             // do nothing if jwt validation fails
   //             // user is not attached to context so request won't have access to secure routes
   //         }
        }
    }

    public class JwtPayloadModel
    {
		// {"data":{"id":1000002,"phone_number":"09357597227"},"iat":1615767569,"exp":1615853969,"iss":"auth.mahtakala.ir","sub":"09357597227"}
		public JwtPayloadData Data { get; set; }
		public string iat { get; set; }
		public string exp { get; set; }
		public string iss { get; set; }
		public string sub { get; set; }
	}

    public class JwtPayloadData
	{
		public long Id { get; set; }
		public string phone_number { get; set; }
		public string PhoneNumber 
        { 
            get { return phone_number; }
			set { phone_number = value; }
        }
	}
}