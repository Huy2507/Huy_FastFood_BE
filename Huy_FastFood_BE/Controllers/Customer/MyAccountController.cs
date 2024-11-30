using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Huy_FastFood_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Huy_FastFood_BE.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Customer")]
    public class MyAccountController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MyAccountController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("my-account")]
        public async Task<IActionResult> GetMyAccount()
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

                // Lấy thông tin khách hàng từ AccountId
                var customer = await _context.Customers
                    .Where(c => c.AccountId == accountId)
                    .Select(c => new MyAccountDTO
                    {
                        CustomerId = c.CustomerId,
                        Name = c.Name,
                        Email = c.Email,
                        Phone = c.Phone
                    })
                    .FirstOrDefaultAsync();

                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found." });
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving account information.", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("update-my-account")]
        public async Task<IActionResult> UpdateMyAccount([FromBody] MyAccountDTO dto)
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

                // Tìm khách hàng theo AccountId
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == accountId);
                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found." });
                }

                // Cập nhật thông tin
                customer.Name = dto.Name ?? customer.Name;
                customer.Email = dto.Email ?? customer.Email;
                customer.Phone = dto.Phone ?? customer.Phone;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Account updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating account.", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
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

                // Tìm tài khoản
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
                if (account == null)
                {
                    return NotFound(new { message = "Account not found." });
                }

                // Kiểm tra mật khẩu cũ
                if (!PasswordHasher.VerifyPassword(dto.OldPassword, account.Password))
                {
                    return BadRequest(new { message = "Old password is incorrect." });
                }

                if (dto.NewPassword != dto.ConfirmNewPassword)
                {
                    return BadRequest(new { message = "Confirm Password is not same as New Password" });
                }

                // Cập nhật mật khẩu mới
                account.Password = PasswordHasher.HashPassword(dto.NewPassword);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while changing password.", error = ex.Message });
            }
        }

    }
}
