using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReviewsController(AppDbContext context)
    {
        _context = context;
    }

    // Thêm review mới
    [HttpPost]
    public async Task<IActionResult> AddReview([FromBody] ReviewDTO reviewDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var review = new Review
        {
            FoodId = reviewDto.FoodId,
            CustomerId = reviewDto.CustomerId,
            Rating = reviewDto.Rating,
            Comment = reviewDto.Comment
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Review added successfully!" });
    }

    // Lấy tất cả review của món ăn
    [HttpGet("food/{foodId}")]
    public async Task<IActionResult> GetReviewsByFood(int foodId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.FoodId == foodId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.ReviewId,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                CustomerName = r.Customer.Name // Nếu bạn có thuộc tính Name trong Customer
            })
            .ToListAsync();

        return Ok(reviews);
    }
}
