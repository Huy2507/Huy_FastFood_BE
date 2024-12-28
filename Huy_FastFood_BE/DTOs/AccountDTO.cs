namespace Huy_FastFood_BE.DTOs
{
    public class AccountDTO
    {
        public int AccountId { get; set; }
        public string Username { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>(); // List of role names
    }

    public class AccountDetailDTO
    {
        public int AccountId { get; set; }
        public string Username { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<int> Roles { get; set; } = new List<int>(); // List of role names
    }

    public class CreateAccountDTO
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool IsActive { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>(); // Danh sách ID vai trò
    }

    public class UpdateAccountDTO
    {
        public string? Password { get; set; }
        public bool IsActive { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>(); // Danh sách ID vai trò
    }
}
