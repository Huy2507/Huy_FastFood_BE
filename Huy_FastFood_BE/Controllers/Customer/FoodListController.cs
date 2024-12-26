using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Huy_FastFood_BE.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodListController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FoodListController(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        [HttpGet("menu-foods")]
        public async Task<IActionResult> GetFoodsByCategoryWithPopular(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 6,
    [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (page < 1 || pageSize <= 0)
                {
                    return BadRequest(new { message = "Invalid page or pageSize parameters." });
                }

                // Lọc món ăn theo từ khóa tìm kiếm và đảm bảo chỉ lấy những món ăn có Enable = true
                var foodsQuery = _context.Foods.Where(f => f.Enable == true).AsQueryable();
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

                // Lọc món ăn phổ biến và đảm bảo chỉ lấy những món ăn có Enable = true
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

                // Tạo danh sách món ăn theo từng danh mục
                var foodsByCategory = new List<object>();

                foreach (var category in categories)
                {
                    // Lấy món ăn theo danh mục và tính toán tổng số món
                    var categoryFoodsQuery = foodsQuery.Where(f => f.CategoryId == category.CategoryId);
                    var totalRecords = await categoryFoodsQuery.CountAsync();
                    var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                    // Lấy danh sách món ăn theo phân trang
                    var foodsInCategory = await categoryFoodsQuery
                        .OrderBy(f => f.Name) // Sắp xếp theo tên món ăn
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

                    // Thêm danh mục và thông tin phân trang vào kết quả
                    foodsByCategory.Add(new
                    {
                        Category = category,
                        Foods = foodsInCategory,
                        Pagination = new
                        {
                            CurrentPage = page,
                            PageSize = pageSize,
                            TotalRecords = totalRecords,
                            TotalPages = totalPages
                        }
                    });
                }

                // Trả kết quả
                var result = new
                {
                    PopularFoods = popularFoods,
                    FoodsByCategory = foodsByCategory
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error fetching foods by category.",
                    error = ex.Message
                });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetFoodById(int id)
        {
            try
            {
                var food = await _context.Foods
                                  .Include(f => f.Category)
                                  .FirstOrDefaultAsync(f => f.FoodId == id);
                if (food == null)
                    return NotFound(new { message = "Food not found" });

                var foodDTO = new FoodDetailsDTO
                {
                    FoodId = food.FoodId,
                    Name = food.Name,
                    Description = food.Description,
                    Price = food.Price,
                    CategoryName = food.Category.CategoryName,
                    ImageUrl = food.ImageUrl,
                    SeoTitle = food.SeoTitle,
                    SeoDescription = food.SeoDescription,
                    SeoKeywords = food.SeoKeywords,
                    Slug = food.Slug
                };
                return Ok(foodDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from database");
            }
        }
    }
}
