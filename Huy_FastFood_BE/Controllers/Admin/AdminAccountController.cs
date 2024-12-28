using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Huy_FastFood_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Huy_FastFood_BE.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminAccountController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public AdminAccountController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/Account
        [HttpGet]
        public async Task<IActionResult> GetAllAccounts(
            string? search, // Search parameter for both AccountId and Username
            bool? isActive,
            string? role)
        {
            try
            {
                var query = _dbContext.Accounts
                    .Include(a => a.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .AsQueryable();

                // If 'search' parameter is provided, filter by AccountId or Username
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(a => a.AccountId.ToString().Contains(search) || a.Username.Contains(search));
                }

                // Filter by IsActive if provided
                if (isActive.HasValue)
                {
                    query = query.Where(a => a.IsActive == isActive.Value);
                }

                // Filter by Role if provided
                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(a => a.UserRoles.Any(ur => ur.Role.RoleName.Contains(role)));
                }

                var accounts = await query
                    .Select(a => new AccountDTO
                    {
                        AccountId = a.AccountId,
                        Username = a.Username,
                        IsActive = a.IsActive,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt,
                        Roles = a.UserRoles.Select(ur => ur.Role.RoleName).ToList()
                    })
                    .ToListAsync();

                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        // GET: api/Account/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            try
            {
                var account = await _dbContext.Accounts
                    .Include(a => a.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(a => a.AccountId == id);

                if (account == null)
                    return NotFound(new { message = "Account not found." });

                var accountDto = new AccountDetailDTO
                {
                    AccountId = account.AccountId,
                    Username = account.Username,
                    IsActive = account.IsActive,
                    CreatedAt = account.CreatedAt,
                    UpdatedAt = account.UpdatedAt,
                    Roles = account.UserRoles.Select(ur => ur.Role.RoleId).ToList()
                };

                return Ok(accountDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // POST: api/Account
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDTO dto)
        {
            try
            {
                if (await _dbContext.Accounts.AnyAsync(a => a.Username == dto.Username))
                    return BadRequest(new { message = "Username already exists." });

                var account = new Account
                {
                    Username = dto.Username,
                    Password = PasswordHasher.HashPassword(dto.Password),
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                foreach (var roleId in dto.RoleIds)
                {
                    var role = await _dbContext.Roles.FindAsync(roleId);
                    if (role != null)
                    {
                        account.UserRoles.Add(new UserRole { RoleId = roleId });
                    }
                }

                await _dbContext.Accounts.AddAsync(account);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAllAccounts), new { id = account.AccountId }, new { message = "Account created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // PUT: api/Account/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateAccountDTO dto)
        {
            try
            {
                var account = await _dbContext.Accounts
                    .Include(a => a.UserRoles)
                    .FirstOrDefaultAsync(a => a.AccountId == id);

                if (account == null)
                    return NotFound(new { message = "Account not found." });

                // Cập nhật thông tin tài khoản
                account.Password = PasswordHasher.HashPassword(dto.Password);
                account.IsActive = dto.IsActive;
                account.UpdatedAt = DateTime.Now;

                // Danh sách vai trò hiện tại
                var currentRoleIds = account.UserRoles.Select(ur => ur.RoleId).ToList();

                // Xóa các vai trò không còn tồn tại
                var rolesToRemove = account.UserRoles.Where(ur => !dto.RoleIds.Contains(ur.RoleId)).ToList();
                _dbContext.UserRoles.RemoveRange(rolesToRemove);

                // Thêm các vai trò mới
                var rolesToAdd = dto.RoleIds.Except(currentRoleIds).ToList();
                foreach (var roleId in rolesToAdd)
                {
                    var role = await _dbContext.Roles.FindAsync(roleId);
                    if (role != null)
                    {
                        account.UserRoles.Add(new UserRole { RoleId = roleId });
                    }
                }

                _dbContext.Accounts.Update(account);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Account updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        // DELETE: api/Account/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                var account = await _dbContext.Accounts.FindAsync(id);
                if (account == null)
                    return NotFound(new { message = "Account not found." });

                _dbContext.Accounts.Remove(account);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Account deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
