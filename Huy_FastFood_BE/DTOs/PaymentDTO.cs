namespace Huy_FastFood_BE.DTOs
{
    public class PaymentDTO
    {
        public int PaymentId { get; set; }
        public int? OrderId { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string PaymentStatus { get; set; } = null!;
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    public class OrderPaymentsDTO
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public List<PaymentDTO> Payments { get; set; } = new List<PaymentDTO>();
    }
}
