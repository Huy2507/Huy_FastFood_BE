namespace Huy_FastFood_BE.DTOs
{
    public class CartItemDTO
    {
        public int CartItemId { get; set; }
        public int? FoodId { get; set; }
        public int Quantity { get; set; }
    }
}
