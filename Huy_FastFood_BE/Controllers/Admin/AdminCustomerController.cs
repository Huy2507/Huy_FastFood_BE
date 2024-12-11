using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Huy_FastFood_BE.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminCustomerController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public AdminCustomerController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/AdminCustomer
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _dbContext.Customers.AsQueryable();

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(c =>
                        c.Name.Contains(search) ||
                        c.Phone.Contains(search) ||
                        c.Email.Contains(search));
                }

                // Pagination
                var totalRecords = await query.CountAsync();
                var customers = await query
                    .Include(c => c.Addresses)
                    .Include(c => c.Orders)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var customerDTOs = customers.Select(c => new CustomerDTO
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Phone = c.Phone,
                    Email = c.Email,
                    Addresses = c.Addresses.Select(a => new AddressDTO
                    {
                        Id = a.Id,
                        Street = a.Street,
                        Ward = a.Ward,
                        District = a.District,
                        City = a.City,
                        IsDefault = a.IsDefault,
                    }).ToList(),
                    Orders = c.Orders.Select(o => new OrderDTO
                    {
                        OrderId = o.OrderId,
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status
                    }).ToList()
                });

                return Ok(new
                {
                    TotalRecords = totalRecords,
                    Page = page,
                    PageSize = pageSize,
                    Data = customerDTOs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // GET: api/AdminCustomer/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerDetails(int id)
        {
            try
            {
                var customer = await _dbContext.Customers
                    .Include(c => c.Addresses)
                    .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Food)
                    .Include(c => c.Carts)
                    .ThenInclude(cart => cart.CartItems)
                    .ThenInclude(ci => ci.Food)
                    .FirstOrDefaultAsync(c => c.CustomerId == id);

                if (customer == null)
                    return NotFound(new { message = "Customer not found." });

                var customerDTO = new CustomerDTO
                {
                    CustomerId = customer.CustomerId,
                    Name = customer.Name,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    Addresses = customer.Addresses.Select(a => new AddressDTO
                    {
                        Id = a.Id,
                        Street = a.Street,
                        Ward = a.Ward,
                        District = a.District,
                        City = a.City,
                        IsDefault = a.IsDefault,
                    }).ToList(),
                    Orders = customer.Orders.Select(o => new OrderDTO
                    {
                        OrderId = o.OrderId,
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status
                    }).ToList()
                };

                return Ok(customerDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // GET: api/AdminCustomer/{id}/orders
        [HttpGet("{id}/orders")]
        public async Task<IActionResult> GetCustomerOrders(int id)
        {
            try
            {
                var orders = await _dbContext.Orders
                    .Where(o => o.CustomerId == id)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Food)
                    .ToListAsync();

                if (!orders.Any())
                    return NotFound(new { message = "No orders found for this customer." });

                var orderDTOs = orders.Select(o => new OrderDTO
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    Note = o.Note
                });

                return Ok(orderDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // GET: api/AdminCustomer/{id}/carts
        [HttpGet("{id}/carts")]
        public async Task<IActionResult> GetCustomerCarts(int id)
        {
            try
            {
                var carts = await _dbContext.Carts
                    .Where(c => c.CustomerId == id)
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Food)
                    .ToListAsync();

                if (!carts.Any())
                    return NotFound(new { message = "No carts found for this customer." });

                var cartDTOs = carts.Select(cart => new CartDTO
                {
                    CartId = cart.CartId,
                    CreatedAt = cart.CreatedAt,
                    CartItems = cart.CartItems.Select(item => new CartItemDTO
                    {
                        CartItemId = item.CartItemId,
                        FoodId = item.FoodId,
                        Quantity = item.Quantity
                    }).ToList()
                });

                return Ok(cartDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // GET: api/AdminCustomer/export
        [HttpGet("export")]
        public async Task<IActionResult> ExportCustomerData()
        {
            try
            {
                var customers = await _dbContext.Customers
                    .Include(c => c.Addresses)
                    .Include(c => c.Orders)
                    .ToListAsync();

                var csvData = "CustomerId,Name,Phone,Email\n";
                foreach (var c in customers)
                {
                    csvData += $"{c.CustomerId},{c.Name},{c.Phone},{c.Email}\n";
                }

                var byteArray = System.Text.Encoding.UTF8.GetBytes(csvData);
                return File(byteArray, "text/csv", "Customers.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        // GET: api/AdminCustomer/{id}/payments
        [HttpGet("{id}/payments")]
        public async Task<IActionResult> GetCustomerPayments(int id)
        {
            try
            {
                var payments = await _dbContext.Payments
                    .Where(p => p.Order.CustomerId == id)
                    .Include(p => p.Order)
                    .ToListAsync();

                if (!payments.Any())
                    return NotFound(new { message = "No payments found for this customer." });

                var paymentDTOs = payments.Select(p => new PaymentDTO
                {
                    PaymentId = p.PaymentId,
                    OrderId = p.OrderId,
                    PaymentMethod = p.PaymentMethod,
                    PaymentStatus = p.PaymentStatus,
                    TransactionId = p.TransactionId,
                    Amount = p.Amount,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });

                return Ok(paymentDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // GET: api/AdminCustomer/{id}/orders/payments
        [HttpGet("{id}/orders/payments")]
        public async Task<IActionResult> GetCustomerOrderPayments(int id)
        {
            try
            {
                var ordersWithPayments = await _dbContext.Orders
                    .Where(o => o.CustomerId == id)
                    .Include(o => o.Payments)
                    .ToListAsync();

                if (!ordersWithPayments.Any())
                    return NotFound(new { message = "No orders with payments found for this customer." });

                var orderPaymentsDTOs = ordersWithPayments.Select(o => new OrderPaymentsDTO
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    Payments = o.Payments.Select(p => new PaymentDTO
                    {
                        PaymentId = p.PaymentId,
                        PaymentMethod = p.PaymentMethod,
                        PaymentStatus = p.PaymentStatus,
                        TransactionId = p.TransactionId,
                        Amount = p.Amount,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    }).ToList()
                });

                return Ok(orderPaymentsDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
