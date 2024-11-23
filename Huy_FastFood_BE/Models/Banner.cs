using System;
using System.Collections.Generic;

namespace Huy_FastFood_BE.Models;

public partial class Banner
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
