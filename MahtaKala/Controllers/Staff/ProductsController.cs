using MahtaKala.Entities;
using MahtaKala.Entities.Models;
using MahtaKala.GeneralServices;
using MahtaKala.Models.ProductModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Controllers.Staff
{
    [Route("~/staff/products/[action]")]
    public class ProductsController : SiteControllerBase<ProductsController>
    {
        private readonly ImportService importService;

        public ProductsController(DataContext dataContext, ILogger<ProductsController> logger, ImportService importService) : base(dataContext, logger)
        {
            this.importService = importService;
        }


        public async Task<ActionResult> Import()

        {
            var file = Request.Form.Files.FirstOrDefault();
            if(file == null)
            {
                ShowMessage("فایل ارسال نشده است", Messages.MessageType.Error);
                return Redirect("~/Staff/ProductList");
            }
            try
            {
                var num = await importService.ImportProductsInfo(file.OpenReadStream());
                ShowMessage($"تعداد {num} کالا بروز رسانی شد", Messages.MessageType.Success);

                return Redirect("~/Staff/ProductList");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, Messages.MessageType.Error);
                logger.LogError(ex, "");
                return Redirect("~/Staff/ProductList");
            }
        }

        //public async Task<ActionResult> Export(ExportProductsModel model)
        //{
        //    var query = db.Products.AsQueryable();
        //    if (base.User.Type == UserType.Seller)
        //    {
        //        var sid = await db.Sellers.Where(a => a.UserId == UserId).Select(a => a.Id).FirstOrDefaultAsync();

        //        query = db.Products.Where(p => p.SellerId == sid).AsQueryable();
        //    }

        //    query = model.Filter(query);
        //    //FlexTextFilter<Product>(query, p => p.Title, nameFilter);
        //    var data = await query.Project();

        //    var stream = ExportService.ExportQuery<ProductListModel>(model.Columns);
        //    return File(stream,"application/excel", "products.xlsx");
        //}
    }

    //public class ExportProductsModel : ProductFiltertModel
    //{
    //    public List<string> Columns { get; set; }
    //}

}
