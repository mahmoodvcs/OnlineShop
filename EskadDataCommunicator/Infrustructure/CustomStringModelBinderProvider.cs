using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure
{
    public class CustomStringModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.IsComplexType)
            {
                return null;
            }

            var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            var fallbackBinder = new SimpleTypeModelBinder(context.Metadata.ModelType, loggerFactory);
            if (context.Metadata.ModelType == typeof(string))
            {
                return new CustomStringModelBinder(fallbackBinder);
            }
            return fallbackBinder;
        }
    }
}
