namespace Services;

using Entities;

public interface IAuthService
{
    //  Task<ClaimsPrincipal?> AuthenticateAsync(string username, string password);
    Task<bool> RegisterUserAsync(string username, string password);
    Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
    Task<string> GenerateTokenAsync(User user);
    //Task<ClaimsPrincipal?> ValidateTokenAsync(string refreshToken);

}