using System;
using System.Collections.Generic;

namespace Huy_FastFood_BE.Models;

public partial class Delivery
{
    public int DeliveryId { get; set; }

    public int? OrderId { get; set; }

    public int? DelivererId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual Deliverer? Deliverer { get; set; }

    public virtual Order? Order { get; set; }
}
