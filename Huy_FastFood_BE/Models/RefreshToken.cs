using System;
using System.Collections.Generic;

namespace Huy_FastFood_BE.Models;

public partial class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string UserRole { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByToken { get; set; }

    public bool IsRevoked { get; set; }

    public bool IsUsed { get; set; }
}
