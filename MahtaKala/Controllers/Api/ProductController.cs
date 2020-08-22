using System;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MahtaKala.Controllers
{
    [ApiController()]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1")]
    [Authorize]
    public class ProductController : ApiControllerBase<ProductController>
    {
        public ProductController(DataContext context, ILogger<ProductController> logger)
            : base(context, logger)
        {
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
            ProductCategory category = null;
            bool newCategory = false;
            if (updateCategoryRequest.Id == 0)
            {
                newCategory = true;
                category = new ProductCategory();
                db.Categories.Add(category);
            }
            else
            {
                category = await db.Categories.FirstOrDefaultAsync(c => c.Id == updateCategoryRequest.Id);
                if (category == null)
                    throw new EntityNotFoundException<ProductCategory>(updateCategoryRequest.Id);
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
        public async Task<List<ProductCategory>> Category([FromQuery] long? parent)
        {
            return await db.Categories.Where(c => c.ParentId == parent).ToListAsync();
        }
        /// <summary>
        /// Removes the Category with the given ID
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Success. The category was Deleted.</response>
        [HttpDelete]
        public async Task<IActionResult> DeleteCategory([FromQuery] long id)
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                throw new EntityNotFoundException<ProductCategory>(id);
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
        public async Task Product([FromBody]ProductModel productMode)
        {
            Product product;
            if(productMode.Id>0)
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
            product.CategoryId = productMode.Category_Id;
            product.Characteristics = productMode.Characteristics;
            product.Description = productMode.Description;
            product.Properties = productMode.Properties;
            product.Thubmnail = productMode.Thubmnail;
            product.Title = productMode.Title;
            await db.SaveChangesAsync();
        }

        //public async Task<string> ProductImage(long id)
        //{

        //}

        [HttpDelete]
        public async Task<StatusCodeResult> DeleteProduct(long id)
        {
            var product = await db.Products.FirstOrDefaultAsync(c => c.Id == id);
            if (product == null)
            {
                throw new EntityNotFoundException<ProductCategory>(id);
            }
            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return StatusCode(200);
        }

        [HttpGet]
        public async Task<ProductModel> Product(long id)
        {
            return await db.Products
                .Where(a => a.Id == id)
                .Select(a => new ProductModel
                {
                    Id = a.Id,
                    Brand_Id = a.BrandId,
                    Category_Id = a.CategoryId,
                    Description = a.Description,
                    Title = a.Title,
                    Thubmnail = a.Thubmnail,
                    Characteristics = a.Characteristics,
                    Properties = a.Properties,
                    ImageList = a.ImageList
                }).FirstOrDefaultAsync();
        }

        [HttpGet]
        public async Task<List<ProductModel>> Products([FromQuery] long? category, [FromQuery] int offset, [FromQuery] int page)
        {
            return await db.Products
                .Where(a => a.CategoryId == category)
                .OrderBy(p => p.Id).Skip(offset).Take(page)
                .Select(a => new ProductModel
                {
                    Id = a.Id,
                    Brand_Id = a.BrandId,
                    Category_Id = a.CategoryId,
                    Description = a.Description,
                    Title = a.Title,
                    Thubmnail = a.Thubmnail,
                    Characteristics = a.Characteristics,
                    Properties = a.Properties,
                    ImageList = a.ImageList
                })
                .ToListAsync();
        }

        /// <summary>
        /// Returns the products that must be displayed in the home page of the app. This will include promotions and ads.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<ProductModel>> Home()
        {
            return await db.Products
                .OrderBy(p => p.Id).Take(10)
                .Select(a => new ProductModel
                {
                    Id = a.Id,
                    Brand_Id = a.BrandId,
                    Category_Id = a.CategoryId,
                    Description = a.Description,
                    Title = a.Title,
                    Thubmnail = a.Thubmnail,
                    Characteristics = a.Characteristics,
                    Properties = a.Properties,
                    ImageList = a.ImageList
                })
                .ToListAsync();
        }




    }
}