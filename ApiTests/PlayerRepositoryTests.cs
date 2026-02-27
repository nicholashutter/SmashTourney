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
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }

    private async Task<IPlayerManager> GetManagerAsync()
        => _factory.Services.CreateAsyncScope().ServiceProvider.GetRequiredService<IPlayerManager>();

    [Fact]
    public async Task CreateAsyncReturnsValidId()
    {
        var manager = await GetManagerAsync();
        var result = await manager.CreateAsync(new Player { UserId = AppConstants.ByeUserId });
        Assert.IsType<Guid>(result);
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllPlayers()
    {
        var manager = await GetManagerAsync();
        const int expected = 10;
        await Task.WhenAll(Enumerable.Range(0, expected)
            .Select(_ => manager.CreateAsync(new Player { UserId = AppConstants.ByeUserId })));

        var result = await manager.GetAllPlayersAsync();
        Assert.Equal(expected, result?.Count);
    }


}