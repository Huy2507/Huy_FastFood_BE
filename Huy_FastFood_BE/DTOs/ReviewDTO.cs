namespace Huy_FastFood_BE.DTOs
{
    public class ReviewDTO
    {
        public int FoodId { get; set; }
        public int CustomerId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

}
