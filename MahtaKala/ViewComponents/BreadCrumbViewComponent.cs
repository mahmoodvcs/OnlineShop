using MahtaKala.Entities;
using MahtaKala.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.ViewComponents
{

    public class BreadCrumbViewComponent : ViewComponent
    {
        private DataContext _db;
        public BreadCrumbViewComponent(DataContext db)
        {
            _db = db;
        }

        private void GetGroup(long id, ref List<IdValModel> lst)
        {
            var p = _db.Categories.FirstOrDefault(a => a.Id == id);
            if (p != null)
            {
                IdValModel v = new IdValModel();
                v.Id = p.Id;
                v.Title = p.Title;
                lst.Add(v);
                if (p.ParentId.HasValue)
                    GetGroup(p.ParentId.Value, ref lst);
            }
        }


        public async Task<IViewComponentResult> InvokeAsync(long categoryId)
        {
            List<IdValModel> lst = new List<IdValModel>();
            GetGroup(categoryId, ref lst);
            lst.Reverse();
            return View(lst);
        }
    }
}
