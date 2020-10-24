using MahtaKala.Messages;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure
{
    public static class HtmlHelpers
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


        public static IHtmlContent UIMessages(this IHtmlHelper html)
        {
            if (html.ViewContext.TempData["Unicorn.Message"] == null)
                return null;
            UIMessage msg = JsonConvert.DeserializeObject<UIMessage>(html.ViewContext.TempData["Unicorn.Message"] as string);
            if (msg == null)
                return null;
            TagBuilder div = new TagBuilder("div");
            div.AddCssClass("alert");
            div.AddCssClass("alert-" + msg.Type.ToString().ToLower());
            var s = "";
            if (msg.Closable)
            {
                div.AddCssClass("alert-dismissible");
                 s= @"<button type='button' class='close' data-dismiss='alert'>
            <span aria-hidden='true'>&times;</span><span class='sr-only'>Close</span>
        </button>";
            }
            div.Attributes["role"] = "alert";
            s += msg.Message;
            div.InnerHtml.SetHtmlContent(s);
            return div;
        }

    }
}
