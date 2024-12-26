using Huy_FastFood_BE.Models;
using Microsoft.AspNetCore.Mvc;

namespace Huy_FastFood_BE.DTOs
{
    public class FoodDTO
    {
        public int FoodId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? ImageUrl { get; set; }
        public bool? Enable { get; set; }
        public bool IsPopular { get; set; }
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoKeywords { get; set; }
        public string? Slug { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }

    public class FoodDetailsDTO
    {
        public int FoodId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public string CategoryName { get; set; }
        public string? ImageUrl { get; set; } = null;
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoKeywords { get; set; }
        public string? Slug { get; set; }
    }

    public class FoodCreateDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool? Enable { get; set; }
        public bool IsPopular { get; set; }
        public int CategoryId { get; set; }
        public string? SeoKeywords { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoTitle { get; set; }

        [FromForm]
        public IFormFile? ImageFile { get; set; } // Nhận file từ input
    }

    public class FoodFavoriteDTO
    {
        public int FoodId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? CategoryName { get; set; }
        public string? ImageUrl { get; set; }
        public bool? Enable { get; set; }
        public bool IsPopular { get; set; }
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoKeywords { get; set; }
        public string? Slug { get; set; }
    }

    public class FoodCategoryDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Slug { get; set; }
    }


    public class FoodCategoryWithItemsDTO
    {
        public FoodCategoryDTO Category { get; set; }
        public List<FoodDTO> Foods { get; set; }
    }

}
