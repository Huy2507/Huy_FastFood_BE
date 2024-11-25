using Huy_FastFood_BE.Models;

namespace Huy_FastFood_BE.DTOs
{
    public class FoodDTO
    {
        public int FoodId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int? CategoryId { get; set; }

        public string? ImageUrl { get; set; }

        public string? Enable { get; set; }

        public string? SeoTitle { get; set; }

        public string? SeoDescription { get; set; }

        public string? SeoKeywords { get; set; }

        public string? Slug { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

    }
}
