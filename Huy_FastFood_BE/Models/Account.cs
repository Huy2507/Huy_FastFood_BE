using System;
using System.Collections.Generic;

namespace Huy_FastFood_BE.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? PasswordResetCode { get; set; }

    public DateTime? ResetCodeExpiration { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Deliverer> Deliverers { get; set; } = new List<Deliverer>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
