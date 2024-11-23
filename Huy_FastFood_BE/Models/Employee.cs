using System;
using System.Collections.Generic;

namespace Huy_FastFood_BE.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public int? AccountId { get; set; }

    public string Name { get; set; } = null!;

    public string? Position { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public DateTime? HireDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account? Account { get; set; }
}
