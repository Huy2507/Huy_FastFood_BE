using Azure.Core;
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
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;

        public AuthController(AppDbContext context, IConfiguration configuration, IEmailService emailService, ITokenService tokenService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _tokenService = tokenService;
        }

        // POST: api/auth/login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                if (loginDTO == null || string.IsNullOrEmpty(loginDTO.Username) || string.IsNullOrEmpty(loginDTO.Password))
                {
                    return BadRequest(new { message = "Yêu cầu không hợp lệ" });
                }

                // Kiểm tra tài khoản trong cơ sở dữ liệu
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Username == loginDTO.Username);

                if (account == null || !VerifyPassword(loginDTO.Password, account.Password))
                {
                    return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không đúng" });
                }

                if (!account.IsActive)
                {
                    return BadRequest(new { message = "Tài khoản đã bị khóa" });
                }

                // Lấy danh sách vai trò của tài khoản
                var roles = await _context.UserRoles
                                          .Where(ur => ur.AccountId == account.AccountId)
                                          .Select(ur => ur.Role.RoleName)
                                          .ToListAsync();

                // Tạo Access Token
                var accessToken = _tokenService.GenerateAccessToken(account, roles);

                // Kiểm tra nếu vai trò là Customer, tạo Refresh Token
                if (roles.Contains("Customer"))
                {
                    var refreshToken = _tokenService.GenerateRefreshToken(account.AccountId, "Customer");
                    _context.RefreshTokens.Add(refreshToken);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken.Token
                    });
                }

                // Nếu không phải Customer, chỉ trả về Access Token
                return Ok(new
                {
                    AccessToken = accessToken
                });
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

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO request)
        {
            if (!await _tokenService.ValidateRefreshToken(request.RefreshToken, request.UserId))
            {
                return Unauthorized("Invalid or expired refresh token");
            }

            var account = await _context.Accounts.FindAsync(request.UserId);
            if (account == null)
            {
                return Unauthorized("Invalid user");
            }

            var roles = await _context.UserRoles.Where(ur => ur.AccountId == account.AccountId).Select(ur => ur.Role.RoleName).ToListAsync();
            var newAccessToken = _tokenService.GenerateAccessToken(account, roles);

            return Ok(new { AccessToken = newAccessToken });
        }

        [HttpPost("verify-access-token")]
        public IActionResult VerifyAccessToken([FromBody] TokenRequest tokenRequest)
        {
            var token = tokenRequest.AccessToken;

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Access token is required." });
            }

            try
            {
                // Lấy khóa bí mật từ cấu hình
                var secretKey = _configuration["Jwt:Key"];
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                // Các tham số để xác thực token
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero // Loại bỏ thời gian chênh lệch mặc định
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

                // Kiểm tra nếu token không phải là JWT
                if (validatedToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return Unauthorized(new { message = "Invalid token format." });
                }

                // Lấy thông tin từ claims
                var userId = principal.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var roles = principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

                return Ok(new
                {
                    message = "Access token is valid.",
                    userId = userId,
                    roles = roles
                });
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new { message = "Access token has expired." });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = "Invalid token.", error = ex.Message });
            }
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDTO request)
        {
            await _tokenService.RevokeRefreshToken(request.RefreshToken);
            return Ok("Logged out successfully");
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            try
            {
                if (registerDTO.Password.Length < 6)
                {
                    return BadRequest("Mật khẩu phải có ít nhất 6 ký tự.");
                }

                // Kiểm tra xác nhận mật khẩu
                if (registerDTO.Password != registerDTO.ConfirmPassword)
                {
                    return BadRequest(new { message = "Mật khẩu không trùng khớp!" });
                }

                // Kiểm tra trùng lặp tên tài khoản
                if (_context.Accounts.Any(a => a.Username == registerDTO.Username))
                {
                    return BadRequest(new { message = "Tên tài khoản đã tồn tại!" });
                }

                // Kiểm tra trùng lặp email
                if (_context.Customers.Any(c => c.Email == registerDTO.Email))
                {
                    return BadRequest(new { message = "Email đã tồn tại!" });
                }

                // Mã hóa mật khẩu
                var hashedPassword = PasswordHasher.HashPassword(registerDTO.Password);

                // Tạo tài khoản mới
                var account = new Account
                {
                    Username = registerDTO.Username,
                    Password = hashedPassword,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                // Thêm tài khoản vào database
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                // Liên kết tài khoản với thông tin khách hàng
                var customer = new Models.Customer
                {
                    AccountId = account.AccountId,
                    Name = registerDTO.Name,
                    Phone = registerDTO.Phone,
                    Email = registerDTO.Email,
                    CreatedAt = DateTime.Now
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

                return Ok(new { message = "Tài khoản đã được tạo thành công!" });
            }
            catch (Exception ex)
            {
                // Ghi log nếu cần thiết
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            try
            {
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Customers.Any(c => c.Email == request.Email));

                if (account == null)
                    return NotFound("Email không tồn tại trong hệ thống.");

                // Tạo mã reset
                var resetCode = new Random().Next(100000, 999999).ToString();
                account.PasswordResetCode = resetCode;
                account.ResetCodeExpiration = DateTime.Now.AddMinutes(2);

                await _context.SaveChangesAsync();

                // Gửi email
                var emailBody = $"Mã đặt lại mật khẩu của bạn là: {resetCode}. Mã này có hiệu lực trong 2 phút.";
                await _emailService.SendEmailAsync(request.Email, "Reset Password Code", emailBody);

                return Ok("Email reset mật khẩu đã được gửi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeDTO request)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a =>
                a.PasswordResetCode == request.ResetCode &&
                a.ResetCodeExpiration > DateTime.Now &&
                a.Customers.Any(c => c.Email == request.Email)); // Check email match

            if (account == null)
            {
                return BadRequest("Email hoặc mã xác nhận không đúng.");
            }

            return Ok("Mã xác nhận đúng.");
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO request)
        {
            // Kiểm tra độ dài mật khẩu
            if (request.NewPassword.Length < 6)
            {
                return BadRequest("Mật khẩu phải có ít nhất 6 ký tự.");
            }

            // Kiểm tra mật khẩu xác nhận
            if (request.ConfirmNewPassword != request.NewPassword)
            {
                return BadRequest("Mật khẩu không trùng khớp.");
            }

            // Tìm tài khoản theo email và reset code
            var account = await _context.Accounts.FirstOrDefaultAsync(a =>
                a.Customers.Any(c => c.Email == request.Email) &&
                a.PasswordResetCode == request.ResetCode &&
                a.ResetCodeExpiration > DateTime.Now);

            if (account == null)
            {
                return BadRequest("Email hoặc mã xác thực không hợp lệ.");
            }

            // Hash mật khẩu mới và cập nhật tài khoản
            account.Password = PasswordHasher.HashPassword(request.NewPassword);
            account.PasswordResetCode = null; // Xóa mã xác thực
            account.ResetCodeExpiration = null;
            account.UpdatedAt = DateTime.Now;

            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return Ok("Mật khẩu đã được thay đổi.");
        }


    }
}
