using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure
{
    public static class KendoUIHelpers
    {
        public static IHtmlContent EnumCombo<EnumType>(this IHtmlHelper html, string name,
            EnumType? value = null, bool showEmptyValue = false, bool isGridAdditionalData = false)
            where EnumType : struct, IConvertible
        {
            var list = TranslateExtentions.GetTitles<EnumType>().Select(a => new SelectListItem
            {

                Value = Convert.ToInt32(a.Key).ToString(),
                Text = a.Value,
                Selected = Object.Equals(value, a.Key)
            }).ToList();
            if (showEmptyValue)
            {
                list.Insert(0, new SelectListItem("", ""));
            }
            return html.DropDownList(name, list, new { @class = "form-control " + (isGridAdditionalData? "additionalData" : ""),  });
        }
    }
}
