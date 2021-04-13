using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure
{
    public static class CustomStringModelBinderExtensions
    {
        public static MvcOptions UseCustomStringModelBinder(this MvcOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var simpleTypeModelBinder = options.ModelBinderProviders.FirstOrDefault(x => x.GetType() == typeof(SimpleTypeModelBinderProvider));
            if (simpleTypeModelBinder == null)
            {
                return options;
            }

            var simpleTypeModelBinderIndex = options.ModelBinderProviders.IndexOf(simpleTypeModelBinder);
            options.ModelBinderProviders.Insert(simpleTypeModelBinderIndex, new CustomStringModelBinderProvider());
            return options;
        }
    }
}
