using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using MahtaKala.Models;
using MahtaKala.GeneralServices;
using MahtaKala.SharedServices;

namespace MahtaKala.Controllers
{
    [Authorize(UserType.Admin)]
    [Route("~/staff/settings")]
    public class SettingsController : SiteControllerBase<SettingsController>
    {
        private readonly SettingsService settings;

        public SettingsController(DataContext dataContext, ILogger<SettingsController> logger,
            SettingsService settings) : base(dataContext, logger)
        {
            this.settings = settings;
        }

        public ActionResult Index()
        {
            List<SettingModel> l = new List<SettingModel>();
            var props = typeof(SettingsService).GetProperties(System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public).Where(p => p.CanRead && p.CanWrite);
            if (Request.Method == "POST")
            {
                foreach (var p in props)
                {
                    if (p.PropertyType == typeof(bool))
                        p.SetValue(settings, Request.Form[p.Name] == "true");
                    else
                        p.SetValue(settings, settings.GetValue(Request.Form[p.Name], p.PropertyType));
                }
                ViewBag.Message = "تغییرات ذخیره شد";
            }
            foreach (var p in props)
            {
                var v = p.GetValue(settings);
                var attrs = p.GetCustomAttributes(false);
                l.Add(new SettingModel()
                {
                    Name = p.Name,
                    Title = TranslateExtentions.GetTitle(p),
                    Value = v == null ? "" : v.ToString(),
                    Type = ((DataTypeAttribute)attrs.FirstOrDefault(a => a.GetType() == typeof(DataTypeAttribute)))?.DataType ?? DataType.Text
                });
            }


            return View("Settings", l);
        }
        //[HttpPost]
        //public ActionResult Index()
        //{
        //    EDMMI.Entities.DynamicSettings.BlockVelocityMinAlarmValueForEmail = BlockVelocityMinAlarmValue;
        //    return View("Settings");
        //}

    }
}