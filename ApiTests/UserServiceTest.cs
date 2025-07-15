namespace Tests;

using Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Services;

public class UserServiceTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UserServiceTest()
    {
        _factory = new WebApplicationFactory<Program>();
    }




    [Fact]
    public async Task CreateUserSavesUserToDb()
    {

        var rand = Guid.NewGuid();

        string userId = "";
        using (var scope = _factory.Services.CreateScope())
        {
            var us = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var user = new ApplicationUser
            {
                Id = rand.ToString(),
                UserName = rand.ToString(),
                Email = $"{rand}@mail.com"
            };

            userId = await us.CreateUserAsync(user);
        }

        Assert.False(string.IsNullOrEmpty(userId));
    }

    [Fact]
    public async Task GetUserByIdAsyncGetsCorrectUser()
    {

        var rand = Guid.NewGuid();
        using (var scope = _factory.Services.CreateScope())
        {
            var us = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var user = new ApplicationUser
            {
                Id = rand.ToString(),
                UserName = rand.ToString(),
                Email = $"{rand}@mail.com"
            };

            var userId = await us.CreateUserAsync(user);

            var foundUser = await us.GetUserByIdAsync(user.Id);

            Assert.Equivalent(user, foundUser);
        }

    }


    [Fact]
    public async Task UpdateUserAsyncAppliesPropertiesCorrectly()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var rand = Guid.NewGuid();

            var us = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var user = new ApplicationUser
            {
                Id = rand.ToString(),
                UserName = rand.ToString(),
                Email = $"{rand}@mail.com"
            };

            var userId = await us.CreateUserAsync(user);

            string editUserName = "edit@mail.com";

            user.UserName = editUserName;

            await us.UpdateUserAsync(user);

            var foundUser = await us.GetUserByIdAsync(user.Id);

            Assert.Equal(user.UserName, foundUser.UserName);
        }
    }

    [Fact]
    public async Task DeleteUserAsyncRemovesUser()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var rand = Guid.NewGuid();

            var us = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var user = new ApplicationUser
            {
                Id = rand.ToString(),
                UserName = rand.ToString(),
                Email = $"{rand}@mail.com"
            };

            var userId = await us.CreateUserAsync(user);

            await us.DeleteUserAsync(userId);

            var foundUser = await us.GetUserByIdAsync(userId);

            Assert.Null(foundUser);
        }
    }
}