using System;
using System.Collections.Generic;

namespace Huy_FastFood_BE.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int? CustomerId { get; set; }

    public decimal? TotalPrice { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Customer? Customer { get; set; }
}
