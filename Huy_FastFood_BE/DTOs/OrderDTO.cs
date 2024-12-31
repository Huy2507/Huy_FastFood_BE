using Huy_FastFood_BE.Models;

namespace Huy_FastFood_BE.DTOs
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public string? Note { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; } = new();
    }

    public class OrderDetailsDTO
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public string? Note { get; set; }
        public List<PaymentDTO> Payment { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; } = new();
    }

    public class CreateOrderDTO
    {
        public int? AddressId { get; set; }
        public string? Note { get; set; }
        public string? PaymentMethod { get; set; }
    }
}
