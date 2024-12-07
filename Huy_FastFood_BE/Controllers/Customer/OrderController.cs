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
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrderController(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrderBy([FromBody] CreateOrderDTO dto)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated." });
                }

                var accountId = int.Parse(userIdClaim.Value);

                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == accountId);
                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found." });
                }

                // Lấy giỏ hàng của khách hàng
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Food)
                    .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

                if (cart == null || !cart.CartItems.Any())
                {
                    return BadRequest(new { message = "Cart is empty or does not exist." });
                }

                // Tạo đơn hàng mới
                var order = new Order
                {
                    CustomerId = customer.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = (decimal)cart.TotalPrice,
                    Status = "Pending", // Trạng thái mặc định là chờ xác nhận
                    AddressId = dto.AddressId
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Chuyển CartItems thành OrderItems
                foreach (var cartItem in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        FoodId = cartItem.FoodId,
                        Quantity = cartItem.Quantity,
                        Price = cartItem.Food.Price,
                        TotalPrice = cartItem.Food.Price * cartItem.Quantity
                    };
                    _context.OrderItems.Add(orderItem);
                }

                // Xóa giỏ hàng sau khi tạo đơn hàng
                _context.CartItems.RemoveRange(cart.CartItems);
                _context.Carts.Remove(cart);

                // Tạo Payment để theo dõi trạng thái thanh toán
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethod = "VNPay",
                    PaymentStatus = "Pending",
                    Amount = order.TotalAmount,
                    CreatedAt = DateTime.UtcNow
                };

                order.PaymentId = payment.PaymentId;

                _context.Payments.Add(payment);

                // Lưu tất cả thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Order created successfully.",
                    orderId = order.OrderId,
                    paymentId = payment.PaymentId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the order.", error = ex.Message });
            }
        }

        [HttpPut("confirm-payment/{orderId}")]
        public async Task<IActionResult> ConfirmPayment(int orderId)
        {
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);

                if (payment == null || payment.PaymentMethod != "Cash on Delivery")
                    return NotFound(new { message = "Payment not found or invalid method." });

                payment.PaymentStatus = "Paid";
                payment.UpdatedAt = DateTime.UtcNow;

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order != null)
                {
                    order.Status = "Paid";
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Payment confirmed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while confirming payment.", error = ex.Message });
            }
        }
    }
}
