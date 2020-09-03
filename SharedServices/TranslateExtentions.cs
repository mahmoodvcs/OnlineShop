using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace MahtaKala.SharedServices
{
    public static class TranslateExtentions
    {
        public static string GetTitle<EnumType>() where EnumType : struct, IConvertible
        {
            var t = typeof(EnumType);
            return GetTitle(t, t.Name);
        }
        public static string GetTitle<EnumType>(EnumType member) where EnumType : struct, IConvertible
        {
            return GetTitle<EnumType>(member.ToString());
        }
        public static string GetTitle(Type enumType, string memberName)
        {
            return GetTitle(enumType.GetField(memberName, BindingFlags.Public | BindingFlags.Static), memberName, enumType);
        }
        public static string GetTitle(Type enumType, object memberValue)
        {
            var name = Enum.GetName(enumType, memberValue);
            return GetTitle(enumType.GetField(name, BindingFlags.Public | BindingFlags.Static), name, enumType);
        }
        public static string GetTitle<EnumType>(string memberName) where EnumType : struct, IConvertible
        {
            var t = typeof(EnumType);
            return GetTitle(t, memberName);
        }
        public static string GetTitle(Type type)
        {
            return GetTitle((ICustomAttributeProvider)type, type.Name, null);
        }
        public static string GetTitle(PropertyInfo typeMember)
        {
            return GetTitle((ICustomAttributeProvider)typeMember, null, null);
        }
        public static string GetTitle(ICustomAttributeProvider typeMember, string name = null, Type containingType = null)
        {
            object[] attrs = typeMember.GetCustomAttributes(typeof(DisplayAttribute), false);
            string title = null;
            if (attrs.Length > 0)
                title = ((DisplayAttribute)attrs[0]).Name;
            else
                title = name;
            return title;
        }
    }
}
