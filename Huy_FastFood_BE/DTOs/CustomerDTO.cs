namespace Huy_FastFood_BE.DTOs
{
    public class CustomerDTO
    {
        public int CustomerId { get; set; }
        public int? AccountId { get; set; }
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<AddressDTO> Addresses { get; set; } = new();
        public List<OrderDetailsDTO> Orders { get; set; } = new();
    }
}
