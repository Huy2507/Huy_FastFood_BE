using System;
using System.Collections.Generic;

namespace Huy_FastFood_BE.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int FoodId { get; set; }

    public int CustomerId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Food Food { get; set; } = null!;
}
