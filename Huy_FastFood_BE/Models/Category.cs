using System;
using System.Collections.Generic;

namespace Huy_FastFood_BE.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? SeoKeywords { get; set; }

    public string? Slug { get; set; }

    public string? ImgUrl { get; set; }

    public virtual ICollection<Food> Foods { get; set; } = new List<Food>();
}
