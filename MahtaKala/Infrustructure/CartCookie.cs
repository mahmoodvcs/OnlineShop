using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure
{
    public class CartCookie
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CartCookie(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }
        public string GetCartCookie()
        {
            string cookieValueFromContext = _httpContextAccessor.HttpContext.Request.Cookies["CartsId"];
            if (cookieValueFromContext != null)
            {
                string guid = Guid.NewGuid().ToString();
                CookieOptions option = new CookieOptions();
                option.Expires = DateTime.Now.AddYears(1);
                _httpContextAccessor.HttpContext.Response.Cookies.Append("CartsId", guid, option);
                return guid;
            }
            else
            {
                return cookieValueFromContext;
            }
        }

        public void RemoveCartCookie()
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("CartsId");
        }
    }
}
