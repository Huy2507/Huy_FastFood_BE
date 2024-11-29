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
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var employees = await _dbContext.Employees
                    .Include(e => e.Account) // Include related account if needed
                    .ToListAsync();

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
                    .Include(e => e.Account) // Include account details
                    .FirstOrDefaultAsync(e => e.EmployeeId == id);

                if (employee == null)
                    return NotFound(new { message = "Employee not found." });

                var employeeDTO = new EmployeeDTO
                {
                    EmployeeId = employee.EmployeeId,
                    AccountId = employee.AccountId,
                    Username = employee.Account?.Username,
                    Name = employee.Name,
                    Position = employee.Position,
                    Phone = employee.Phone,
                    Email = employee.Email,
                    HireDate = employee.HireDate,
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
                // Create Account
                var newAccount = new Account
                {
                    Username = employeeDTO.Username,
                    Password = PasswordHasher.HashPassword(employeeDTO.Password),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _dbContext.Accounts.AddAsync(newAccount);
                await _dbContext.SaveChangesAsync();

                // Create Employee
                var newEmployee = new Employee
                {
                    AccountId = newAccount.AccountId,
                    Name = employeeDTO.Name,
                    Position = employeeDTO.Position,
                    Phone = employeeDTO.Phone,
                    Email = employeeDTO.Email,
                    HireDate = employeeDTO.HireDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
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
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeDTO employeeDTO)
        {
            try
            {
                var existingEmployee = await _dbContext.Employees.FindAsync(id);

                if (existingEmployee == null)
                    return NotFound(new { message = "Employee not found." });

                existingEmployee.Name = employeeDTO.Name;
                existingEmployee.Position = employeeDTO.Position;
                existingEmployee.Phone = employeeDTO.Phone;
                existingEmployee.Email = employeeDTO.Email;
                existingEmployee.HireDate = employeeDTO.HireDate;
                existingEmployee.UpdatedAt = DateTime.UtcNow;

                _dbContext.Employees.Update(existingEmployee);
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
