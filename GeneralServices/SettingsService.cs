using MahtaKala.Entities;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace MahtaKala.GeneralServices
{
    public class SettingsService
    {
        private readonly DataContext db;
        private readonly static MemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        public SettingsService(DataContext db)
        {
            this.db = db;
        }


        #region Properties

        [Display(Name = "هزینه ی ارسال کالا (ریال)")]
        [Category("سفارش")]
        public decimal DeliveryPrice
        {
            get { return GetProperty(120000m); }
            set { SetProperty(value); }
        }

        [Display(Name = "جلسه ی کاربر حساس به آدرس IP")]
        [Category("امنیت")]
        public bool UserSessionBasedOnIP
        {
            get { return GetProperty(true); }
            set { SetProperty(value); }
        }

        [Display(Name = "حداکثر تعداد جلسات فعال کاربر")]
        [Category("امنیت")]
        public int MaxNumberOfActiveUserSessions
        {
            get { return GetProperty(1); }
            set { SetProperty(value); }
        }

        //[Display(Name = "شماره تماس ارسال کننده کالا")]
        //public string DeliverySenderPhoneNumber
        //{
        //    get { return GetProperty(""); }
        //    set { SetProperty(value); }
        //}

        //[Display(Name = "آدرس مبدا ارسال کالا")]
        //public string DeliveryOriginAddress
        //{
        //    get { return GetProperty(""); }
        //    set { SetProperty(value); }
        //}

        //[Display(Name ="موقعیت مبدا ارسال کالا - طول جغرافیایی")]
        //public double DeliveryOriginLatitude
        //{
        //    get { return GetProperty(0d); }
        //    set { SetProperty(value); }
        //}

        //[Display(Name ="موقعیت مبدا ارسال کالا - عرض جغرافیایی")]
        //public double DeliveryOriginLongitude
        //{
        //    get { return GetProperty(0d); }
        //    set { SetProperty(value); }
        //}

        #endregion Properties

        #region Methods
        public void Set(string key, object value)
        {
            var d = db.DynamicSettings.Find(key);
            if (d == null)
            {
                d = new DynamicSetting()
                {
                    Key = key,
                    //Title = title,
                    Vallue = SerializeValue(value)
                };
                db.DynamicSettings.Add(d);
            }
            else
                d.Vallue = SerializeValue(value);
            db.SaveChanges();
            cache.Remove("Settings." + key);
        }
        public object Get(string key, object defaultValue = null)
        {
            object o = cache.Get("Settings." + key);
            if (o == null)
            {
                var d = db.DynamicSettings.Find(key);
                if (d == null)
                    return defaultValue;
                o = d.Vallue;
                //CacheItemPolicy p = new CacheItemPolicy();
                //p.SlidingExpiration = new TimeSpan(100, 0, 0);
                cache.Set("Settings." + key, o);
            }
            return o;
        }

        public T GetProperty<T>(T defaultValue, [CallerMemberName] string name = null)
        {
            object v = Get(name, defaultValue);
            return (T)(GetValue(v, typeof(T)) ?? defaultValue);
        }

        public void SetProperty(object o, [CallerMemberName] string name = null)
        {
            Set(name, o);
        }
        public object GetValue(object v, Type type)
        {
            if (v == null || v.GetType() == type)
                return v;
            return DeSerializeValue(v.ToString(), type);
        }
        private string SerializeValue(object value)
        {
            if (value == null)
                return "";
            if (value.GetType().IsValueType)
                return Convert.ToString(value);
            if (value is string)
                return (string)value;
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }
        private T DeSerializeValue<T>(string value)
        {
            return (T)DeSerializeValue(value, typeof(T));
        }
        private object DeSerializeValue(string value, Type t)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (t.IsValueType)
                {
                    return Activator.CreateInstance(t);
                }
                return null;
            }
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject(value, t);
            }
            catch
            {
                var converter = TypeDescriptor.GetConverter(t);
                return converter.CanConvertFrom(typeof(string)) ?
                    converter.ConvertFrom(null, CultureInfo.InvariantCulture, value) :
                    null;
            }
        }

    }
    #endregion Methods
}
