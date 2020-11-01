using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MahtaKala.Entities;
using MahtaKala.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Z.EntityFramework.Extensions;

namespace MahtaKala.Controllers
{
    public class QueryDto
    {
		public long R { get; set; }
		public long Product_Id { get; set; }
		public long id { get; set; }
		public long Category_Id { get; set; }
		public decimal Discount_Price { get; set; }
		public decimal Price { get; set; }
		public string Title { get; set; }
		public string Thubmnail { get; set; }
		public int Status { get; set; }
		public decimal Discount { get; set; }
	}
    [ApiController()]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1")]
    [ActionFilter.Authorize()]
    public class AllProductsController : ApiControllerBase<AllProductsController>
    {
        private readonly string connectionString;
        private readonly IProductImageService _productImageService;
        public AllProductsController(DataContext context
            , ILogger<AllProductsController> logger
            , IConfiguration configuration
            , IProductImageService productImageService) : base(context, logger)
        {
            //Npgsql
            connectionString = configuration.GetSection("ConnectionStrings")["DataContextPG"];
            this._productImageService = productImageService;
        }

		[HttpGet]
		public IActionResult TopProducts(long input)
		{
            //string query = "SELECT * FROM (SELECT ROW_NUMBER() OVER (PARTITION BY category_id) AS r, pc.*, pp.discount_price, p.title, p.thubmnail, (100 - (discount_price/price)*100)::INT AS discount FROM product_category pc JOIN product_prices pp ON pp.product_id = pc.product_id JOIN products p ON p.id = pp.product_id WHERE price <> 0 AND p.status = 0 AND published = 't' ORDER BY discount DESC) x WHERE x.r <= @inputParam;";
            var query = "SELECT * FROM (SELECT ROW_NUMBER() OVER (PARTITION BY parent_id) AS r, pc.*, pp.discount_price, pp.price, p.title, p.thubmnail, p.status, (100 - (discount_price/price)*100)::INT AS discount FROM categories c JOIN product_category  pc ON c.id = pc.category_id JOIN product_prices pp ON pp.product_id = pc.product_id JOIN products p ON p.id = pp.product_id WHERE price <> 0 AND p.published = 't' AND c.published = 't' ORDER BY p.status ASC, discount DESC) x WHERE x.r <= @inputParam;";
            //var connectionString = "Host=localhost;Database=mahtakala;Username=postgres;Password=1";
            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                var resultSet = connection.Query<QueryDto>(query, new { inputParam = input });
                foreach (var item in resultSet)
                {
                    item.Thubmnail = _productImageService.GetImageUrl(item.Product_Id, item.Thubmnail);
                    item.id = item.Product_Id;
                }
                return Json(resultSet);
            }
        }
    }
}
