namespace Huy_FastFood_BE.DTOs
{
    public class CustomerDTO
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public List<AddressDTO> Addresses { get; set; } = new();
        public List<OrderDTO> Orders { get; set; } = new();
        public List<CartDTO> Carts { get; set; } = new();
    }
}
