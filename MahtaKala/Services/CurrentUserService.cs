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
        }
        HttpContext HttpContext { get; }
        public User User
        {
            get { return (User)HttpContext.Items["User"]; }
        } 
    }
}
