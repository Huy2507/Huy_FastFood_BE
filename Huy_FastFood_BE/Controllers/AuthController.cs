using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Huy_FastFood_BE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Huy_FastFood_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/auth/login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                if (loginDTO == null || string.IsNullOrEmpty(loginDTO.Username) || string.IsNullOrEmpty(loginDTO.Password))
                {
                    return BadRequest(new { message = "Invalid login request" });
                }

                // Kiểm tra tài khoản trong cơ sở dữ liệu
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Username == loginDTO.Username);

                if (account == null || !VerifyPassword(loginDTO.Password, account.Password))
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }
             
                var token = GenerateJwtToken(account);

                return Ok(new{Token = token});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            // Ở đây, sử dụng mã hóa Hash để kiểm tra mật khẩu. Ví dụ với BCrypt:
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash);
        }

        private string GenerateJwtToken(Account account)
        {
            var roles = _context.UserRoles
                .Where(ur => ur.AccountId == account.AccountId)
                .Select(ur => ur.Role.RoleName)
                .ToList();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("AccountId", account.AccountId.ToString()),
                new Claim("UserName", account.Username),
            };

            // Thêm vai trò vào Claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            try
            {
                // Kiểm tra xác nhận mật khẩu
                if (registerDTO.Password != registerDTO.ConfirmPassword)
                {
                    return BadRequest(new { message = "Passwords do not match!" });
                }

                // Kiểm tra trùng lặp tên tài khoản
                if (_context.Accounts.Any(a => a.Username == registerDTO.Username))
                {
                    return BadRequest(new { message = "Username already exists!" });
                }

                // Kiểm tra trùng lặp email
                if (_context.Customers.Any(c => c.Email == registerDTO.Email))
                {
                    return BadRequest(new { message = "Email already exists!" });
                }

                // Mã hóa mật khẩu
                var hashedPassword = PasswordHasher.HashPassword(registerDTO.Password);

                // Tạo tài khoản mới
                var account = new Account
                {
                    Username = registerDTO.Username,
                    Password = hashedPassword,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Thêm tài khoản vào database
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                // Liên kết tài khoản với thông tin khách hàng
                var customer = new Customer
                {
                    AccountId = account.AccountId,
                    Name = registerDTO.Name,
                    Phone = registerDTO.Phone,
                    Email = registerDTO.Email,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Customers.Add(customer);

                // Gán role mặc định là "Customer"
                var role = _context.Roles.FirstOrDefault(r => r.RoleName == "Customer");
                if (role != null)
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        AccountId = account.AccountId,
                        RoleId = role.RoleId
                    });
                }

                // Lưu tất cả thay đổi
                await _context.SaveChangesAsync();

                return Ok(new { message = "Account created successfully!" });
            }
            catch (Exception ex)
            {
                // Ghi log nếu cần thiết
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }
    }
}
