namespace Huy_FastFood_BE.DTOs
{
    public class LoginDTO
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class RegisterDTO
    {
        public required string Username { get; set; } = null!;
        public required string Password { get; set; } = null!;
        public required string ConfirmPassword { get; set; } = null!;
        public required string Name { get; set; } = null!;
        public required string Phone { get; set; }
        public required string Email { get; set; }
    }
}
