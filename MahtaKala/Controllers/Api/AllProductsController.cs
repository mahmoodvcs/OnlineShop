using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MahtaKala.Entities;
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
		public long Category_Id { get; set; }
		public decimal Discount_Price { get; set; }
		public string Title { get; set; }
		public string Thumbnail { get; set; }
		public decimal Discount { get; set; }
	}
    [ApiController()]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1")]
    [ActionFilter.Authorize()]
    public class AllProductsController : ApiControllerBase<AllProductsController>
    {
        private readonly string connectionString;
        public AllProductsController(DataContext context, ILogger<AllProductsController> logger, IConfiguration configuration) : base(context, logger)
        {
            //Npgsql
            connectionString = configuration.GetSection("ConnectionStrings")["DataContextPG"];
        }

		[HttpGet]
		public IActionResult TopProducts(long input)
		{
            string query = "SELECT * FROM (SELECT ROW_NUMBER() OVER (PARTITION BY category_id) AS r, pc.*, pp.discount_price, p.title, p.thubmnail, (100 - (discount_price/price)*100)::INT AS discount FROM product_category pc JOIN product_prices pp ON pp.product_id = pc.product_id JOIN products p ON p.id = pp.product_id WHERE price <> 0 AND p.status = 0 AND published = 't' ORDER BY discount DESC) x WHERE x.r <= @inputParam;";
            //var connectionString = "Host=localhost;Database=mahtakala;Username=postgres;Password=1";
            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                var resultSet = connection.Query<QueryDto>(query, new { inputParam = input });
                return Json(resultSet);
            }
        }
    }
}
