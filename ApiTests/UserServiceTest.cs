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
        _factory = _factory = new CustomWebApplicationFactory<Program>();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateUserSavesUserToDb()
    {

        Guid rand = Guid.NewGuid();

        string userId = "";
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();

            ApplicationUser user = new ApplicationUser
            {
                UserName = rand.ToString(),
                Email = $"{rand}@mail.com"
            };

            userId = await userManager.CreateUserAsync(user);
        }

        Assert.False(string.IsNullOrEmpty(userId));
    }

    [Fact]
    public async Task CreateUserReturnsValidUserId()
    {
        string rand = Guid.NewGuid().ToString();

        using IServiceScope scope = _factory.Services.CreateScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();

        ApplicationUser user = new ApplicationUser
        {
            UserName = rand,
            Email = $"{rand}@mail.com"
        };

        string userId = await userManager.CreateUserAsync(user);

        Assert.False(string.IsNullOrEmpty(userId));
    }

    [Fact]
    public async Task GetUserByIdGetsCorrectUser()
    {
        string rand = Guid.NewGuid().ToString();

        using IServiceScope scope = _factory.Services.CreateScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        ApplicationUser user = new ApplicationUser
        {
            UserName = rand,
            Email = $"{rand}@mail.com"
        };

        await userManager.CreateUserAsync(user);

        ApplicationUser foundUser = await userManager.GetUserByIdAsync(user.Id);

        Assert.Equivalent(user, foundUser);
    }



    [Fact]
    public async Task UpdateUserAsyncModifiesUserName()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        Guid rand = Guid.NewGuid();

        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();

        ApplicationUser user = new ApplicationUser
        {
            UserName = rand.ToString(),
            Email = $"{rand}@mail.com"
        };

        await userManager.CreateUserAsync(user);

        string newName = "edit@mail.com";
        user.UserName = newName;

        await userManager.UpdateUserAsync(user);
        ApplicationUser foundUser = await userManager.GetUserByIdAsync(user.Id);

        Assert.Equal(newName, foundUser.UserName);
    }



    [Fact]
    public async Task DeleteUserAsyncRemovesUserFromDatabase()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        string rand = Guid.NewGuid().ToString();

        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();

        ApplicationUser user = new ApplicationUser
        {
            UserName = rand,
            Email = $"{rand}@mail.com"
        };

        await userManager.CreateUserAsync(user);
        await userManager.DeleteUserAsync(user.Id);

        ApplicationUser deletedUser = await userManager.GetUserByIdAsync(user.Id);
        Assert.Null(deletedUser);
    }


}