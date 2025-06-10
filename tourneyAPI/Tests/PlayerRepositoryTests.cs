using Xunit;
using Entities;
using Services;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Tests;

public class PlayerRepositoryTest
{
    private const string dbName = "tourneydb.db";
    private ApplicationDbContext db;
    private PlayerRepository repository;

    private UserRepository userRepository;

    public PlayerRepositoryTest()
    {
        db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>(), dbName);
        repository = new PlayerRepository(db);
        userRepository = new UserRepository(db);
    }

    public void Dispose()
    {
        db.Dispose();
    }

    [Fact]
    public async Task createPlayer()
    {
        var users = await userRepository.GetAllUsersAsync();

        var dummyGuid = Guid.NewGuid();



        var Id = await repository.CreateAsync(new Player
        {
            UserId = users[0].Id,
            DisplayName = "testUser",
            CurrentScore = 0,
            CurrentRound = 0,
            CurrentOpponent = dummyGuid,
            CurrentCharacter = "Bowser",
            CurrentGameID = dummyGuid,
            HasVoted = false,
            MatchWinner = dummyGuid
        });

        Assert.IsType<Guid>(Id);
    }

    [Fact]
    public async Task getAllPlayers()
    {

        var players = await repository.GetAllAsync();

        Assert.NotNull(players);
    }

    [Fact]
    public async Task getByIdAsync()
    {
        var users = await userRepository.GetAllUsersAsync();

        var dummyGuid = new Guid();



        var Id = await repository.CreateAsync(new Player
        {
            UserId = users[0].Id,
            DisplayName = "testUser",
            CurrentScore = 0,
            CurrentRound = 0,
            CurrentOpponent = dummyGuid,
            CurrentCharacter = "Bowser",
            CurrentGameID = dummyGuid,
            HasVoted = false,
            MatchWinner = dummyGuid
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
    [Fact]
    public async Task updateAsync()
    {
        var users = await userRepository.GetAllUsersAsync();

        var dummyGuid = new Guid();

        var Id = await repository.CreateAsync(new Player
        {
            UserId = users[0].Id,
            DisplayName = "testUser",
            CurrentScore = 0,
            CurrentRound = 0,
            CurrentOpponent = dummyGuid,
            CurrentCharacter = "Bowser",
            CurrentGameID = dummyGuid,
            HasVoted = false,
            MatchWinner = dummyGuid
        });

        var success = await repository.UpdateAsync(new Player
        {
            Id = (Guid)Id,
            UserId = users[0].Id,
            DisplayName = "testUserUpdate",
            CurrentScore = 0,
            CurrentRound = 0,
            CurrentOpponent = dummyGuid,
            CurrentCharacter = "Bowser",
            CurrentGameID = dummyGuid,
            HasVoted = false,
            MatchWinner = dummyGuid
        });

        Assert.True(success);
    }

    public async Task deleteAsync()
    {
        var users = await userRepository.GetAllUsersAsync();

        var dummyGuid = new Guid();

        var Id = await repository.CreateAsync(new Player
        {
            UserId = users[0].Id,
            DisplayName = "testUser",
            CurrentScore = 0,
            CurrentRound = 0,
            CurrentOpponent = dummyGuid,
            CurrentCharacter = "Bowser",
            CurrentGameID = dummyGuid,
            HasVoted = false,
            MatchWinner = dummyGuid
        });

        var success = await repository.DeleteAsync((Guid)Id);

        Assert.True(success);
    }
}