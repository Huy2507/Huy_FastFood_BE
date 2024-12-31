using Huy_FastFood_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Huy_FastFood_BE.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminStatisticController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public AdminStatisticController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("best-selling-foods")]
        public async Task<IActionResult> GetBestSellingFoods(
    [FromQuery] int? year,
    [FromQuery] int? month,
    [FromQuery] string period = "day",
    [FromQuery] int limit = 10)
        {
            try
            {
                // Xác định khoảng thời gian
                DateTime start, end;

                if (month.HasValue)
                {
                    start = new DateTime(year ?? DateTime.UtcNow.Year, month.Value, 1);
                    end = start.AddMonths(1).AddDays(-1);
                }
                else if (year.HasValue)
                {
                    start = new DateTime(year.Value, 1, 1);
                    end = new DateTime(year.Value, 12, 31);
                }
                else
                {
                    start = period switch
                    {
                        "week" => DateTime.UtcNow.AddDays(-7),
                        "year" => DateTime.UtcNow.AddYears(-1),
                        _ => DateTime.UtcNow.AddMonths(-1)
                    };
                    end = DateTime.UtcNow;
                }

                // Truy vấn dữ liệu
                var bestSellingFoods = await _dbContext.OrderItems
                    .Where(oi => oi.Order.OrderDate.HasValue && oi.Order.OrderDate >= start && oi.Order.OrderDate <= end)
                    .GroupBy(oi => new { oi.FoodId, oi.Food.Name, oi.Food.Category.CategoryName, oi.Food.ImageUrl })
                    .Select(g => new
                    {
                        FoodId = g.Key.FoodId,
                        FoodName = g.Key.Name,
                        ImageUrl = g.Key.ImageUrl,
                        Category = g.Key.CategoryName,
                        QuantitySold = g.Sum(oi => oi.Quantity),
                        TotalRevenue = g.Sum(oi => oi.TotalPrice)
                    })
                    .OrderByDescending(f => f.QuantitySold)
                    .Take(limit)
                    .ToListAsync();

                return Ok(bestSellingFoods );
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("popular-categories")]
        public async Task<IActionResult> GetPopularCategories(
            [FromQuery] int? year,
            [FromQuery] int? month,
            [FromQuery] string period = "day",
            [FromQuery] int limit = 5)
        {
            try
            {
                // Xác định khoảng thời gian
                DateTime start, end;

                if (month.HasValue)
                {
                    start = new DateTime(year ?? DateTime.UtcNow.Year, month.Value, 1);
                    end = start.AddMonths(1).AddDays(-1);
                }
                else if (year.HasValue)
                {
                    start = new DateTime(year.Value, 1, 1);
                    end = new DateTime(year.Value, 12, 31);
                }
                else
                {
                    start = period switch
                    {
                        "week" => DateTime.UtcNow.AddDays(-7),
                        "year" => DateTime.UtcNow.AddYears(-1),
                        _ => DateTime.UtcNow.AddMonths(-1)
                    };
                    end = DateTime.UtcNow;
                }

                // Truy vấn dữ liệu
                var popularCategories = await _dbContext.OrderItems
                    .Where(oi => oi.Order.OrderDate.HasValue && oi.Order.OrderDate >= start && oi.Order.OrderDate <= end)
                    .GroupBy(oi => new { oi.Food.CategoryId, oi.Food.Category.CategoryName, oi.Food.Category.ImgUrl })
                    .Select(g => new
                    {
                        CategoryId = g.Key.CategoryId,
                        ImageUrl = g.Key.ImgUrl,
                        CategoryName = g.Key.CategoryName,
                        QuantitySold = g.Sum(oi => oi.Quantity),
                        TotalRevenue = g.Sum(oi => oi.TotalPrice)
                    })
                    .OrderByDescending(c => c.QuantitySold)
                    .Take(limit)
                    .ToListAsync();

                return Ok(popularCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }



        [HttpGet("order-count")]
        public async Task<IActionResult> GetOrderCountStatistics(
    [FromQuery] int? year,
    [FromQuery] int? month,
    [FromQuery] string period = "day")
        {
            try
            {
                // Xác định khoảng thời gian
                DateTime start, end;

                if (month.HasValue)
                {
                    start = new DateTime(year ?? DateTime.UtcNow.Year, month.Value, 1);
                    end = new DateTime(start.Year, start.Month, DateTime.DaysInMonth(start.Year, start.Month)).AddDays(1);
                }
                else if (year.HasValue)
                {
                    start = new DateTime(year.Value, 1, 1);
                    end = new DateTime(year.Value, 12, 31).AddDays(1);
                }
                else
                {
                    start = DateTime.UtcNow.AddDays(-30).Date;
                    end = DateTime.UtcNow.AddDays(1).Date;
                }

                // Truy vấn cơ sở dữ liệu
                var query = _dbContext.Orders
                    .Where(o => o.OrderDate.HasValue && o.OrderDate >= start && o.OrderDate < end);

                // Xử lý thống kê
                var statistics = period.ToLower() switch
                {
                    "day" => query
                        .GroupBy(o => o.OrderDate.Value.Date)
                        .Select(g => new
                        {
                            Date = g.Key.ToString("yyyy-MM-dd"),
                            OrderCount = g.Count()
                        }),
                    "week" => query
                        .GroupBy(o => new
                        {
                            Week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(o.OrderDate.Value, CalendarWeekRule.FirstDay, DayOfWeek.Monday),
                            Year = o.OrderDate.Value.Year
                        })
                        .Select(g => new
                        {
                            Date = $"Year {g.Key.Year}, Week {g.Key.Week}",
                            OrderCount = g.Count()
                        }),
                    _ => query
                        .GroupBy(o => new { o.OrderDate.Value.Year, o.OrderDate.Value.Month })
                        .Select(g => new
                        {
                            Date = $"{g.Key.Year}-{g.Key.Month:D2}",
                            OrderCount = g.Count()
                        })
                };

                return Ok(await statistics.ToListAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        [HttpGet("order-revenue")]
        public async Task<IActionResult> GetOrderRevenueStatistics(
    [FromQuery] int? year,
    [FromQuery] int? month,
    [FromQuery] string period = "month")
        {
            try
            {
                // Xác định khoảng thời gian
                DateTime start, end;

                if (month.HasValue)
                {
                    start = new DateTime(year ?? DateTime.UtcNow.Year, month.Value, 1);
                    end = new DateTime(start.Year, start.Month, DateTime.DaysInMonth(start.Year, start.Month)).AddDays(1);
                }
                else if (year.HasValue)
                {
                    start = new DateTime(year.Value, 1, 1);
                    end = new DateTime(year.Value, 12, 31).AddDays(1);
                }
                else
                {
                    start = DateTime.UtcNow.AddDays(-30).Date;
                    end = DateTime.UtcNow.AddDays(1).Date;
                }

                // Truy vấn cơ sở dữ liệu
                var query = _dbContext.Orders
                    .Where(o => o.OrderDate.HasValue && o.OrderDate >= start && o.OrderDate < end);

                // Xử lý thống kê
                var statistics = period.ToLower() switch
                {
                    "day" => query
                        .GroupBy(o => o.OrderDate.Value.Date)
                        .Select(g => new
                        {
                            Date = g.Key.ToString("yyyy-MM-dd"),
                            Revenue = g.Sum(o => o.TotalAmount)
                        }),
                    "week" => query
                        .GroupBy(o => new
                        {
                            Week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(o.OrderDate.Value, CalendarWeekRule.FirstDay, DayOfWeek.Monday),
                            Year = o.OrderDate.Value.Year
                        })
                        .Select(g => new
                        {
                            Date = $"Year {g.Key.Year}, Week {g.Key.Week}",
                            Revenue = g.Sum(o => o.TotalAmount)
                        }),
                    _ => query
                        .GroupBy(o => new { o.OrderDate.Value.Year, o.OrderDate.Value.Month })
                        .Select(g => new
                        {
                            Date = $"{g.Key.Year}-{g.Key.Month:D2}",
                            Revenue = g.Sum(o => o.TotalAmount)
                        })
                };

                return Ok(await statistics.ToListAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
