namespace Huy_FastFood_BE.DTOs
{
    public class EmployeeDTO
    {
        public int EmployeeId { get; set; }
        public int? AccountId { get; set; }
        public string? Username { get; set; }
        public string Name { get; set; } = null!;
        public List<int> RoleIds { get; set; } = new List<int>();
        public string? Position { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? LeaveDate { get; set; } // Thêm ngày nghỉ việc
        public bool IsActive { get; set; } // Thêm trạng thái hoạt động
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateEmployeeDTO
    {
        public string Username { get; set; } = null!; // Tạo tài khoản
        public string Password { get; set; } = null!; // Tạo tài khoản
        public List<int> RoleIds { get; set; } = new List<int>();
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? HireDate { get; set; }

    }

    public class UpdateEmployeeDTO
    {
        public string Name { get; set; } = null!;
        public List<int> RoleIds { get; set; } = new List<int>();
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? LeaveDate { get; set; }
        public bool IsActive { get; set; }
    }
}
