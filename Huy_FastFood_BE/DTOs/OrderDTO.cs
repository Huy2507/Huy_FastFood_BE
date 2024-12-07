namespace Huy_FastFood_BE.DTOs
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
    }

    public class CreateOrderDTO
    {
        public int? AddressId { get; set; }
    }
}
