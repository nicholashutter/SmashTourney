using Xunit;
using Entities;
using Services;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Tests;

public class PlayerRepositoryTest
{
    private WebApplication app;

    public PlayerRepositoryTest()
    {
        var builder = WebApplication.CreateBuilder();

        var dbFileName = "tourneyDb.db";
        var dbPath = $"DataSource={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFileName)}";
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(dbPath);
        });

        builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        app = builder.Build();

    }

    [Fact]
    public async Task createPlayer()
    {
        using (var serviceScope = app.Services.CreateScope())
        {
            var services = serviceScope.ServiceProvider;

            var mockUser = Guid.NewGuid();

            var repository = services.GetRequiredService<IPlayerRepository>();

            var Id = await repository.CreateAsync(new Player
            {
                UserId = mockUser.ToString(),
                DisplayName = "testUser",
                CurrentScore = 0,
                CurrentRound = 0,
                CurrentOpponent = mockUser,
                CurrentCharacter = "Bowser",
                CurrentGameID = mockUser,
                HasVoted = false,
                MatchWinner = mockUser
            });

            Assert.IsType<Guid>(Id);
        }
    }

    [Fact]
    public async Task getAllPlayers()
    {
        using (var serviceScope = app.Services.CreateScope())
        {
            var services = serviceScope.ServiceProvider;

            var repository = services.GetRequiredService<IPlayerRepository>();

            var players = await repository.GetAllAsync();

            Assert.NotNull(players);
        }

    }

    [Fact]
    public async Task getByIdAsync()
    {

        using (var serviceScope = app.Services.CreateScope())
        {
            var services = serviceScope.ServiceProvider;

            var mockUser = new Guid();

            var repository = services.GetRequiredService<IPlayerRepository>();

            var Id = await repository.CreateAsync(new Player
            {
                UserId = mockUser.ToString(),
                DisplayName = "testUser",
                CurrentScore = 0,
                CurrentRound = 0,
                CurrentOpponent = mockUser,
                CurrentCharacter = "Bowser",
                CurrentGameID = mockUser,
                HasVoted = false,
                MatchWinner = mockUser
            });

            if (Id is not null)
            {
                var foundPlayer = await repository.GetByIdAsync((Guid)Id);
                Assert.Equal(foundPlayer.Id, Id);
            }
            else
            {
                throw new Exception();
            }
        }

    }
    [Fact]
    public async Task updateAsync()
    {

        using (var serviceScope = app.Services.CreateScope())
        {
            var services = serviceScope.ServiceProvider;

            var mockUser = new Guid();

            var repository = services.GetRequiredService<IPlayerRepository>();

            var Id = await repository.CreateAsync(new Player
            {
                UserId = mockUser.ToString(),
                DisplayName = "testUser",
                CurrentScore = 0,
                CurrentRound = 0,
                CurrentOpponent = mockUser,
                CurrentCharacter = "Bowser",
                CurrentGameID = mockUser,
                HasVoted = false,
                MatchWinner = mockUser
            });

            var success = await repository.UpdateAsync(new Player
            {
                Id = (Guid)Id,
                UserId = mockUser.ToString(),
                DisplayName = "testUserUpdate",
                CurrentScore = 0,
                CurrentRound = 0,
                CurrentOpponent = mockUser,
                CurrentCharacter = "Bowser",
                CurrentGameID = mockUser,
                HasVoted = false,
                MatchWinner = mockUser
            });

            Assert.True(success);
        }


    }

    public async Task deleteAsync()
    {
        using (var serviceScope = app.Services.CreateScope())
        {

            var services = serviceScope.ServiceProvider;

            var mockUser = Guid.NewGuid();

            var repository = services.GetRequiredService<IPlayerRepository>();

            var Id = await repository.CreateAsync(new Player
            {
                UserId = mockUser.ToString(),
                DisplayName = "testUser",
                CurrentScore = 0,
                CurrentRound = 0,
                CurrentOpponent = mockUser,
                CurrentCharacter = "Bowser",
                CurrentGameID = mockUser,
                HasVoted = false,
                MatchWinner = mockUser
            });

            var success = await repository.DeleteAsync((Guid)Id);

            Assert.True(success);
        }


    }
}