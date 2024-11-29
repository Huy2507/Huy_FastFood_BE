namespace Huy_FastFood_BE.DTOs
{
    public class BannerDTO
    {
        public int Id { get; set; }

        public string? BannerImg { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? LinkUrl { get; set; }

        public string? SeoTitle { get; set; }

        public string? SeoDescript { get; set; }

        public string? SeoKeywords { get; set; }

        public string? Slug { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
    public class BannerCreateDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? LinkUrl { get; set; }
        public string? SeoTitle { get; set; }
        public string? SeoDescript { get; set; }
        public string? SeoKeywords { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IFormFile? BannerImgFile { get; set; } // Dùng để nhận file ảnh
    }

}
