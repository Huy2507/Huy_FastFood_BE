using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Huy_FastFood_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Huy_FastFood_BE.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateCategory([FromForm] CategoryCreateDTO categoryDto)
        {
            try
            {
                if (categoryDto == null)
                {
                    return BadRequest("Invalid category data.");
                }

                string imageUrl = null;

                // Xử lý file ảnh tải lên
                if (categoryDto.ImageFile != null && categoryDto.ImageFile.Length > 0)
                {
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Category");
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(categoryDto.ImageFile.FileName);
                    var filePath = Path.Combine(imagesFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await categoryDto.ImageFile.CopyToAsync(stream);
                    }

                    imageUrl = $"/Images/Category/{fileName}";
                }

                var category = new Category
                {
                    CategoryName = categoryDto.CategoryName,
                    Description = categoryDto.Description,
                    SeoTitle = categoryDto.SeoTitle,
                    SeoDescription = categoryDto.SeoDescription,
                    SeoKeywords = categoryDto.SeoKeywords,
                    Slug = SlugHelper.GenerateSlug(categoryDto.CategoryName),
                    ImgUrl = imageUrl
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, category);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving data to database");
            }
        }


        // Update a category
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCategory(int id, [FromForm] CategoryCreateDTO categoryDto)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound("Category not found.");
                }

                // Xử lý ảnh mới (nếu có)
                if (categoryDto.ImageFile != null && categoryDto.ImageFile.Length > 0)
                {
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Category");
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    // Xóa ảnh cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(category.ImgUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", category.ImgUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Lưu ảnh mới
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(categoryDto.ImageFile.FileName);
                    var filePath = Path.Combine(imagesFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await categoryDto.ImageFile.CopyToAsync(stream);
                    }

                    category.ImgUrl = $"/Images/Category/{fileName}";
                }

                // Cập nhật thông tin khác
                category.CategoryName = categoryDto.CategoryName;
                category.Description = categoryDto.Description;
                category.SeoTitle = categoryDto.SeoTitle;
                category.SeoDescription = categoryDto.SeoDescription;
                category.SeoKeywords = categoryDto.SeoKeywords;
                category.Slug = SlugHelper.GenerateSlug(categoryDto.CategoryName);

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

                // Xóa ảnh nếu tồn tại
                if (!string.IsNullOrEmpty(category.ImgUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", category.ImgUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
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
