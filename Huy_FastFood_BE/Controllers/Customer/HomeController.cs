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

        [Authorize(Roles = "Customer")]
        [HttpGet("recent-orders")]
        public async Task<IActionResult> GetRecentOrderedFoods()
        {
            try
            {
                // Lấy UserId từ JWT token
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated." });
                }

                var accountId = int.Parse(userIdClaim.Value);

                // Lấy thông tin khách hàng từ tài khoản
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == accountId);
                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found." });
                }

                // Lấy danh sách món ăn từ các đơn hàng của khách hàng
                var recentFoods = await _context.OrderItems
                    .Where(oi => oi.Order != null && oi.Order.CustomerId == customer.CustomerId)
                    .OrderByDescending(oi => oi.Order.OrderDate) // Sắp xếp theo ngày đặt hàng giảm dần
                    .Take(5) // Giới hạn lấy tối thiểu 5 món
                    .Select(oi => new
                    {
                        FoodId = oi.FoodId,
                        FoodName = oi.Food.Name,
                        Price = oi.Food.Price,
                        ImageUrl = oi.Food.ImageUrl,
                        Quantity = oi.Quantity,
                        OrderDate = oi.Order.OrderDate
                    })
                    .ToListAsync();

                if (!recentFoods.Any())
                {
                    return NotFound(new { message = "No recent orders found for this customer." });
                }

                return Ok(new { message = "Recent ordered foods retrieved successfully.", data = recentFoods });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving recent ordered foods.", error = ex.Message });
            }
        }


        // GET: api/Category/GetPagedCategories?pageNumber=1&pageSize=10
        [HttpGet("GetPagedCategories")]
        public async Task<IActionResult> GetPagedCategories(int pageNumber = 1, int pageSize = 5)
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
                // Giả sử "IsPopular" là tiêu chí bạn muốn sử dụng, hoặc thay bằng logic thực tế như số lượng bán ra
                var favoriteFoodsQuery = await _context.Foods
                    .Where(f => f.IsPopular == true) // Thay bằng logic phù hợp (VD: số lượng bán > 100)
                    .Include(f => f.Category)
                    .Select(f => new FoodFavoriteDTO
                    {
                        FoodId = f.FoodId,
                        Name = f.Name,
                        Price = f.Price,
                        ImageUrl = f.ImageUrl,
                        Description = f.Description,
                        CategoryName = f.Category.CategoryName,
                        SeoTitle = f.SeoTitle,
                        SeoDescription = f.SeoDescription,
                        SeoKeywords = f.SeoKeywords,
                        Slug = f.Slug
                    })
                    .ToListAsync();

                return Ok(favoriteFoodsQuery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving favorite foods.", error = ex.Message });
            }
        }

    }
}
