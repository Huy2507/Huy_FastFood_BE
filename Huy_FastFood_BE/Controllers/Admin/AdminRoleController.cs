using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminRoleController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminRoleController(AppDbContext context)
    {
        _context = context;
    }

    // Lấy danh sách vai trò
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        try
        {
            var roles = await _context.Roles
                .Select(r => new RoleDTO
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .ToListAsync();

            return Ok(roles);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    // Lấy vai trò theo ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleById(int id)
    {
        try
        {
            var role = await _context.Roles
                .Where(r => r.RoleId == id)
                .Select(r => new RoleDTO
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return NotFound("Role not found.");
            }

            return Ok(role);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    // Thêm vai trò mới
    [HttpPost]
    public async Task<IActionResult> AddRole([FromBody] RoleDTO roleDTO)
    {
        try
        {
            if (roleDTO == null)
            {
                return BadRequest("Invalid data.");
            }

            var role = new Role
            {
                RoleName = roleDTO.RoleName
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            roleDTO.RoleId = role.RoleId;

            return CreatedAtAction(nameof(GetRoleById), new { id = roleDTO.RoleId }, roleDTO);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    // Cập nhật vai trò
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleDTO roleDTO)
    {
        try
        {
            if (roleDTO == null)
            {
                return BadRequest("Invalid data.");
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            role.RoleName = roleDTO.RoleName;
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            return Ok(role);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    // Xóa vai trò
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        try
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }
}
