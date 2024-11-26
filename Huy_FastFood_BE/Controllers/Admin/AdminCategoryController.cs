using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Huy_FastFood_BE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Huy_FastFood_BE.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminCategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminCategoryController(AppDbContext context)
        {
            _context = context;
        }

        // Get all categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                .Select(c => new CategoryDTO
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    SeoTitle = c.SeoTitle,
                    SeoDescription = c.SeoDescription,
                    SeoKeywords = c.SeoKeywords,
                    Slug = c.Slug,
                    ImgUrl = c.ImgUrl
                }).ToListAsync();

                return Ok(categories);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving data to database");
            }
        }

        // Get category by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _context.Categories
                .Where(c => c.CategoryId == id)
                .Select(c => new CategoryDTO
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    SeoTitle = c.SeoTitle,
                    SeoDescription = c.SeoDescription,
                    SeoKeywords = c.SeoKeywords,
                    Slug = c.Slug,
                    ImgUrl = c.ImgUrl
                }).FirstOrDefaultAsync();
                if (category == null)
                {
                    return NotFound("Category not found.");
                }

                return Ok(category);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving data to database");
            }
        }

        // Add a new category
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDTO categoryDto)
        {
            try
            {
                if (categoryDto == null)
                {
                    return BadRequest("Invalid category data.");
                }

                var category = new Category
                {
                    CategoryName = categoryDto.CategoryName,
                    Description = categoryDto.Description,
                    SeoTitle = categoryDto.SeoTitle,
                    SeoDescription = categoryDto.SeoDescription,
                    SeoKeywords = categoryDto.SeoKeywords,
                    Slug = SlugHelper.GenerateSlug(categoryDto.CategoryName),
                    ImgUrl = categoryDto.ImgUrl
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                categoryDto.CategoryId = category.CategoryId; // Set the generated ID

                return CreatedAtAction(nameof(GetCategoryById), new { id = categoryDto.CategoryId }, categoryDto);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving data to database");
            }
        }

        // Update a category
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDTO categoryDto)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound("Category not found.");
                }

                category.CategoryName = categoryDto.CategoryName;
                category.Description = categoryDto.Description;
                category.SeoTitle = categoryDto.SeoTitle;
                category.SeoDescription = categoryDto.SeoDescription;
                category.SeoKeywords = categoryDto.SeoKeywords;
                category.Slug = SlugHelper.GenerateSlug(categoryDto.CategoryName);
                category.ImgUrl = categoryDto.ImgUrl;

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                return Ok(category);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving data to database");
            }
        }

        // Delete a category
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound("Category not found.");
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return Ok("Category deleted successfully.");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving data to database");
            }
        }
    }
}
