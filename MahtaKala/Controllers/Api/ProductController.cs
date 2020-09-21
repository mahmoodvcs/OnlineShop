﻿using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.ActionFilter;
using MahtaKala.Controllers.Api;
using MahtaKala.Entities;
using MahtaKala.Entities.Extentions;
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
        public async Task<List<Category>> Category([FromQuery] long? parent)
        {
            var data = await categoryService.Categories().Where(c => c.ParentId == parent).ToListAsync();
            foreach (var item in data)
            {
                item.Image = categoryImageService.GetImageUrl(item.Id, item.Image);
            }
            return data;
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
        public async Task Product([FromBody] ProductUpdateModel productMode)
        {
            Product product;
            if (productMode.Id > 0)
            {
                product = db.Products.Find(productMode.Id);
                if (product == null)
                    throw new EntityNotFoundException<Product>(productMode.Id);
            }
            else
            {
                product = new Product();
            }
            product.BrandId = productMode.Brand_Id;
            product.ProductCategories = productMode.Categories.Select(c => new ProductCategory
            {
                CategoryId = c
            }).ToList();
            product.Characteristics = productMode.Characteristics;
            product.Description = productMode.Description;
            product.Properties = productMode.Properties.ToList();
            product.Thubmnail = productMode.Thubmnail;
            product.Title = productMode.Title;
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
            var data = await db.Products
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
                    Prices = a.Prices
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
            var categories = categoryService.Categories().AsQueryable();

            if (categoryIds.Count() > 0)
                categories = categories.Where(c => categoryIds.Contains(c.Id));

            var query = from cat in categories
                        from prc in cat.ProductCategories.Where(c => c.Product.Published)
                        orderby prc.ProductId
                        select new ProductConciseModel
                        {
                            Id = prc.Product.Id,
                            Brand = prc.Product.Seller.Name,
                            Category = cat.Title,
                            Status = prc.Product.Status,
                            Title = prc.Product.Title,
                            Thubmnail = prc.Product.Thubmnail,
                            Price = prc.Product.Prices.FirstOrDefault().Price,
                            DiscountPrice = prc.Product.Prices.FirstOrDefault().DiscountPrice
                        };

            var data = await query.Skip(offset).Take(page).ToListAsync();
            foreach (var p in data)
            {
                p.Thubmnail = imageService.GetImageUrl(p.Id, p.Thubmnail);
            }
            return data;
        }


        [HttpGet]
        public async Task<List<ProductConciseModel>> Search([FromQuery] string q, [FromQuery] int? offset, [FromQuery] int? page)
        {
            var products = (IQueryable<Product>)productService.ProductsView().Where(p => p.Title.Contains(q))
                .OrderBy(p => p.Id);

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
                result.Add(cp);
                cp.Children = new List<CategoryWithProductsModel>();
                CreateHierarchy(c.Id, cp.Children, c.Children);
            }
        }


    }
}