namespace Services;

using Entities;

public interface IUserRepository
{
    Task<ApplicationUser?> GetUserByIdAsync(Guid id);
    Task<List<ApplicationUser>?> GetAllUsersAsync();
    Task<Guid?> CreateUserAsync(ApplicationUser newUser);
    Task<bool> UpdateUserAsync(ApplicationUser updateUser);
    Task<bool> DeleteUserAsync(Guid id);

}