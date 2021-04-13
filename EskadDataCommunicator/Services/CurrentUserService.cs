using MahtaKala.Entities;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService(IHttpContextAccessor contextAccessor)
        {
            HttpContext = contextAccessor.HttpContext;
            this.contextAccessor = contextAccessor;
        }
        const string AnonymousSessionCookieName = "AnonymousSession";
        private readonly IHttpContextAccessor contextAccessor;

        HttpContext HttpContext { get; }
        public User User
        {
            get { return (User)HttpContext.Items["User"]; }
        }

        private string GetCartCookie()
        {
            string cookieValueFromContext = contextAccessor.HttpContext.Request.Cookies[AnonymousSessionCookieName];
            if (cookieValueFromContext == null)
            {
                string guid = Guid.NewGuid().ToString();
                CookieOptions option = new CookieOptions();
                option.Expires = DateTime.Now.AddYears(1);
                contextAccessor.HttpContext.Response.Cookies.Append(AnonymousSessionCookieName, guid, option);
                return guid;
            }
            else
            {
                return cookieValueFromContext;
            }
        }

        public void RemoveCartCookie()
        {
            contextAccessor.HttpContext.Response.Cookies.Delete(AnonymousSessionCookieName);
        }

        public string AnonymousSessionId
        {
            get
            {
                return GetCartCookie();
            }
        }
    }
}
