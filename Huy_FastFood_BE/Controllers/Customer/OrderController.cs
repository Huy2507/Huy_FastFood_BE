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
                    OrderDate = DateTime.Now,
                    TotalAmount = (decimal)cart.TotalPrice,
                    Status = "Pending", // Trạng thái mặc định là chờ xác nhận
                    AddressId = dto.AddressId,
                    Note = dto.Note
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
                    CreatedAt = DateTime.Now
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
                payment.UpdatedAt = DateTime.Now;

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

        [Authorize(Roles = "Customer")]
        [HttpGet("orders")]
        public async Task<IActionResult> GetCustomerOrders()
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

                // Lấy danh sách đơn hàng của khách hàng
                var orders = await _context.Orders
                    .Where(o => o.CustomerId == customer.CustomerId)
                    .OrderByDescending(o => o.OrderDate) // Sắp xếp theo ngày đặt giảm dần
                    .Select(o => new
                    {
                        OrderId = o.OrderId,
                        OrderDate = o.OrderDate,
                        Status = o.Status, // Trạng thái đơn hàng
                        TotalPrice = o.TotalAmount,
                        Note = o.Note, // Tổng giá trị
                        DeliveryAddress = new // Địa chỉ giao hàng
                        {
                            AddressId = o.Address.Id,
                            Street = o.Address.Street,
                            City = o.Address.City,
                            District = o.Address.District,
                            Ward = o.Address.Ward,
                            FullAddress = $"{o.Address.Street}, {o.Address.Ward}, {o.Address.District}, {o.Address.City}"
                        },
                        Items = o.OrderItems.Select(oi => new
                        {
                            FoodId = oi.FoodId,
                            ImageUrl = oi.Food.ImageUrl,
                            FoodName = oi.Food.Name,
                            Quantity = oi.Quantity,
                            Price = oi.Food.Price,
                            TotalPrice = oi.TotalPrice
                        }).ToList() // Chi tiết món ăn trong đơn hàng
                    })
                    .ToListAsync();

                if (!orders.Any())
                {
                    return NotFound(new { message = "No orders found for this customer." });
                }

                return Ok(new { message = "Orders retrieved successfully.", data = orders });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving orders.", error = ex.Message });
            }
        }

    }
}
