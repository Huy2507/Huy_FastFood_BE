using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Huy_FastFood_BE.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Customer")]
    public class FoodListController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FoodListController(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        [HttpGet("foods-by-category-popular")]
        public async Task<IActionResult> GetFoodsByCategoryWithPopular(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                // Lấy danh sách món ăn từ CSDL
                var foodsQuery = _context.Foods.AsQueryable();

                // Lọc theo từ khóa tìm kiếm
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    foodsQuery = foodsQuery.Where(f =>
                        f.Name.Contains(searchTerm) ||
                        f.Description.Contains(searchTerm));
                }

                // Lấy danh mục món ăn
                var categories = await _context.Categories
                    .Select(c => new FoodCategoryDTO
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        Slug = c.Slug
                    }).ToListAsync();

                // Lọc món ăn phổ biến
                var popularFoods = await foodsQuery
                    .Where(f => f.IsPopular == true)
                    .Select(f => new FoodFavoriteDTO
                    {
                        FoodId = f.FoodId,
                        Name = f.Name,
                        Price = f.Price,
                        ImageUrl = f.ImageUrl,
                        Description = f.Description,
                        CategoryName = f.Category.CategoryName,
                        IsPopular = f.IsPopular
                    }).ToListAsync();

                // Tạo danh sách món ăn theo danh mục
                var foodsByCategory = new List<FoodCategoryWithItemsDTO>();

                foreach (var category in categories)
                {
                    var foodsInCategory = await foodsQuery
                        .Where(f => f.CategoryId == category.CategoryId)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(f => new FoodDTO
                        {
                            FoodId = f.FoodId,
                            Name = f.Name,
                            Price = f.Price,
                            ImageUrl = f.ImageUrl,
                            Description = f.Description,
                            IsPopular = f.IsPopular
                        }).ToListAsync();

                    if (foodsInCategory.Any())
                    {
                        foodsByCategory.Add(new FoodCategoryWithItemsDTO
                        {
                            Category = category,
                            Foods = foodsInCategory
                        });
                    }
                }

                // Kết quả trả về
                var result = new
                {
                    PopularFoods = popularFoods,
                    FoodsByCategory = foodsByCategory
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching foods by category.", error = ex.Message });
            }
        }

    }
}
