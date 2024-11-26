namespace Huy_FastFood_BE.DTOs
{
    public class EmployeeDTO
    {
        public int EmployeeId { get; set; }
        public int? AccountId { get; set; }
        public string? Username { get; set; }
        public string Name { get; set; } = null!;
        public string? Position { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? HireDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }


    public class CreateEmployeeDTO
    {
        public string Username { get; set; } = null!; // For account creation
        public string Password { get; set; } = null!; // For account creation

        public string Name { get; set; } = null!;
        public string? Position { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? HireDate { get; set; }
    }
}
