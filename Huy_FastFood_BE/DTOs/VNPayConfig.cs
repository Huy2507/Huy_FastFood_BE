namespace Huy_FastFood_BE.DTOs
{
    public class VNPayConfig
    {
        public string TmnCode { get; set; }
        public string HashSecret { get; set; }
        public string Url { get; set; }
        public string RefundUrl { get; set; }
        public string ReturnUrl { get; set; }
    }
    public class RefundRequest
    {
        public int OrderId { get; set; } // ID đơn hàng
        public string? TransactionNo { get; set; } // Số giao dịch từ VNPay (nếu cần)
    }
}
