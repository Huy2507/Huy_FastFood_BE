using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Hubs;
using Huy_FastFood_BE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Huy_FastFood_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DelivererController : ControllerBase
    {
        private readonly IHubContext<DeliveryOrderHub> _hubContext;
        private readonly AppDbContext _context;

        public DelivererController(IHubContext<DeliveryOrderHub> hubContext, AppDbContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }

        // API 1: Hiển thị các đơn hàng có trạng thái là "done"
        [HttpGet("orders")]
        public async Task<IActionResult> GetCompletedOrders()
        {
            var completedOrders = await _context.Orders
                .Where(o => o.Status == "Done")
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

            if (completedOrders == null || completedOrders.Count == 0)
            {
                return NotFound(new { Message = "No completed orders found." });
            }

            return Ok(completedOrders);
        }
        [HttpPost("accept/{orderId}")]
        public async Task<IActionResult> AcceptOrder(int orderId)
        {
            // Lấy account_id từ JWT token
            var accountIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (accountIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token or user not authenticated." });
            }

            var accountId = int.Parse(accountIdClaim.Value);

            // Tìm đơn hàng tương ứng với orderId
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Food) // Bao gồm thông tin về món ăn
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.Status == "Done");

            if (order == null)
            {
                return NotFound(new { Message = "Order not found or already in delivery." });
            }

            // Tìm deliverer tương ứng với accountId
            var deliverer = await _context.Deliverers
                .FirstOrDefaultAsync(d => d.AccountId == accountId);

            if (deliverer == null)
            {
                return NotFound(new { Message = "Deliverer not found." });
            }

            // Tạo một đối tượng Delivery mới
            var delivery = new Delivery
            {
                OrderId = orderId,
                DelivererId = deliverer.DelivererId,
                AcceptedAt = DateTime.Now,
                Status = "Is Delivering"
            };

            _context.Deliveries.Add(delivery);

            // Cập nhật trạng thái đơn hàng
            order.Status = "Is Delivering";

            // Lưu thay đổi vào database
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("RefreshDeliveryOrderList");

            await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate");

            // Tạo DTO để trả về
            var acceptedOrder = new OrderDTO
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Note = order.Note,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
                {
                    FoodId = oi.FoodId,
                    ImageUrl = oi.Food.ImageUrl,
                    FoodName = oi.Food.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    TotalPrice = oi.TotalPrice
                }).ToList() // Đừng quên ToList()
            };

            return Ok(new { Message = "Order accepted successfully.", Data = acceptedOrder });
        }



        // API 3: Người giao hàng hoàn thành đơn hàng
        [HttpPost("complete/{orderId}")]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            // Tìm delivery dựa trên orderId
            var delivery = await _context.Deliveries
                .FirstOrDefaultAsync(d => d.OrderId == orderId && d.Status == "Is Delivering");

            if (delivery == null)
            {
                return NotFound(new { Message = "Delivery not found or already completed." });
            }

            // Tìm order dựa trên orderId
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.Status == "Is Delivering");

            if (order == null)
            {
                return NotFound(new { Message = "Order not found or not in progress." });
            }

            // Cập nhật trạng thái của delivery và order
            delivery.CompletedAt = DateTime.UtcNow;
            delivery.Status = "Completed";
            order.Status = "Completed";

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Đã hoàn thành giao hàng thành công!",
            });
        }


        // GET: api/Deliverer/CurrentOrder
        [HttpGet("CurrentOrder")]
        public async Task<IActionResult> GetDelivererCurrentOrder()
        {
            // Lấy accountId từ JWT token
            var accountIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (accountIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token or user not authenticated." });
            }

            var accountId = int.Parse(accountIdClaim.Value);

            // Tìm deliverer tương ứng với accountId
            var deliverer = await _context.Deliverers.FirstOrDefaultAsync(d => d.AccountId == accountId);
            if (deliverer == null)
            {
                return NotFound(new { message = "Deliverer not found." });
            }

            // Tìm đơn hàng mà deliverer đang giao
            var delivery = await _context.Deliveries
                .Where(d => d.DelivererId == deliverer.DelivererId && d.Status == "Is Delivering")
                .Include(d => d.Order)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Food)
                .FirstOrDefaultAsync();

            if (delivery == null)
            {
                return NotFound(new { message = "No active delivery found for this deliverer." });
            }

            // Chuẩn bị dữ liệu phản hồi
            var response = new
            {
                delivery.DeliveryId,
                delivery.Order.OrderId,
                delivery.Order.Status,
                delivery.Order.OrderDate,
                delivery.Order.TotalAmount,
                delivery.Order.Note,
                OrderItems = delivery.Order.OrderItems.Select(oi => new
                {
                    FoodId = oi.FoodId,
                    ImageUrl = oi.Food.ImageUrl,
                    FoodName = oi.Food.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };

            return Ok(response);
        }

    }
}
