using Huy_FastFood_BE.Models;

namespace Huy_FastFood_BE.Services;

public interface ITokenService
{
    string GenerateAccessToken(Account account, List<string> roles);
    RefreshToken GenerateRefreshToken(int userId, string userRole);
    Task<bool> ValidateRefreshToken(string token, int userId);
    Task RevokeRefreshToken(string token);
}
