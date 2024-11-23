using System;
using System.Collections.Generic;

namespace Huy_FastFood_BE.Models;

public partial class UserRole
{
    public int UserRoleId { get; set; }

    public int AccountId { get; set; }

    public int RoleId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
