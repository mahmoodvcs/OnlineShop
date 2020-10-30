using Kendo.Mvc.UI;
using MahtaKala.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.Security
{
    public class StaffMenuManager
    {
        public static readonly StaffMenu[] MenuItems = new StaffMenu[]
        {
            new StaffMenuItem("داشبورد", "~/Staff", "graph"),
            new StaffMenuCategory("اطلاعات پارامتریک", "folder-2", new StaffMenu[]
                {
                    new StaffMenuItem("کاربران", "~/Staff/UserList", "users"),
                    new StaffMenuItem("فروشنده ها", "~/Staff/Sellers", "coins"),
                    new StaffMenuItem("استانها", "~/Staff/ProvinceList", "placeholder-1"),
                    new StaffMenuItem("شهرها", "~/Staff/CityList", "placeholder"),
                    new StaffMenuItem("برندها", "~/Staff/BrandList", "interface-9"),
                    new StaffMenuItem("تامین کننده ها", "~/Staff/SupplierList", "interface-9"),
                    new StaffMenuItem("تگ ها", "~/Staff/Tags", "coins"),
                    new StaffMenuItem("به روز رسانی قیمت کالاها", "~/Staff/ImportProductPrices", "edit-1"),
                }),
            new StaffMenuCategory("کاتالوگ", "list-2", new StaffMenu[]
                {
                    new StaffMenuItem("دسته بندی کالا", "~/Staff/CategoryList", "open-box"),
                    new StaffMenuItem("کالا و خدمات", "~/Staff/ProductList", "gift"),
                }),
            new StaffMenuItem("گزارش خریدها", "~/Staff/BuyHistory", "bank",UserType.Delivery, UserType.Staff),
            new StaffMenuItem("اقلام فروخته شده", "~/Staff/Orders/Items", "bank",UserType.Seller, UserType.Staff),
            new StaffMenuItem("تنظیمات سیستم", "~/Staff/Settings", "gear"),
            new StaffMenuItem("تغییرات نسخه", "~/Staff/ReleaseNotes", "gear"),
        };

        public static IEnumerable<StaffMenu> GetItems(HttpContext context)
        {
            var user = (User)context.Items["User"];
            if (user == null)
                return Enumerable.Empty<StaffMenu>();
            return GetItems(user, MenuItems);
        }
        public static IEnumerable<StaffMenu> GetItems(User user, IEnumerable<StaffMenu> allItems)
        {
            var items = new List<StaffMenu>();

            foreach (var item in allItems)
            {
                if (item is StaffMenuCategory)
                {
                    var cat = new StaffMenuCategory(item.Name, item.Icon, GetItems(user, ((StaffMenuCategory)item).Children).ToArray());
                    if (cat.Children.Any())
                        items.Add(cat);
                }
                else if (user.Type == UserType.Admin || ((StaffMenuItem)item).Users.Contains(user.Type))
                    items.Add(item);
            }

            return items;
        }
    }

    public class StaffMenuCategory : StaffMenu
    {
        public StaffMenuCategory(string name, string icon, StaffMenu[] children) : base(name, icon)
        {
            Children = children;
        }

        public StaffMenu[] Children { get; }
    }

    public abstract class StaffMenu
    {
        protected StaffMenu(string name, string icon)
        {
            Name = name;
            Icon = icon;
        }

        public string Name { get; }
        public string Icon { get; }

    }
    public class StaffMenuItem : StaffMenu
    {
        public StaffMenuItem(string name, string url, string icon, params UserType[] userTypes)
            : base(name, icon)
        {
            Url = url;
            Users = userTypes;
        }

        public string Url { get; }
        public UserType[] Users { get; }
    }

}
