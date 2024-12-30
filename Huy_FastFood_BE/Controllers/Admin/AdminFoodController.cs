using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Huy_FastFood_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Huy_FastFood_BE.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminFoodController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public AdminFoodController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Lấy danh sách tất cả món ăn
        [HttpGet]
        public async Task<IActionResult> GetAllFoods(
    string? search = null,
    bool? enable = null,
    bool? isPopular = null)
        {
            try
            {
                var query = _dbContext.Foods.Include(f => f.Category).AsQueryable();

                // Lọc theo tên hoặc mô tả
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(f =>
                        f.Name.Contains(search) || f.Description.Contains(search) || f.FoodId.ToString().Contains(search));
                }

                // Lọc theo khả dụng
                if (enable.HasValue)
                {
                    query = query.Where(f => f.Enable == enable.Value);
                }

                // Lọc theo phổ biến
                if (isPopular.HasValue)
                {
                    query = query.Where(f => f.IsPopular == isPopular.Value);
                }

                // Chuyển đổi dữ liệu sang DTO
                var foods = await query.Select(f => new FoodDTO
                {
                    FoodId = f.FoodId,
                    Name = f.Name,
                    Description = f.Description,
                    Price = f.Price,
                    CategoryName = f.Category.CategoryName,
                    ImageUrl = f.ImageUrl,
                    Enable = f.Enable,
                    IsPopular = f.IsPopular,
                    SeoTitle = f.SeoTitle,
                    SeoDescription = f.SeoDescription,
                    SeoKeywords = f.SeoKeywords,
                    Slug = f.Slug,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                }).ToListAsync();

                return Ok(foods);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from database");
            }
        }


        // Lấy món ăn theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFoodById(int id)
        {
            try
            {
                var food = await _dbContext.Foods
                    .Where(f => f.FoodId == id)
                    .Select(f => new FoodDTO
                {
                    FoodId = f.FoodId,
                    Name = f.Name,
                    Description = f.Description,
                    Price = f.Price,
                    CategoryId = f.Category.CategoryId,
                    CategoryName = f.Category.CategoryName,
                    ImageUrl = f.ImageUrl,
                    Enable = f.Enable,
                    IsPopular = f.IsPopular,
                    SeoTitle = f.SeoTitle,
                    SeoDescription = f.SeoDescription,
                    SeoKeywords = f.SeoKeywords,
                    Slug = f.Slug,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                }).FirstOrDefaultAsync();
                if (food == null)
                    return NotFound(new { message = "Food not found" });

                return Ok(food);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from database");
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddFood([FromForm] FoodCreateDTO foodDTO)
        {
            try
            {
                if (foodDTO == null)
                    return BadRequest(new { message = "Invalid food data" });

                // Kiểm tra xem món ăn đã tồn tại với tên này chưa
                var existingFood = await _dbContext.Foods
                    .Where(f => f.Name == foodDTO.Name)
                    .FirstOrDefaultAsync();

                if (existingFood != null)
                {
                    return BadRequest(new { message = "Food name already exists. Please enter a different name." });
                }

                string imageUrl = null;

                // Xử lý file ảnh tải lên
                if (foodDTO.ImageFile != null && foodDTO.ImageFile.Length > 0)
                {
                    // Đường dẫn thư mục lưu ảnh
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Food");
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    // Tạo tên file duy nhất để tránh trùng lặp
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(foodDTO.ImageFile.FileName);
                    var filePath = Path.Combine(imagesFolder, fileName);

                    // Lưu file ảnh
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await foodDTO.ImageFile.CopyToAsync(stream);
                    }

                    // Lấy đường dẫn URL của ảnh
                    imageUrl = $"/Images/Food/{fileName}";
                }

                // Tạo đối tượng Food
                var food = new Food
                {
                    Name = foodDTO.Name,
                    Description = foodDTO.Description,
                    Price = foodDTO.Price,
                    Enable = foodDTO.Enable,
                    IsPopular = (bool)foodDTO.IsPopular,
                    ImageUrl = imageUrl, // Gán đường dẫn ảnh
                    CategoryId = foodDTO.CategoryId,
                    SeoKeywords = foodDTO.SeoKeywords,
                    SeoDescription = foodDTO.SeoDescription,
                    SeoTitle = foodDTO.SeoTitle,
                    Slug = SlugHelper.GenerateSlug(foodDTO.Name),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Lưu vào database
                await _dbContext.Foods.AddAsync(food);
                await _dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error saving data to database", error = ex.Message });
            }
        }
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateFood(int id, [FromForm] FoodCreateDTO foodDTO)
        {
            try
            {
                var food = await _dbContext.Foods.FindAsync(id);
                if (food == null)
                    return NotFound(new { message = "Food not found" });

                // Kiểm tra xem món ăn khác có tên giống món ăn này không
                var existingFood = await _dbContext.Foods
                    .Where(f => f.Name == foodDTO.Name && f.FoodId != id)
                    .FirstOrDefaultAsync();

                if (existingFood != null)
                {
                    return BadRequest(new { message = "Food name already exists. Please enter a different name." });
                }

                string imageUrl = food.ImageUrl; // Giữ ảnh cũ nếu không tải ảnh mới

                // Xử lý file ảnh tải lên
                if (foodDTO.ImageFile != null && foodDTO.ImageFile.Length > 0)
                {
                    // Đường dẫn thư mục lưu ảnh
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Food");
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    // Tạo tên file duy nhất để tránh trùng lặp
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(foodDTO.ImageFile.FileName);
                    var filePath = Path.Combine(imagesFolder, fileName);

                    // Lưu file ảnh mới
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await foodDTO.ImageFile.CopyToAsync(stream);
                    }

                    // Xóa ảnh cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(food.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", food.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Cập nhật đường dẫn ảnh mới
                    imageUrl = $"/Images/Food/{fileName}";
                }

                // Cập nhật thông tin món ăn
                food.Name = foodDTO.Name;
                food.Description = foodDTO.Description;
                food.Price = foodDTO.Price;
                food.Enable = foodDTO.Enable;
                food.IsPopular = foodDTO.IsPopular;
                food.ImageUrl = imageUrl; // Gán ảnh mới (hoặc giữ ảnh cũ)
                food.CategoryId = foodDTO.CategoryId;
                food.SeoKeywords = foodDTO.SeoKeywords;
                food.SeoDescription = foodDTO.SeoDescription;
                food.SeoTitle = foodDTO.SeoTitle;
                food.Slug = SlugHelper.GenerateSlug(foodDTO.Name);
                food.UpdatedAt = DateTime.Now;

                _dbContext.Foods.Update(food);
                await _dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error updating data", error = ex.Message });
            }
        }


    }
}
