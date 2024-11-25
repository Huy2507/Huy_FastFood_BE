using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
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
    public class FoodController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public FoodController(AppDbContext dbContext)
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
        [Authorize(Roles ="Admin")]
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
        public async Task<IActionResult> AddFood([FromBody] FoodDTO foodDTO)
        {
            try
            {
                if (foodDTO == null)
                    return BadRequest(new { message = "Invalid food data" });

                var food = MapToEntity(foodDTO);
                food.CreatedAt = DateTime.UtcNow;
                food.UpdatedAt = DateTime.UtcNow;

                await _dbContext.Foods.AddAsync(food);
                await _dbContext.SaveChangesAsync();

                var createdFoodDTO = MapToDTO(food);
                return CreatedAtAction(nameof(GetFoodById), new { id = food.FoodId }, createdFoodDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving data to database");
            }
        }

        // Cập nhật món ăn
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFood(int id, [FromBody] FoodDTO foodDTO)
        {
            try
            {
                if (id != foodDTO.FoodId)
                    return BadRequest(new { message = "Food ID mismatch" });

                var food = await _dbContext.Foods.FindAsync(id);
                if (food == null)
                    return NotFound(new { message = "Food not found" });

                // Cập nhật thủ công
                food.Name = foodDTO.Name;
                food.Description = foodDTO.Description;
                food.Price = foodDTO.Price;
                food.ImageUrl = foodDTO.ImageUrl;
                food.CategoryId = foodDTO.CategoryId;
                food.UpdatedAt = DateTime.UtcNow;

                _dbContext.Foods.Update(food);
                await _dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating data");
            }
        }
    }
}
