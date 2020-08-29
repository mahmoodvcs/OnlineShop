using MahtaKala.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.ViewComponents
{
    public class UserInfoViewComponent : ViewComponent
    {
        public UserInfoViewComponent()
        {

        }
        private User user;
        public new User User
        {
            get
            {
                if (user == null)
                {
                    user = (User)HttpContext.Items["User"];
                }
                return user;
            }
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            bool isLogin = false;
            string fullName = string.Empty;
            string url = "~/account/index";
            if (User != null)
            {
                isLogin = true;
                fullName = user.FullName();
                if (User.Type==UserType.Customer)
                {
                    url = "#";
                }
                else
                {
                    url = "~/staff/index";
                }
            }
            var vm = (isLogin, fullName, url);
            return View(vm);
        }
    }
}
