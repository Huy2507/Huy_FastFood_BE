using Microsoft.AspNetCore.Mvc;
using Huy_FastFood_BE.Services;
using Microsoft.EntityFrameworkCore;
using Huy_FastFood_BE.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;
using Huy_FastFood_BE.DTOs;
using System.Text.Json;

namespace Huy_FastFood_BE.Controllers
{
    [ApiController]
    [Route("api/v1/vnpay")]
    public class VNPayController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly VNPayService _vnPayService;

        public VNPayController(VNPayService vnPayService, AppDbContext context)
        {
            _vnPayService = vnPayService;
            _context = context;
        }

        [HttpPost("create-payment")]
        public IActionResult CreatePayment(decimal amount, string orderInfo)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var paymentUrl = _vnPayService.CreatePaymentUrl(amount, orderInfo, ipAddress);
                return Ok(new { PaymentUrl = paymentUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // API xử lý dữ liệu trả về từ VNPay
        [HttpGet("payment-return")]
        public async Task<IActionResult> PaymentReturn([FromQuery] Dictionary<string, string> vnpData)
        {
            try
            {
                // Log all incoming query parameters for debugging
                foreach (var key in vnpData.Keys)
                {
                    Console.WriteLine($"Key: {key}, Value: {vnpData[key]}");
                }

                // Check if vnp_SecureHash is present in the data
                if (!vnpData.ContainsKey("vnp_SecureHash"))
                {
                    return BadRequest(new { message = "Missing vnp_SecureHash." });
                }

                // Validate the return data (check HMAC SHA-512 signature)
                var isValid = _vnPayService.ValidateReturn(vnpData);

                if (!isValid)
                {
                    return BadRequest(new { message = "Invalid payment response from VNPay." });
                }

                // Extract the transaction details from the return data
                var txnRef = vnpData["vnp_TxnRef"];
                var paymentStatus = vnpData["vnp_TransactionStatus"];
                var amount = decimal.Parse(vnpData["vnp_Amount"]) / 100; // Convert from VND to decimal amount
                var orderId = vnpData["vnp_OrderInfo"];

                // Find the order from the database
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId.ToString() == orderId);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found." });
                }

                // Find the associated payment record
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == order.OrderId);
                if (payment == null)
                {
                    return NotFound(new { message = "Payment record not found." });
                }

                // Update the payment status based on the VNPay response
                payment.PaymentMethod = "VNPay";
                payment.PaymentStatus = paymentStatus == "00" ? "Completed" : "Failed";
                payment.TransactionId = vnpData["vnp_TransactionNo"];
                payment.TxnRef = txnRef;
                payment.UpdatedAt = DateTime.Now;

                // Xóa giỏ hàng sau khi tạo đơn hàng
                if (payment.PaymentStatus == "Completed")
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == order.CustomerId);
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
                    _context.CartItems.RemoveRange(cart.CartItems);
                    _context.Carts.Remove(cart);
                }


                // Save changes to the database
                await _context.SaveChangesAsync();

                return Redirect($"http://localhost:5173/payment-status?status={(payment.PaymentStatus == "Completed" ? "completed" : "Failed")}");

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing the payment.", error = ex.Message });
            }
        }

        [HttpPost("refund")]
        public async Task<IActionResult> RefundOrder([FromBody] RefundRequest request)
        {
            try
            {
                // Lấy thông tin payment dựa trên orderId
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == request.OrderId);
                if (payment == null)
                {
                    return NotFound(new { message = "Payment không tồn tại" });
                }

                // Tạo URL refund qua VNPayService
                var refundUrl = _vnPayService.CreateRefundUrl(
                    payment.TxnRef ?? string.Empty,
                    payment.TransactionId,
                    payment.Amount,
                    $"Refund for Order #{request.OrderId}",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1"
                );

                // Log URL để kiểm tra
                Console.WriteLine($"Refund URL: {refundUrl}");

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(refundUrl);

                // Cập nhật trạng thái Payment và Order
                payment.RefundStatus = "Refunded";
                payment.UpdatedAt = DateTime.Now;

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == request.OrderId);
                if (order != null)
                {
                    order.Status = "Refunded";
                };

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (HttpRequestException httpEx)
            {
                return StatusCode(500, new
                {
                    message = "Lỗi HTTP khi gửi yêu cầu refund",
                    error = httpEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Lỗi xử lý refund",
                    error = ex.Message
                });
            }
        }





    }
}
