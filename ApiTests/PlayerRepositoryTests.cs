namespace ApiTests;

using Xunit;
using Entities;
using Services;
using Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

public class PlayerRepositoryTest : IClassFixture<CustomWebApplicationFactory<Program>>

{
    private readonly CustomWebApplicationFactory<Program> _factory;
    public PlayerRepositoryTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateAsyncReturnsValidId()
    {
        using (IServiceScope scope = _factory.Services.CreateAsyncScope())
        {
            IPlayerManager playerManager = scope.ServiceProvider.GetRequiredService<IPlayerManager>();
            Player newPlayer = new Player { UserId = AppConstants.ByeUserId };
            Guid? result = await playerManager.CreateAsync(newPlayer);
            Assert.IsType<Guid>(result);
        }

    }

    [Fact]
    public async Task GetAllAsyncReturnsAllPlayers()
    {
        using (IServiceScope scope = _factory.Services.CreateAsyncScope())
        {
            IPlayerManager playerManager = scope.ServiceProvider.GetRequiredService<IPlayerManager>();

            for (int i = 0; i < 10; i++)
            {
                Player newPlayer = new Player { UserId = AppConstants.ByeUserId };
                await playerManager.CreateAsync(newPlayer);
            }

            List<Player>? result = await playerManager.GetAllPlayersAsync();
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
        }
    }


}