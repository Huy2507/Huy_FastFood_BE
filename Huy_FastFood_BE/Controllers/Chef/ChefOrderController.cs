using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;

namespace Huy_FastFood_BE.Controllers
{
    [Authorize(Roles = "Chef")]
    [ApiController]
    [Route("api/[controller]")]
    public class ChefOrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChefOrderController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetPendingOrders()
        {
            var orders = await _context.Orders
                .Where(o => o.Status == "Pending") // Chỉ lấy các đơn hàng có trạng thái "Pending"
                .OrderBy(o => o.OrderDate) // Sắp xếp theo thứ tự cũ nhất đến mới nhất
                .Include(o => o.OrderItems)
                .Select(o => new OrderDTO
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    Note = o.Note,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDTO
                    {
                        FoodId = oi.FoodId,
                        ImageUrl = oi.Food.ImageUrl,
                        FoodName = oi.Food.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price,
                        TotalPrice = oi.TotalPrice
                    }).ToList()
                }).ToListAsync();

            if (!orders.Any())
            {
                return NotFound(new { message = "No pending orders found." });
            }

            return Ok(orders);
        }



        // Cập nhật trạng thái của order
        [HttpPut("orders/{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return BadRequest(new { message = "Status is required." });
            }

            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            order.Status = status;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order status updated successfully." });
        }
    }
}
