using MahtaKala.Infrustructure.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure
{
    public class CustomStringModelBinder : IModelBinder
    {
        private readonly IModelBinder _fallbackBinder;
        public CustomStringModelBinder(IModelBinder fallbackBinder)
        {
            if (fallbackBinder == null)
            {
                throw new ArgumentNullException(nameof(fallbackBinder));
            }
            _fallbackBinder = fallbackBinder;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

                var valueAsString = valueProviderResult.FirstValue;
                if (string.IsNullOrWhiteSpace(valueAsString))
                {
                    return _fallbackBinder.BindModelAsync(bindingContext);
                }

                var model = valueAsString.Replace((char)1610, (char)1740).Replace((char)1603, (char)1705);
                model = model.ToEnglishNumber();
                bindingContext.Result = ModelBindingResult.Success(model);
                return Task.CompletedTask;
            }

            return _fallbackBinder.BindModelAsync(bindingContext);
        }
    }
}
