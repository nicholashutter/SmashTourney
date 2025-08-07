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
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();

            string Password = "SecureP@ssw0rd123!";

            ApplicationUser user = new ApplicationUser
            {
                UserName = rand.ToString(),
                Email = $"{rand}@mail.com"
            };

            var result = await userManager.CreateUserAsync(user, Password);

            Assert.True(result.Succeeded,
                $"User creation failed. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    [Fact]
    public async Task CreateUserReturnsValidUserId()
    {
        string rand = Guid.NewGuid().ToString();

        using IServiceScope scope = _factory.Services.CreateScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();

        var Password = "SecureP@ssw0rd123!";

        ApplicationUser user = new ApplicationUser
        {
            UserName = rand,
            Email = $"{rand}@mail.com"
        };

        var result = await userManager.CreateUserAsync(user, Password);

        ApplicationUser? foundUser = await userManager.GetUserByUserNameAsync(rand);

        Assert.False(string.IsNullOrEmpty(foundUser?.Id));
    }

    [Fact]
    public async Task GetUserByIdGetsCorrectUser()
    {
        string rand = Guid.NewGuid().ToString();

        using IServiceScope scope = _factory.Services.CreateScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var Password = "SecureP@ssw0rd123!";

        ApplicationUser user = new ApplicationUser
        {
            UserName = rand,
            Email = $"{rand}@mail.com"
        };

        await userManager.CreateUserAsync(user, Password);

        ApplicationUser? foundUser = await userManager.GetUserByIdAsync(user.Id);

        Assert.Equivalent(user, foundUser);
    }



    [Fact]
    public async Task UpdateUserAsyncModifiesUserName()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        Guid rand = Guid.NewGuid();

        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();

        var Password = "SecureP@ssw0rd123!";

        ApplicationUser user = new ApplicationUser
        {
            UserName = rand.ToString(),
            Email = $"{rand}@mail.com"
        };

        await userManager.CreateUserAsync(user, Password);

        string newName = "edit@mail.com";
        user.UserName = newName;

        await userManager.UpdateUserAsync(user);
        ApplicationUser? foundUser = await userManager.GetUserByIdAsync(user.Id);

        Assert.Equal(newName, foundUser?.UserName);
    }



    [Fact]
    public async Task DeleteUserAsyncRemovesUserFromDatabase()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        string rand = Guid.NewGuid().ToString();

        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();

        var Password = "SecureP@ssw0rd123!";

        ApplicationUser user = new ApplicationUser
        {
            UserName = rand,
            Email = $"{rand}@mail.com"
        };

        await userManager.CreateUserAsync(user, Password);
        await userManager.DeleteUserAsync(user.Id);

        ApplicationUser? deletedUser = await userManager.GetUserByIdAsync(user.Id);
        Assert.Null(deletedUser);
    }


}