using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.Models;
using MahtaKala.Models.CategoryModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MahtaKala.Controllers
{
    [ApiController()]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1")]
    public class ProductController : ControllerBase
    {

        private readonly DataContext db;
        public ProductController(DataContext context)
        {
            this.db = context;
        }


        /// <summary>
        /// Update the Existing Category or Create New One
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Success. The category was updated.</response>
        /// <response code="201">Success. Category was new.</response>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Category([FromBody]UpdateCategoryRequest updateCategoryRequest)
        {
            ProductCategory category = null;
            bool newCategory = false;
            if(updateCategoryRequest.Id == 0)
            {
                newCategory = true;
                category = new ProductCategory();
                db.Categories.Add(category);
            }
            else
            {
                category = await db.Categories.FirstOrDefaultAsync(c => c.Id == updateCategoryRequest.Id);
                if (category == null)
                    throw new Exception("Category Not Found.");
            }

            category.Title = updateCategoryRequest.Title;
            if (!string.IsNullOrEmpty(updateCategoryRequest.Icon))
                category.Image = Convert.FromBase64String(updateCategoryRequest.Icon);
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
        [Authorize]
        [HttpGet]
        public async Task<List<ProductCategory>> Category([FromBody]GetListCategoryRequest getListCategoryModel)
        {
            return await db.Categories.Where(c => c.ParentId == getListCategoryModel.Parent).ToListAsync();
        }
        /// <summary>
        /// Removes the Category with the given ID
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Success. The category was Deleted.</response>
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Category([FromBody]IdModel model)
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == model.Id);
            if(category == null)
            {
                throw new Exception("Category Not Found.");
            }
            if(await db.Categories.AnyAsync(c=>c.ParentId == model.Id))
            {
                throw new Exception("Category Has Child.");
            }
            db.Categories.Remove(category);
            await db.SaveChangesAsync();
            return StatusCode(200);

        }
    }
}