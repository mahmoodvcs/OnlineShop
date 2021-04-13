using MahtaKala.Entities;
using MahtaKala.Infrustructure;
using MahtaKala.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Controllers
{
    /// <summary>
    /// This is the base for all controllers that render HTML
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SiteControllerBase<T> : MahtaControllerBase<T>
        where T : SiteControllerBase<T>
    {
        public SiteControllerBase(DataContext dataContext, ILogger<T> logger) : base(dataContext, logger)
        {
        }

        public override ViewResult View()
        {
            ViewBag.User = User;
            return base.View();
        }

        public override ViewResult View(object model)
        {
            ViewBag.User = User;
            return base.View(model);
        }

        public override ViewResult View(string viewName)
        {
            ViewBag.User = User;
            return base.View(viewName);
        }
        public override ViewResult View(string viewName, object model)
        {
            ViewBag.User = User;
            return base.View(viewName, model);
        }


        [NonAction]
        public void ShowMessage(string msg, MessageType type = MessageType.Info, bool closable = true)
        {
            UIMessage m = new UIMessage()
            {
                Message = msg,
                Type = type,
                Closable = closable
            };
            //ViewBag.Message = m;
            TempData["Unicorn.Message"] = JsonConvert.SerializeObject(m);
        }

    }
}
