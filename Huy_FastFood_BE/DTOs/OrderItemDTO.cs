namespace Huy_FastFood_BE.DTOs
{
    public class OrderItemDTO
    {
        public int? FoodId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string FoodName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal? TotalPrice { get; set; }
    }

}
