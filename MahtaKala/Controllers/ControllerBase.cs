using MahtaKala.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Controllers
{
    public class ControllerBase : Controller
    {
        private User user;
        public new User User
        {
            get
            {
                if(user == null)
                {
                    user = (User)HttpContext.Items["User"];
                }
                return user;
            }
        }
        long userId;
        public long UserId
        {
            get
            {
                if (userId == 0 && HttpContext.Items["User"] != null)
                {
                    userId = User.Id;
                }
                return userId;
            }
        }


    }
}
