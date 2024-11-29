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
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrderController(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrderBy([FromBody] CreateOrderDTO dto)
        {
            try
            {
                // Tạo đơn hàng mới
                var order = new Order
                {
                    CustomerId = dto.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = dto.TotalAmount,
                    Status = "Pending", // Trạng thái mặc định là chờ xác nhận
                    AddressId = dto.AddressId
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Tạo Payment để theo dõi trạng thái thanh toán
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethod = "VNPay",
                    PaymentStatus = "Pending",
                    Amount = dto.TotalAmount,
                    CreatedAt = DateTime.UtcNow
                };

                order.PaymentId = payment.PaymentId;

                _context.Payments.Add(payment);
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
