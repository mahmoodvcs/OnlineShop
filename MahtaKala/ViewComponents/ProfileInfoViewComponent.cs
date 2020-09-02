using MahtaKala.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.ViewComponents
{
    public class ProfileInfoViewComponent : ViewComponent
    {
        public ProfileInfoViewComponent()
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
            string fullName = User.FullName();
            string userName = User.Username;
            var vm = (fullName, userName);
            return View(vm);
        }
    }
}
