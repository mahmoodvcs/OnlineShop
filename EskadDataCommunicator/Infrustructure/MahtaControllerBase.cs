using MahtaKala.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure
{
    /// <summary>
    /// This is the base for all controllers in the application.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MahtaControllerBase<T> : Controller
        where T : MahtaControllerBase<T>
    {
        public MahtaControllerBase(DataContext dataContext, ILogger<T> logger)
        {
            db = dataContext;
            this.logger = logger;
        }
        protected readonly DataContext db;
        protected readonly ILogger<T> logger;

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

        [NonAction]
        public IActionResult KendoJson(object data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            return Content(json, "application/json");
        }

        [NonAction]
        public JsonResult Success(string message = null)
        {
            return Json(new { ok = true, success = true, message });
        }


    }
}
