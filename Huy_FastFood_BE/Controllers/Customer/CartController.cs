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
    [Authorize(Roles ="Customer")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CartController(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("cart-items")]
        public async Task<IActionResult> GetCartItems()
        {
            try
            {
                // Lấy account_id từ JWT token
                var accountIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (accountIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated." });
                }

                var accountId = int.Parse(accountIdClaim.Value);

                // Tìm customer_id từ account_id
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == accountId);
                if (customer == null)
                {
                    return BadRequest(new { message = "Customer not found for the given account." });
                }

                // Lấy các món ăn trong giỏ hàng
                var cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Food)
                        .AsNoTracking() // Thêm AsNoTracking nếu không cần theo dõi sự thay đổi của các entity này
                        .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

                if (cart == null || !cart.CartItems.Any())
                {
                    return Ok(new List<object>()); // Trả về danh sách trống nếu không có món ăn
                }
                var totalPrice = cart.TotalPrice;
                var cartItems = cart.CartItems.Select(ci => new
                {
                    id = ci.CartItemId,
                    ci.FoodId,
                    FoodName = ci.Food.Name,
                    ci.Food.Price,
                    ci.Food.ImageUrl,
                    ci.Quantity,
                    ci.TotalPrice,
                }).ToList();

                var result = new {totalPrice, cartItems};
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching cart items.", error = ex.Message });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO dto)
        {
            try
            {
                // Lấy account_id từ JWT token
                var accountIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (accountIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated." });
                }

                var accountId = int.Parse(accountIdClaim.Value);

                // Tra cứu customer_id từ account_id
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == accountId);
                if (customer == null)
                {
                    return BadRequest(new { message = "Customer not found for the given account." });
                }

                var customerId = customer.CustomerId;

                // Tìm hoặc tạo giỏ hàng
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        CustomerId = customerId,
                        CreatedAt = DateTime.Now,
                        CartItems = new List<CartItem>()
                    };
                    _context.Carts.Add(cart);
                }

                // Kiểm tra món ăn đã tồn tại trong giỏ chưa
                var food = await _context.Foods.FindAsync(dto.FoodId);
                var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.FoodId == dto.FoodId);
                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += dto.Quantity;
                    existingCartItem.TotalPrice = food.Price * existingCartItem.Quantity;
                    existingCartItem.UpdatedAt = DateTime.Now;
                    // Tính lại tổng giá trị của giỏ hàng
                    cart.TotalPrice = cart.CartItems.Sum(ci => ci.TotalPrice);

                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                else
                {
                    
                    if (food == null)
                    {
                        return NotFound(new { message = "Food not found." });
                    }

                    var newCartItem = new CartItem
                    {
                        FoodId = dto.FoodId,
                        Quantity = dto.Quantity,
                        CreatedAt = DateTime.Now
                    };
                    newCartItem.TotalPrice = food.Price * newCartItem.Quantity;
                    cart.CartItems.Add(newCartItem);
                }

                // Tính lại tổng giá trị của giỏ hàng
                cart.TotalPrice = cart.CartItems.Sum(ci => ci.TotalPrice);

                await _context.SaveChangesAsync();
                return Ok(new { message = "Food added to cart successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding food to cart.", error = ex.Message });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("decrease-quantity")]
        public async Task<IActionResult> DecreaseQuantity([FromBody] UpdateCartDTO dto)
        {
            try
            {
                // Lấy account_id từ JWT token
                var accountIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (accountIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated." });
                }

                var accountId = int.Parse(accountIdClaim.Value);

                // Tìm customer_id từ account_id
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == accountId);
                if (customer == null)
                {
                    return BadRequest(new { message = "Customer not found for the given account." });
                }
                // Tìm hoặc tạo giỏ hàng
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

                var food = await _context.Foods.FirstOrDefaultAsync(f => f.FoodId == dto.FoodId);

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.Cart.CustomerId == customer.CustomerId && ci.FoodId == dto.FoodId);

                if (cartItem == null)
                {
                    return NotFound(new { message = "Cart item not found." });
                }

                cartItem.Quantity -= dto.Quantity;
                cartItem.TotalPrice = cartItem.Quantity * food.Price;
                if (cartItem.Quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem); // Xóa nếu số lượng <= 0
                }

                // Tính lại tổng giá trị của giỏ hàng
                cart.TotalPrice = cart.CartItems.Sum(ci => ci.TotalPrice);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Quantity updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating quantity.", error = ex.Message });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpDelete("remove-item/{foodId}")]
        public async Task<IActionResult> RemoveCartItem(int foodId)
        {
            try
            {
                // Lấy account_id từ JWT token
                var accountIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (accountIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated." });
                }

                var accountId = int.Parse(accountIdClaim.Value);

                // Tìm customer_id từ account_id
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == accountId);
                if (customer == null)
                {
                    return BadRequest(new { message = "Customer not found for the given account." });
                }

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.Cart.CustomerId == customer.CustomerId && ci.FoodId == foodId);

                if (cartItem == null)
                {
                    return NotFound(new { message = "Cart item not found." });
                }

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

                // Tính lại tổng giá trị của giỏ hàng
                cart.TotalPrice = cart.CartItems.Sum(ci => ci.TotalPrice);

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cart item removed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing cart item.", error = ex.Message });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpDelete("clear-cart")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                // Lấy account_id từ JWT token
                var accountIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (accountIdClaim == null)
                {
                    return Unauthorized(new { message = "Invalid token or user not authenticated." });
                }

                var accountId = int.Parse(accountIdClaim.Value);

                // Tìm customer_id từ account_id
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountId == accountId);
                if (customer == null)
                {
                    return BadRequest(new { message = "Customer not found for the given account." });
                }

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

                if (cart == null || !cart.CartItems.Any())
                {
                    return Ok(new { message = "Cart is already empty." });
                }

                // Tính lại tổng giá trị của giỏ hàng
                cart.TotalPrice = cart.CartItems.Sum(ci => ci.TotalPrice);

                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cart cleared successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while clearing the cart.", error = ex.Message });
            }
        }
    }
}
