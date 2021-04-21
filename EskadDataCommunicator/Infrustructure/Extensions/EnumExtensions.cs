using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.Extensions
{
	public static class EnumExtensions
	{
        public static string GetEnumDescriptionAttribute<T>(this T value) where T : struct
        {
            // The type of the enum, it will be reused.
            Type type = typeof(T);

            // If T is not an enum, get out.
            if (!type.IsEnum)
                throw new InvalidOperationException(
                    "The type parameter T must be an enum type.");

            // If the value isn't defined throw an exception.
            if (!Enum.IsDefined(type, value))
                throw new InvalidEnumArgumentException(
                    "value", Convert.ToInt32(value), type);

            // Get the static field for the value.
            FieldInfo fi = type.GetField(value.ToString(),
                BindingFlags.Static | BindingFlags.Public);

            // Get the description attribute, if there is one.
            var descriptionAttribute = 
                fi.GetCustomAttributes(typeof(DescriptionAttribute), true).
                Cast<DescriptionAttribute>().SingleOrDefault();
            if (descriptionAttribute != null)
                return descriptionAttribute.Description;
            return "";
        }
    }
}
