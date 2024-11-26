namespace Huy_FastFood_BE.DTOs
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }

        public string? SeoTitle { get; set; }

        public string? SeoDescription { get; set; }

        public string? SeoKeywords { get; set; }

        public string? Slug { get; set; }

        public string? ImgUrl { get; set; }
    }
}
