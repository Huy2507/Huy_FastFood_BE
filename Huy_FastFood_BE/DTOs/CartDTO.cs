namespace Huy_FastFood_BE.DTOs
{
    public class CartDTO
    {
        public int CartId { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<CartItemDTO> CartItems { get; set; } = new();
    }

    public class AddToCartDTO
    {
        public int FoodId { get; set; } // ID món ăn
        public int Quantity { get; set; } // Số lượng muốn thêm
    }

    public class UpdateCartDTO
    {
        public int FoodId { get; set; }
        public int Quantity { get; set; } // Số lượng muốn giảm
    }
}
