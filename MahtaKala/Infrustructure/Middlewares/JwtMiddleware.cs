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
                var payloadLowerCase = payloadString.ToLower();
                if (payloadLowerCase.Contains("given_name") && payloadLowerCase.Contains("role") && payloadLowerCase.Contains("nbf"))
                {
                    // This means the token passed here is NOT generated by the "auth" micro-service, which in turn means that, this is called by the staff panel service, which is managed 
                    //  by the previous regime! Long live our king, king Kong! king Kong is our undesputed leader, from whom we get our direction in life! Our way of living is dictated 
                    //  by the benevolent king, Mr Kong, the liberator of the baboons, the haver of the red ass, and the bringer of bananas! Our beloved king Kong is the best king ever!
                    await attachUserToContextForStaffPanel(context, userService, token);
                    return;
                }
                var signature = tokenPartsBase64[2];

                var hasher = new HMACSHA256(Encoding.UTF8.GetBytes(jwtSecret));
                var generatedSignatureBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(tokenPartsBase64[0] + "." + tokenPartsBase64[1]));
                var generatedSignature = Base64UrlTextEncoder.Encode(generatedSignatureBytes);
                if (!signature.Equals(generatedSignature))
                {
                    throw new ApiException(401, authorizatoinFailedMessage);
                    //var payload = JsonSerializer.Deserialize(payloadString);
                }
                var jwtToken = new JwtSecurityToken();
                //{"data":{"id":1000002,"phone_number":"09357597227"},"iat":1615767569,"exp":1615853969,"iss":"auth.mahtakala.ir","sub":"09357597227"}
                var payload = JsonSerializer.Deserialize<JwtPayloadModel>(payloadString, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                if (payload.Data.Id <= 0)
                {
                    // TODO: This here should be logged, that the Id value passed through token is invalid - or missing! Who knows!
                    throw new ApiException(401, authorizatoinFailedMessage);
                }
                if (string.IsNullOrWhiteSpace(payload.Data.PhoneNumber))
                {
                    throw new ApiException(401, authorizatoinFailedMessage);
                }
                payload.Data.PhoneNumber = payload.Data.PhoneNumber.Trim();
                if (payload.Data.PhoneNumber.Length < 10)
                {
                    throw new ApiException(401, "Invalid phone number! Let's try that again, shall we?!");
                }

                var phoneNumberLastTenDigits = payload.Data.PhoneNumber.Substring(payload.Data.PhoneNumber.Length - 10);
                //            if (!(payload.Data.PhoneNumber.StartsWith("09") && payload.Data.PhoneNumber.Length == 11))
                //// TODO: The check above is incomplete! Replace it with a proper function which thoroughly validates the passed phone number!
                //{
                //                throw new ApiException(401, authorizatoinFailedMessage);
                //}
                //var user = await dataContext.Users.Where(x => x.Id == payload.Data.Id).FirstOrDefaultAsync();
                var user = await userService.GetByIdAsync(payload.Data.Id);
                if (user == null)
                {
                    user = User.Create(payload.Data.PhoneNumber, UserType.Customer);
                    user.Id = payload.Data.Id;
                    dbContext.Users.Add(user);
                    await dbContext.SaveChangesAsync();
                }
                else
                {

                    if (string.IsNullOrWhiteSpace(user.MobileNumber) || !user.MobileNumber.EndsWith(phoneNumberLastTenDigits))
                    //if (!(user.MobileNumber.Substring(user.MobileNumber.Length - 10).Equals(payload.Data.PhoneNumber.Substring(payload.Data.PhoneNumber.Length - 10))))
                    {
                        throw new Exception($"Inconsistency in user data between mahtakala database and auth database! User with id {user.Id} has two different phone numbers in mahtakala and auth! Look into it! It might be important, and, your own ass might be on the line! So, stay sharp, and keep your head in the game!");
                    }
                }
                /////////////// WARNING TO THE DEVELOPER ////////////////////
                /// The following "if" block has been commented out, because of changing the authentication method for normal users (customers, and NOT staff or admin), and they will use the auth-microservice
                /// instead. Now, because there's a chance that staff users will use the new method, too! Removing the following check (i.e. userService.IsValidToken) will enable them to do so, but, 
                /// it will also enable them to login from different machines at the same time; a possibility that were deliberately taken away! Although, the "IsValidToken" check won't work like this,
                /// and, if added here, will always deny access to anyone! To make it work as it's supposed to, one has to do a lot more (e.g. recoding the user's ip address, first! Then, checking if they still have
                /// the same address!).
                /////////////////////////////////////////////////////////////
                //if (user.Type != Entities.UserType.Customer)
                //{
                //    if (!await userService.IsValidToken(user, token, Util.GetIpAddress(context)))
                //        return;
                //}

                context.Items["User"] = user;
            }
            catch (Exception e)
            {

            }
        }

        private async Task attachUserToContextForStaffPanel(HttpContext context, IUserService userService, string token)
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
				if (user.Type != Entities.UserType.Customer)
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

    public class JwtPayloadModel
    {
		// {"data":{"id":1000002,"phone_number":"09357597227"},"iat":1615767569,"exp":1615853969,"iss":"auth.mahtakala.ir","sub":"09357597227"}
		public JwtPayloadData Data { get; set; }
		public long iat { get; set; }
		public long exp { get; set; }
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