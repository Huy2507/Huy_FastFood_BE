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

        // Phương thức ánh xạ từ entity sang DTO
        private FoodDTO MapToDTO(Food food)
        {
            return new FoodDTO
            {
                FoodId = food.FoodId,
                Name = food.Name,
                Description = food.Description,
                Price = food.Price,
                CategoryId = food.CategoryId,
                ImageUrl = food.ImageUrl,
                Enable = food.Enable,
                IsPopular = food.IsPopular,
                SeoTitle = food.SeoTitle,
                SeoDescription = food.SeoDescription,
                SeoKeywords = food.SeoKeywords,
                Slug = food.Slug,
                CreatedAt = food.CreatedAt,
                UpdatedAt = food.UpdatedAt
            };
        }

        // Phương thức ánh xạ từ DTO sang entity
        private Food MapToEntity(FoodDTO foodDTO)
        {
            return new Food
            {
                FoodId = foodDTO.FoodId,
                Name = foodDTO.Name,
                Description = foodDTO.Description,
                Price = foodDTO.Price,
                CategoryId = foodDTO.CategoryId,
                ImageUrl = foodDTO.ImageUrl,
                Enable = foodDTO.Enable,
                IsPopular= foodDTO.IsPopular,
                SeoTitle = foodDTO.SeoTitle,
                SeoDescription = foodDTO.SeoDescription,
                SeoKeywords = foodDTO.SeoKeywords,
                Slug = foodDTO.Slug,
                CreatedAt = foodDTO.CreatedAt,
                UpdatedAt = foodDTO.UpdatedAt
            };
        }


        // Lấy danh sách tất cả món ăn
        [HttpGet]
        public async Task<IActionResult> GetAllFoods()
        {
            try
            {
                var foods = await _dbContext.Foods.ToListAsync();
                var foodDTOs = foods.Select(food => MapToDTO(food)).ToList();

                return Ok(foodDTOs);
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
                var food = await _dbContext.Foods.FindAsync(id);
                if (food == null)
                    return NotFound(new { message = "Food not found" });

                var foodDTO = MapToDTO(food);
                return Ok(foodDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from database");
            }
        }

        // Thêm món ăn mới
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddFood([FromForm] FoodCreateDTO foodDTO)
        {
            try
            {
                if (foodDTO == null)
                    return BadRequest(new { message = "Invalid food data" });

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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Lưu vào database
                await _dbContext.Foods.AddAsync(food);
                await _dbContext.SaveChangesAsync();

                // Chuyển đổi sang DTO
                var createdFoodDTO = MapToDTO(food);
                return CreatedAtAction(nameof(GetFoodById), new { id = food.FoodId }, createdFoodDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error saving data to database", error = ex.Message });
            }
        }


        // Cập nhật món ăn
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateFood(int id, [FromForm] FoodCreateDTO foodDTO)
        {
            try
            {
                var food = await _dbContext.Foods.FindAsync(id);
                if (food == null)
                    return NotFound(new { message = "Food not found" });

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
                food.UpdatedAt = DateTime.UtcNow;

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
