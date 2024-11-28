using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Huy_FastFood_BE.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        // GET: api/Banner
        [HttpGet("banner")]
        public async Task<IActionResult> GetAllBanners()
        {
            try
            {
                var banners = await _context.Banners
                    .Select(b => new BannerDTO
                    {
                        Id = b.Id,
                        BannerImg = b.BannerImg,
                        Title = b.Title,
                        Description = b.Description,
                        LinkUrl = b.LinkUrl,
                        SeoTitle = b.SeoTitle,
                        SeoDescript = b.SeoDescript,
                        SeoKeywords = b.SeoKeywords,
                        Slug = b.Slug,
                    })
                    .ToListAsync();

                return Ok(banners);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // GET: api/Category/GetPagedCategories?pageNumber=1&pageSize=10
        [HttpGet("GetPagedCategories")]
        public async Task<IActionResult> GetPagedCategories(int pageNumber = 1, int pageSize = 2)
        {
            try
            {
                // Lấy tổng số bản ghi
                var totalRecords = await _context.Categories.CountAsync();

                // Tính toán phân trang
                var categories = await _context.Categories
                    .OrderBy(c => c.CategoryId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
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
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Data = categories,
                    Pagination = new
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching categories", error = ex.Message });
            }
        }

        // GET: api/Food/favorites
        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavoriteFoods()
        {
            try
            {
                // Tính tổng số lượng bán theo món ăn
                var favoriteFoodsQuery = await _context.Foods
                    .Include(f => f.OrderItems) // Kết nối với bảng Order_Items
                    .Select(f => new
                    {
                        f.FoodId,
                        f.Name,
                        f.Description,
                        f.Price,
                        f.ImageUrl,
                        TotalSold = f.OrderItems.Sum(oi => oi.Quantity) // Tổng số lượng bán
                    })
                    .OrderByDescending(f => f.TotalSold).Take(5).ToListAsync(); // Sắp xếp theo số lượng bán giảm dần

                return Ok(favoriteFoodsQuery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving favorite foods.", error = ex.Message });
            }
        }
    }
}
