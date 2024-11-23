using System;
using System.Collections.Generic;

namespace Huy_FastFood_BE.Models;

public partial class Deliverer
{
    public int DelivererId { get; set; }

    public int? AccountId { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? VehicleInfo { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account? Account { get; set; }

    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
}
