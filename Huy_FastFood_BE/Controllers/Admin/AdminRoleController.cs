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

            return Ok(roles);  // Trả về mã 200 OK và dữ liệu
        }
        catch (Exception ex)
        {
            // Log lỗi (nếu cần)
            return StatusCode(500, "Internal server error: " + ex.Message);  // Trả về mã 500 nếu có lỗi
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
                return BadRequest("Invalid data.");  // Trả về mã 400 nếu dữ liệu không hợp lệ
            }

            var role = new Role
            {
                RoleName = roleDTO.RoleName
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            roleDTO.RoleId = role.RoleId;

            return CreatedAtAction(nameof(GetRoles), new { id = roleDTO.RoleId }, roleDTO);  // Trả về mã 201 Created
        }
        catch (Exception ex)
        {
            // Log lỗi (nếu cần)
            return StatusCode(500, "Internal server error: " + ex.Message);  // Trả về mã 500 nếu có lỗi
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
                return BadRequest("Invalid data.");  // Trả về mã 400 nếu dữ liệu không hợp lệ
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound("Role not found.");  // Trả về mã 404 nếu không tìm thấy vai trò
            }

            role.RoleName = roleDTO.RoleName;
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            return Ok(role);  // Trả về mã 200 OK và dữ liệu đã cập nhật
        }
        catch (Exception ex)
        {
            // Log lỗi (nếu cần)
            return StatusCode(500, "Internal server error: " + ex.Message);  // Trả về mã 500 nếu có lỗi
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
                return NotFound("Role not found.");  // Trả về mã 404 nếu không tìm thấy vai trò
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();  // Trả về mã 204 No Content sau khi xóa thành công
        }
        catch (Exception ex)
        {
            // Log lỗi (nếu cần)
            return StatusCode(500, "Internal server error: " + ex.Message);  // Trả về mã 500 nếu có lỗi
        }
    }
}
