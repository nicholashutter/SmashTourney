namespace ApiTests;

using Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Services;

// Verifies user management business outcomes for create, read, update, and delete flows.
public class UserServiceTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    // Initializes test host resources required by user service tests.
    public UserServiceTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        using var scope = _factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }

    // Resolves a user manager from the test host service container.
    private IUserManager GetManager()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IUserManager>();
    }

    // Creates one random user used by user service tests.
    private async Task<ApplicationUser> CreateRandomUserAsync(IUserManager manager)
    {
        var rand = Guid.NewGuid().ToString();
        var user = new ApplicationUser { UserName = rand, Email = $"{rand}@mail.com" };
        await manager.CreateUserAsync(user, "SecureP@ssw0rd123!");
        return user;
    }

    // Confirms user creation is persisted and user can be retrieved by username.
    [Fact]
    public async Task CreateUserSavesUserToDb()
    {
        var manager = GetManager();
        var user = await CreateRandomUserAsync(manager);
        if (string.IsNullOrWhiteSpace(user.UserName))
        {
            throw new InvalidOperationException("Created test user did not have a valid username.");
        }
        var found = await manager.GetUserByUserNameAsync(user.UserName);
        Assert.NotNull(found);
    }

    // Confirms created users receive a valid user identifier.
    [Fact]
    public async Task CreateUserReturnsValidUserId()
    {
        var manager = GetManager();
        var user = await CreateRandomUserAsync(manager);
        Assert.False(string.IsNullOrEmpty(user.Id));
    }

    // Confirms user lookup by ID returns the expected user record.
    [Fact]
    public async Task GetUserByIdGetsCorrectUser()
    {
        var manager = GetManager();
        var user = await CreateRandomUserAsync(manager);
        var found = await manager.GetUserByIdAsync(user.Id);
        Assert.Equivalent(user, found);
    }

    // Confirms updating a user modifies persisted profile values.
    [Fact]
    public async Task UpdateUserAsyncModifiesUserName()
    {
        var manager = GetManager();
        var user = await CreateRandomUserAsync(manager);
        user.UserName = "edit@mail.com";
        await manager.UpdateUserAsync(user);
        var found = await manager.GetUserByIdAsync(user.Id);
        Assert.Equal("edit@mail.com", found?.UserName);
    }

    // Confirms deleting a user removes the record from storage.
    [Fact]
    public async Task DeleteUserAsyncRemovesUserFromDatabase()
    {
        var manager = GetManager();
        var user = await CreateRandomUserAsync(manager);
        await manager.DeleteUserAsync(user.Id);
        Assert.Null(await manager.GetUserByIdAsync(user.Id));
    }
}