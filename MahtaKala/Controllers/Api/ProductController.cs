using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.ActionFilter;
using MahtaKala.Controllers.Api;
using MahtaKala.Entities;
using MahtaKala.Entities.Extentions;
using MahtaKala.GeneralServices.Delivery;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Models;
using MahtaKala.Models.ProductModels;
using MahtaKala.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Dissolve;
using AuthorizeAttribute = MahtaKala.ActionFilter.AuthorizeAttribute;

namespace MahtaKala.Controllers
{
    [ApiController()]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1")]
    [ActionFilter.Authorize]
    public class ProductController : ApiControllerBase<ProductController>
    {
        private readonly ICategoryImageService categoryImageService;
        private readonly IProductImageService imageService;
        private readonly CategoryService categoryService;
        private readonly ProductService productService;

        public ProductController(
            DataContext context,
            ILogger<ProductController> logger,
            ICategoryImageService categoryImageService,
            IProductImageService imageService,
            CategoryService categoryService,
            ProductService productService)
            : base(context, logger)
        {
            this.categoryImageService = categoryImageService;
            this.imageService = imageService;
            this.categoryService = categoryService;
            this.productService = productService;
        }


        /// <summary>
        /// Update the Existing Category or Create New One
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Success. The category was updated.</response>
        /// <response code="201">Success. Category was new.</response>
        [HttpPost]
        public async Task<IActionResult> Category([FromBody] UpdateCategoryRequest updateCategoryRequest)
        {
            Category category = null;
            bool newCategory = false;
            if (updateCategoryRequest.Id == 0)
            {
                newCategory = true;
                category = new Category();
                db.Categories.Add(category);
            }
            else
            {
                category = await db.Categories.FirstOrDefaultAsync(c => c.Id == updateCategoryRequest.Id);
                if (category == null)
                    throw new EntityNotFoundException<Category>(updateCategoryRequest.Id);
            }

            category.Title = updateCategoryRequest.Title;
            //TODO: Category Icon
            //if (!string.IsNullOrEmpty(updateCategoryRequest.Icon))
            //    category.Image = Convert.FromBase64String(updateCategoryRequest.Icon);
            category.ParentId = updateCategoryRequest.ParentId;

            await db.SaveChangesAsync();
            if (newCategory)
                return StatusCode(200);
            else
                return StatusCode(201);
        }
        /// <summary>
        /// Return the List of Categories with the given parent ID
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<CategoryModel>> Category([FromQuery] long? parent)
        {
            var data = await categoryService.Categories().Where(c => c.ParentId == parent)
                .OrderByDescending(a => a.Disabled ? 0 : 1).ThenBy(a => a.Order).ThenBy(x => x.Id).ToListAsync();
            foreach (var item in data)
            {
                item.Image = categoryImageService.GetImageUrl(item.Id, item.Image);
            }
            var list = new List<CategoryModel>();
            foreach (var item in data)
            {
                var cat = new CategoryModel
                {
                    Disabled = item.Disabled,
                    Id = item.Id,
                    Image = item.Image,
                    Order = item.Order,
                    ParentId = item.ParentId,
                    Published = item.Published,
                    Title = item.Title,
                };
                if (!string.IsNullOrEmpty(item.Color))
                {
                    var c = Util.ParseColor(item.Color);
                    cat.Color = new CategoryModel.ColorModel
                    {
                        A = c.A / 255f,
                        R = c.R,
                        G = c.G,
                        B = c.B,
                    };
                }
                list.Add(cat);
            }
            return list;
        }
        /// <summary>
        /// Removes the Category with the given ID
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Success. The category was Deleted.</response>
        [HttpDelete]
        [Authorize(UserType.Admin)]
        public async Task<IActionResult> DeleteCategory([FromQuery] long id)
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                throw new EntityNotFoundException<Category>(id);
            }
            //if (await db.Categories.AnyAsync(c => c.ParentId == model.Id))
            //{
            //    throw new Exception("Category Has Child.");
            //}
            db.Categories.Remove(category);
            await db.SaveChangesAsync();
            return StatusCode(200);
        }

        [HttpPost]
        [Authorize(UserType.Admin)]
        public async Task Product([FromBody] ProductUpdateModel productModel)
        {
            Product product;
            if (productModel.Id > 0)
            {
                product = db.Products.Find(productModel.Id);
                if (product == null)
                    throw new EntityNotFoundException<Product>(productModel.Id);
            }
            else
            {
                product = new Product();
            }

            if (db.Categories.Any(x => x.ParentId.HasValue && productModel.Categories.Contains(x.ParentId.Value)))
            {
                var childCategoryOfAParentInTheList = db.Categories.Where(x => x.ParentId.HasValue && productModel.Categories.Contains(x.ParentId.Value)).Include(x => x.Parent).First();
                throw new ApiException(400, 
                    string.Format("The category {0} is a parent category! Only non-parent (or leaf) categories are allowed to have a connected product!", 
                    childCategoryOfAParentInTheList.Parent.Title));
            }
            product.ProductCategories = productModel.Categories.Select(c => new ProductCategory
            {
                CategoryId = c
            }).ToList();
            product.BrandId = productModel.Brand_Id;
            product.Characteristics = productModel.Characteristics;
            product.Description = productModel.Description;
            product.Properties = productModel.Properties.ToList();
            product.Thubmnail = productModel.Thubmnail;
            product.Title = productModel.Title;
            await db.SaveChangesAsync();
        }

        //public async Task<string> ProductImage(long id)
        //{

        //}

        [HttpDelete]
        [Authorize(UserType.Admin)]
        public async Task<StatusCodeResult> DeleteProduct(long id)
        {
            var product = await db.Products.FirstOrDefaultAsync(c => c.Id == id);
            if (product == null)
            {
                throw new EntityNotFoundException<Category>(id);
            }
            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return StatusCode(200);
        }

        [HttpGet]
        public async Task<ProductModel> Product(long id)
        {
            var data = await productService.ProductsView()
                .Where(a => a.Id == id)
                .Select(a => new ProductModel
                {
                    Id = a.Id,
                    Brand_Id = a.BrandId,
                    Brand = a.Seller.Name,
                    Category_Id = a.ProductCategories.FirstOrDefault().CategoryId,
                    Category = a.ProductCategories.FirstOrDefault().Category.Title,
                    Description = a.Description,
                    Status = a.Status,
                    Title = a.Title,
                    Thubmnail = a.Thubmnail,
                    Characteristics = a.Characteristics,
                    PropertiesKeyValues = a.Properties,
                    ImageList = a.ImageList,
                    Price = a.Prices.FirstOrDefault().Price,
                    DiscountPrice = a.Prices.FirstOrDefault().DiscountPrice,
                    Prices = a.Prices,
                    Quantity = a.Quantities.First().Quantity
                }).ToListAsync();

            //Product.Properties must be Dictionary
            return data.Select(a => new ProductModel
            {
                Id = a.Id,
                Brand_Id = a.Brand_Id,
                Brand = a.Brand,
                Category_Id = a.Category_Id,
                Category = a.Category,
                Status = a.Status,
                Description = a.Description,
                Title = a.Title,
                Thubmnail = imageService.GetImageUrl(a.Id, a.Thubmnail),
                Characteristics = a.Characteristics,
                Properties = a.PropertiesKeyValues?.ToDictionary(a => a.Key, a => a.Value),
                ImageList = imageService.GetImageUrls(a.Id, a.ImageList),
                Price = a.Prices?.FirstOrDefault()?.Price,
                DiscountPrice = a.Prices?.FirstOrDefault()?.DiscountPrice,
                Prices = a.Prices
            }).FirstOrDefault();
        }

        [HttpGet]
        public async Task<List<ProductConciseModel>> Products([FromQuery] long? category, [FromQuery] int offset, [FromQuery] int page)
        {
            List<long> cids = new List<long>();
            if (category.HasValue)
                cids.Add(category.Value);
            return await GetProductsData(cids, offset, page);
        }

        [NonAction]
        public async Task<List<ProductConciseModel>> GetProductsData(IEnumerable<long> categoryIds, int offset, int page)
        {
            var query = productService.ProductsView(categoryIds: categoryIds?.ToArray());

            query = query.Skip(offset).Take(page);

            var data = await query.Select(prc =>
                new ProductConciseModel
                {
                    Id = prc.Id,
                    Brand = prc.Seller.Name,
                    Category = prc.ProductCategories.First().Category.Title,
                    Status = prc.Status,
                    Title = prc.Title,
                    Thubmnail = prc.Thubmnail,
                    Price = prc.Prices.FirstOrDefault().Price,
                    DiscountPrice = prc.Prices.FirstOrDefault().DiscountPrice,
                    Quantity = prc.Quantities.FirstOrDefault().Quantity
                }).ToListAsync();

            foreach (var p in data)
            {
                p.Thubmnail = imageService.GetImageUrl(p.Id, p.Thubmnail);
            }
            return data;
        }


        [HttpGet]
        public async Task<List<ProductConciseModel>> Search([FromQuery] string q, [FromQuery] int? offset, [FromQuery] int? page)
        {
            var products = productService.ProductsView().Where(p => p.Title.Contains(q));

            if (offset.HasValue)
            {
                products = products.Skip(offset.Value);
            }

            if (page.HasValue)
            {
                products = products.Take(page.Value);
            }

            return await products.Select(a => new ProductConciseModel
            {
                Id = a.Id,
                Brand = a.Seller.Name,
                Category = a.ProductCategories.FirstOrDefault().Category.Title,
                Title = a.Title,
                Thubmnail = imageService.GetImageUrl(a.Id, a.Thubmnail),
                Price = a.Prices.FirstOrDefault().Price,
                DiscountPrice = a.Prices.FirstOrDefault().DiscountPrice,
                Status = a.Status
            })
            .ToListAsync();
        }

        //[HttpGet]
        //public async Task<byte[]> Image(long pid, string iid, int s)
        //{
        //    var prod = db.Products.Where(p => p.Id == pid);
        //    if (iid.StartsWith("th"))
        //    {
        //        var th = prod.Select(p => p.Thubmnail).FirstOrDefault();
        //        if(th == iid)
        //        {
        //            return 
        //        }
        //    }
        //}

        /// <summary>
        /// Returns the products that must be displayed in the home page of the app. This will include promotions and ads.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<ProductConciseModel>> Home()
        {
            return await GetTopProducts();
        }

        /// <summary>
        /// Return the List of Categories with the given parent ID
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<CategoryWithProductsModel>> AllCategories([FromQuery] int numProducts = 0)
        {
            List<Category> categories = await categoryService.Categories().ToListAsync();
            List<CategoryWithProductsModel> result = new List<CategoryWithProductsModel>();
            CreateHierarchy(null, result, categories.Where(c => c.ParentId == null).ToList());
            if (numProducts > 0)
            {
                foreach (var c in result)
                {
                    c.Image = categoryImageService.GetImageUrl(c.Id, c.Image);
                    List<long> catagories = new List<long>();
                    catagories.Add(c.Id);
                    if (c.Children != null)
                        foreach (var item in c.Children)
                        {
                            catagories.Add(item.Id);
                        }
                    c.Products = await GetTopProducts(numProducts, catagories.ToArray());
                }
            }
            return result;
        }

        private async Task<List<ProductConciseModel>> GetTopProducts(long? category = null, int num = 10)
        {
            List<long> catagories = new List<long>();
            if (category != null)
                catagories.Add(category.Value);
            return await GetTopProducts(num, catagories.ToArray());
        }
        private async Task<List<ProductConciseModel>> GetTopProducts(int num = 10, params long[] categoryIds)
        {
            return await GetProductsData(categoryIds, 0, num);
        }

        private void CreateHierarchy(long? parentId, IList<CategoryWithProductsModel> result, IList<Category> categories)
        {
            if (categories == null)
                return;
            foreach (var c in categories)
            {
                var cp = new CategoryWithProductsModel
                {
                    Id = c.Id,
                    Image = categoryImageService.GetImageUrl(c.Id, c.Image),
                    ParentId = parentId,
                    Title = c.Title,
                    Disabled = c.Disabled
                };
                if (!string.IsNullOrEmpty(c.Color))
                {
                    var col = Util.ParseColor(c.Color);
                    cp.Color = new CategoryModel.ColorModel
                    {
                        A = col.A / 255f,
                        R = col.R,
                        G = col.G,
                        B = col.B,
                    };
                }

                result.Add(cp);
                cp.Children = new List<CategoryWithProductsModel>();
                CreateHierarchy(c.Id, cp.Children, c.Children);
            }
        }


    }
}