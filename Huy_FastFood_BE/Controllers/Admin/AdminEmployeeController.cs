using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Huy_FastFood_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Huy_FastFood_BE.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminEmployeeController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public AdminEmployeeController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/Employee
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees([FromQuery] string? search, [FromQuery] bool? isActive)
        {
            try
            {
                // Truy vấn cơ bản ban đầu
                var query = _dbContext.Employees
                    .Include(e => e.Account)
                    .AsQueryable();

                // Áp dụng bộ lọc theo trạng thái hoạt động nếu được truyền vào
                if (isActive.HasValue)
                {
                    query = query.Where(e => e.IsActive == isActive.Value);
                }

                // Thực hiện truy vấn và chuyển sang xử lý phía client
                var employees = await query.ToListAsync();

                // Áp dụng tìm kiếm phía client nếu chuỗi tìm kiếm được truyền vào
                if (!string.IsNullOrEmpty(search))
                {
                    employees = employees
                        .Where(e =>
                            e.EmployeeId.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            (!string.IsNullOrEmpty(e.Name) && e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(e.Position) && e.Position.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(e.Phone) && e.Phone.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(e.Email) && e.Email.Contains(search, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }

                // Ánh xạ dữ liệu sang DTO
                var employeeDTOs = employees.Select(e => new EmployeeDTO
                {
                    EmployeeId = e.EmployeeId,
                    AccountId = e.AccountId,
                    Username = e.Account?.Username,
                    Name = e.Name,
                    Position = e.Position,
                    Phone = e.Phone,
                    Email = e.Email,
                    HireDate = e.HireDate,
                    LeaveDate = e.LeaveDate,
                    IsActive = e.IsActive,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                });

                return Ok(employeeDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }




        // GET: api/Employee/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            try
            {
                var employee = await _dbContext.Employees
                    .Include(e => e.Account)
                    .FirstOrDefaultAsync(e => e.EmployeeId == id);

                if (employee == null)
                    return NotFound(new { message = "Employee not found." });

                var account = await _dbContext.Accounts.Where(a => a.AccountId == employee.AccountId)
                    .Include(a => a.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync();

                var employeeDTO = new EmployeeDTO
                {
                    EmployeeId = employee.EmployeeId,
                    AccountId = employee.AccountId,
                    Username = employee.Account?.Username,
                    Name = employee.Name,
                    Position = employee.Position,
                    RoleIds = account.UserRoles.Select(ur => ur.Role.RoleId).ToList(),
                    Phone = employee.Phone,
                    Email = employee.Email,
                    HireDate = employee.HireDate,
                    LeaveDate = employee.LeaveDate,
                    IsActive = employee.IsActive,
                    CreatedAt = employee.CreatedAt,
                    UpdatedAt = employee.UpdatedAt
                };

                return Ok(employeeDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        // POST: api/Employee
        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDTO employeeDTO)
        {
            try
            {
                if (employeeDTO.Password.Length < 6)
                {
                    return BadRequest("Mật khẩu phải có ít nhất 6 ký tự.");
                }

                var newAccount = new Account
                {
                    Username = employeeDTO.Username,
                    Password = PasswordHasher.HashPassword(employeeDTO.Password),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                foreach (var roleId in employeeDTO.RoleIds)
                {
                    var role = await _dbContext.Roles.FindAsync(roleId);
                    if (role != null)
                    {
                        newAccount.UserRoles.Add(new UserRole { RoleId = roleId });
                    }
                }

                await _dbContext.Accounts.AddAsync(newAccount);
                await _dbContext.SaveChangesAsync();

                // Nối các vai trò thành một chuỗi (ví dụ: "Admin, Manager, Developer")
                var roleNames = newAccount.UserRoles.Select(ur => ur.Role.RoleName).ToList();
                var positions = string.Join(", ", roleNames); // Nối các tên vai trò thành một chuỗi

                var newEmployee = new Employee
                {
                    AccountId = newAccount.AccountId,
                    Name = employeeDTO.Name,
                    Position = positions, // Gán chuỗi vai trò vào Position
                    Phone = employeeDTO.Phone,
                    Email = employeeDTO.Email,
                    HireDate = employeeDTO.HireDate,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _dbContext.Employees.AddAsync(newEmployee);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEmployeeById), new { id = newEmployee.EmployeeId }, new { message = "Employee and account created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        // PUT: api/Employee/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDTO employeeDTO)
        {
            try
            {
                var existingEmployee = await _dbContext.Employees.FindAsync(id);

                if (existingEmployee == null)
                    return NotFound(new { message = "Employee not found." });

                // Cập nhật các thông tin cơ bản của nhân viên
                existingEmployee.Name = employeeDTO.Name;
                existingEmployee.Phone = employeeDTO.Phone;
                existingEmployee.Email = employeeDTO.Email;
                existingEmployee.LeaveDate = employeeDTO.LeaveDate;
                existingEmployee.IsActive = employeeDTO.IsActive;
                existingEmployee.UpdatedAt = DateTime.Now;

                // Cập nhật vai trò của nhân viên
                var existingRoles = _dbContext.UserRoles.Where(ur => ur.AccountId == existingEmployee.AccountId).ToList();

                // Xóa vai trò cũ
                _dbContext.UserRoles.RemoveRange(existingRoles);

                // Tạo danh sách tên vai trò từ bảng Role
                List<string> newRoles = new List<string>();

                // Thêm vai trò mới và lấy danh sách tên vai trò
                foreach (var roleId in employeeDTO.RoleIds)
                {
                    var role = await _dbContext.Roles.FindAsync(roleId);
                    if (role != null)
                    {
                        _dbContext.UserRoles.Add(new UserRole { AccountId = (int)existingEmployee.AccountId, RoleId = roleId });
                        newRoles.Add(role.RoleName);  // Thêm tên vai trò vào danh sách
                    }
                }

                // Cập nhật trường Position với danh sách vai trò
                existingEmployee.Position = string.Join(", ", newRoles);  // Gộp các tên vai trò thành chuỗi

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Employee updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }




        // DELETE: api/Employee/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                var employee = await _dbContext.Employees.FindAsync(id);

                if (employee == null)
                    return NotFound(new { message = "Employee not found." });

                _dbContext.Employees.Remove(employee);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Employee deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
