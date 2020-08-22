using Bogus;
using MahtaKala.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MahtaKala.Controllers
{
    public class FakerController : ApiControllerBase<FakerController>
    {
        public FakerController(DataContext dataContext, ILogger<FakerController> logger) : base(dataContext, logger)
        {
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Products(int num)
        {
            var faker = new Faker();
            var random = new Random();
            WebClient cli = new WebClient();
            foreach (var c in faker.Commerce.Categories(num))
            {
                ProductCategory cat = new ProductCategory()
                {
                    Title = c,
                    Image = faker.Image.LoremPixelUrl(c, 200, 200)
                };
                db.Categories.Add(cat);

                for (int i = 0; i < 10; i++)
                {
                    var p = new Product()
                    {
                        Title = faker.Commerce.ProductName(),
                        Category = cat,
                        Brand = new Brand()
                        {
                            Name = faker.Company.CompanyName()
                        },
                        Characteristics = new List<Characteristic>()
                        {
                            new Characteristic()
                            {
                                Name = "رنگ",
                                Values =new List<string>
                                {
                                    faker.Commerce.Color(),
                                    faker.Commerce.Color(),
                                }
                            },
                            new Characteristic
                            {
                                Name = "جنس",
                                Values =new List<string>
                                {
                                    faker.Commerce.ProductMaterial(),
                                    faker.Commerce.ProductMaterial()
                                }
                            }
                        },
                        Description = faker.Commerce.ProductDescription(),
                    };
                    p.ImageList = new List<string>();
                    for (int j = 0; j < random.Next(2, 5); j++)
                    {
                        p.ImageList.Add(faker.Image.LoremPixelUrl(c, 800, 600, true));
                    }
                    p.Properties = new Dictionary<string, string>();
                    foreach (var w in faker.Lorem.Words(random.Next(3, 10)))
                    {
                        if (!p.Properties.ContainsKey(w))
                            p.Properties.Add(w, faker.Lorem.Sentence());
                    }

                    db.Products.Add(p);
                }

            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
