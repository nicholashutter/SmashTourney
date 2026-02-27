namespace ApiTests;

using CustomExceptions;
using Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Services;

public class UserServiceTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public UserServiceTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        using var scope = _factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }

    private async Task<IUserManager> GetManagerAsync()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IUserManager>();
    }

    private async Task<ApplicationUser> CreateRandomUserAsync(IUserManager manager)
    {
        var rand = Guid.NewGuid().ToString();
        var user = new ApplicationUser { UserName = rand, Email = $"{rand}@mail.com" };
        await manager.CreateUserAsync(user, "SecureP@ssw0rd123!");
        return user;
    }

    [Fact]
    public async Task CreateUserSavesUserToDb()
    {
        var manager = await GetManagerAsync();
        var user = await CreateRandomUserAsync(manager);
        var found = await manager.GetUserByUserNameAsync(user.UserName);
        Assert.NotNull(found);
    }

    [Fact]
    public async Task CreateUserReturnsValidUserId()
    {
        var manager = await GetManagerAsync();
        var user = await CreateRandomUserAsync(manager);
        Assert.False(string.IsNullOrEmpty(user.Id));
    }

    [Fact]
    public async Task GetUserByIdGetsCorrectUser()
    {
        var manager = await GetManagerAsync();
        var user = await CreateRandomUserAsync(manager);
        var found = await manager.GetUserByIdAsync(user.Id);
        Assert.Equivalent(user, found);
    }

    [Fact]
    public async Task UpdateUserAsyncModifiesUserName()
    {
        var manager = await GetManagerAsync();
        var user = await CreateRandomUserAsync(manager);
        user.UserName = "edit@mail.com";
        await manager.UpdateUserAsync(user);
        var found = await manager.GetUserByIdAsync(user.Id);
        Assert.Equal("edit@mail.com", found?.UserName);
    }

    [Fact]
    public async Task DeleteUserAsyncRemovesUserFromDatabase()
    {
        var manager = await GetManagerAsync();
        var user = await CreateRandomUserAsync(manager);
        await manager.DeleteUserAsync(user.Id);
        Assert.Null(await manager.GetUserByIdAsync(user.Id));
    }


}