namespace Services;

using System.Security.Claims;
using Entities;

public interface IAuthService
{

    Task<ClaimsPrincipal?> AuthenticateAsync(string username, string password);
    Task<bool> RegisterUserAsync(string username, string password);
    Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
    Task<string> GenerateTokenAsync(ApplicationUser user);
    Task<ClaimsPrincipal?> ValidateTokenAsync(string refreshToken);

}