namespace Services;

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Entities;

public class AuthService : IAuthService
{
    public Task<ClaimsPrincipal?> AuthenticateAsync(string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RegisterUserAsync(string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        throw new NotImplementedException();
    }

    public Task<string> GenerateTokenAsync(ApplicationUser user)
    {
        throw new NotImplementedException();
    }

    public Task<ClaimsPrincipal?> ValidateTokenAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }
}