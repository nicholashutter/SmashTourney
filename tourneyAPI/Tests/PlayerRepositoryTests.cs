using Xunit;
using Entities;
using Services;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Tests;

public class PlayerRepositoryTest
{
    private ApplicationDbContext db;
    private PlayerRepository repository;

    private UserRepository userRepository;


    public PlayerRepositoryTest()
    {
        db = new ApplicationDbContext();
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

        var mockUser = Guid.NewGuid();



        var Id = await repository.CreateAsync(new Player
        {
            UserId = mockUser,
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

    [Fact]
    public async Task getAllPlayers()
    {

        var players = await repository.GetAllAsync();

        Assert.NotNull(players);
    }

    [Fact]
    public async Task getByIdAsync()
    {

        var mockUser = new Guid();



        var Id = await repository.CreateAsync(new Player
        {
            UserId = mockUser,
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
    [Fact]
    public async Task updateAsync()
    {

        var mockUser = new Guid();

        var Id = await repository.CreateAsync(new Player
        {
            UserId = mockUser,
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
            UserId = mockUser,
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

    public async Task deleteAsync()
    {

        var mockUser = new Guid();

        var Id = await repository.CreateAsync(new Player
        {
            UserId = mockUser,
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