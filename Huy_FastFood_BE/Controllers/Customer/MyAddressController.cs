using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Huy_FastFood_BE.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyAddressController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MyAddressController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated." });
                }

                var accountId = int.Parse(userIdClaim.Value);

                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == accountId);
                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found." });
                }

                var addresses = await _context.Addresses
                    .Where(a => a.CustomerId == customer.CustomerId)
                    .Select(a => new AddressDTO
                    {
                        Id = a.Id,
                        Street = a.Street,
                        Ward = a.Ward,
                        District = a.District,
                        City = a.City,
                        IsDefault = a.IsDefault
                    })
                    .ToListAsync();

                return Ok(addresses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving addresses.", error = ex.Message });
            }
        }

        [HttpGet("addresses/{id}")]
        public async Task<IActionResult> GetAddressById(int id)
        {
            try
            {
                var address = await _context.Addresses.FindAsync(id);
                if (address == null)
                {
                    return NotFound(new { message = "Address not found." });
                }

                var result = new AddressDTO
                {
                    Id = address.Id,
                    Street = address.Street,
                    Ward = address.Ward,
                    District = address.District,
                    City = address.City,
                    IsDefault = address.IsDefault
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the address.", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("addresses")]
        public async Task<IActionResult> AddAddress([FromBody] AddressDTO dto)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated." });
                }

                var accountId = int.Parse(userIdClaim.Value);

                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == accountId);
                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found." });
                }

                if (dto.IsDefault)
                {
                    // Đặt tất cả địa chỉ khác của khách hàng thành không mặc định
                    var existingAddresses = await _context.Addresses
                        .Where(a => a.CustomerId == customer.CustomerId && a.IsDefault)
                        .ToListAsync();

                    foreach (var address in existingAddresses)
                    {
                        address.IsDefault = false;
                    }
                }

                var newAddress = new Address
                {
                    CustomerId = customer.CustomerId,
                    Street = dto.Street,
                    Ward = dto.Ward,
                    District = dto.District,
                    City = dto.City,
                    IsDefault = dto.IsDefault
                };

                _context.Addresses.Add(newAddress);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Address added successfully."});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the address.", error = ex.Message });
            }
        }

        [HttpPut("addresses/{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] AddressDTO dto)
        {
            try
            {
                var address = await _context.Addresses.FindAsync(id);
                if (address == null)
                {
                    return NotFound(new { message = "Address not found." });
                }

                if (dto.IsDefault)
                {
                    // Đặt tất cả địa chỉ khác của khách hàng thành không mặc định
                    var existingAddresses = await _context.Addresses
                        .Where(a => a.CustomerId == address.CustomerId && a.Id != id && a.IsDefault)
                        .ToListAsync();

                    foreach (var addr in existingAddresses)
                    {
                        addr.IsDefault = false;
                    }
                }

                address.Street = dto.Street ?? address.Street;
                address.Ward = dto.Ward ?? address.Ward;
                address.District = dto.District ?? address.District;
                address.City = dto.City ?? address.City;
                address.IsDefault = dto.IsDefault;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Address updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the address.", error = ex.Message });
            }
        }

        [HttpDelete("addresses/{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            try
            {
                var address = await _context.Addresses.FindAsync(id);
                if (address == null)
                {
                    return NotFound(new { message = "Address not found." });
                }

                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Address deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the address.", error = ex.Message });
            }
        }
    }
}
