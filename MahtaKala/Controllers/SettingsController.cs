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
using System.ComponentModel;

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
                    Value = v,
                    Type = ((DataTypeAttribute)attrs.FirstOrDefault(a => a.GetType() == typeof(DataTypeAttribute)))?.DataType ?? GetDataType(p.PropertyType),
                    TypeCode = Type.GetTypeCode(p.PropertyType),
                    Category = ((CategoryAttribute)attrs.FirstOrDefault(a => a.GetType() == typeof(CategoryAttribute)))?.Category
                });
            }

            return View("~/Views/Staff/Settings/Settings.cshtml", l);
        }

        private DataType GetDataType(Type propertyType)
        {
            switch (Type.GetTypeCode(propertyType))
            {
                case TypeCode.Boolean:
                    return DataType.Custom;
                case TypeCode.String:
                    return DataType.Text;
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Decimal:
                case TypeCode.Double:
                    return DataType.Text;
            }
            return DataType.Text;
        }
    }
}