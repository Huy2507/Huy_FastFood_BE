namespace Huy_FastFood_BE.DTOs
{
    public class CartDTO
    {
        public int CartId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<CartItemDTO> CartItems { get; set; } = new();
    }
}
