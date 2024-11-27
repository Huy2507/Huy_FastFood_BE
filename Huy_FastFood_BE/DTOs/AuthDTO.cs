namespace Huy_FastFood_BE.DTOs
{
    public class LoginDTO
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class RefreshTokenDTO
    {
        public string RefreshToken { get; set; } = null!;
        public int UserId { get; set; }
    }

    public class LogoutDTO
    {
        public string RefreshToken { get; set; } = null!;
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

    public class ForgotPasswordRequestDTO
    {
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyResetCodeDTO
    {
        public string Email { get; set; } = string.Empty;
        public string ResetCode { get; set; } = string.Empty;
    }

    public class ResetPasswordDTO
    {
        public string Email { get; set; } = string.Empty;
        public string ResetCode { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }


}
