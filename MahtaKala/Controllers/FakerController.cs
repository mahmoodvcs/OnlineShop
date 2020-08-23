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

        Faker faker = new Faker();
        Random random = new Random();
        public ActionResult Products(int num)
        {

            List<Brand> brands = new List<Brand>();
            for (int i = 0; i < num * 2; i++)
            {
                brands.Add(new Brand
                {
                    Name = faker.Company.CompanyName()
                });
            }

            foreach (var c in faker.Commerce.Categories(num))
            {
                ProductCategory cat1 = new ProductCategory()
                {
                    Title = c,
                    Image = GetPicture(200, c)
                };
                db.Categories.Add(cat1);
                foreach (var c2 in faker.Commerce.Categories(num))
                {
                    ProductCategory cat = new ProductCategory()
                    {
                        Title = c2,
                        Image = GetPicture(200, c2),
                        Parent = cat1
                    };
                    db.Categories.Add(cat);

                    for (int i = 0; i < 10; i++)
                    {
                        var p = new Product()
                        {
                            Title = faker.Commerce.ProductName(),
                            Category = cat,
                            Brand = brands[random.Next(brands.Count)],
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
                            p.ImageList.Add(faker.Image.LoremPixelUrl(cat.Title, 800, 600, true));
                        }
                        p.Properties = new Dictionary<string, string>();
                        foreach (var w in faker.Lorem.Words(random.Next(3, 10)))
                        {
                            if (!p.Properties.ContainsKey(w))
                                p.Properties.Add(w, faker.Lorem.Sentence());
                        }
                        p.Thubmnail = faker.Image.LoremPixelUrl(c, 400, 400, true);
                        db.Products.Add(p);
                    }
                }
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        private string GetPicture(int size, string c)
        {
            return $"http://via.placeholder.com/{size}x{size}/{faker.Random.Hexadecimal(6)}.png?text={c}";
        }
    }
}
