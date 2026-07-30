namespace ApiTests;

using Contracts;
using Entities;
using Enums;
using Microsoft.Extensions.DependencyInjection;
using Services;

// Verifies that belonging to a game is what grants access to it.
//
// Group authorization only ever established that somebody was signed in, which
// on a laptop at a party is indistinguishable from being safe. On a public host
// it is not: one registered account was enough to read any game's bracket, live
// match and roster, and to start somebody else's tournament. These tests are
// written from the outsider's side — a real signed-in user with a real account
// who simply is not in the game — because that is the caller the old code could
// not tell apart from a participant.
public class GameAccessControlTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly IGameService _gameService;

    public GameAccessControlTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();

        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        database.Database.EnsureCreated();

        _gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
    }

    // Creates a user with a matching player row.
    private async Task<(ApplicationUser User, Player Player)> CreateUserWithPlayerAsync(string displayName)
    {
        using var scope = _factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        var playerManager = scope.ServiceProvider.GetRequiredService<IPlayerManager>();

        var userId = Guid.NewGuid();
        var email = $"access{userId}@email.com";

        var user = new ApplicationUser
        {
            Id = userId.ToString(),
            UserName = email,
            Email = email
        };

        await userManager.CreateUserAsync(user, "SecureP@ssw0rd123!");

        var player = new Player
        {
            Id = userId,
            UserId = user.Id,
            DisplayName = displayName,
            CurrentCharacter = new Character()
        };

        await playerManager.CreateAsync(player);

        return (user, player);
    }

    // Creates a game hosted by its first participant, with everyone joined.
    private async Task<(Guid GameId, List<(ApplicationUser User, Player Player)> Participants)> CreateGameAsync(int participantCount)
    {
        var participants = new List<(ApplicationUser User, Player Player)>();

        for (var index = 0; index < participantCount; index++)
        {
            participants.Add(await CreateUserWithPlayerAsync($"Player {index + 1}"));
        }

        var gameId = await _gameService.CreateGame(
            new CreateGameOptions(BracketMode.SINGLE_ELIMINATION, participantCount),
            participants[0].User.Id);

        foreach (var participant in participants)
        {
            _gameService.AddPlayerToGame(participant.Player, gameId, participant.User.Id);
        }

        return (gameId, participants);
    }

    // Confirms a participant is recognised as being in their game.
    [Fact]
    public async Task AParticipantIsInTheirGame()
    {
        var game = await CreateGameAsync(2);

        Assert.True(await _gameService.IsUserInGameAsync(game.GameId, game.Participants[1].User.Id));
    }

    // Confirms a host counts as being in the game before taking a player slot.
    //
    // A host creates the game and may not have joined it as a player yet, but
    // they still have to be able to read and start it.
    [Fact]
    public async Task AHostIsInTheGameWithoutAPlayerSlot()
    {
        var host = await CreateUserWithPlayerAsync("Host");

        var gameId = await _gameService.CreateGame(
            new CreateGameOptions(BracketMode.SINGLE_ELIMINATION, 4),
            host.User.Id);

        Assert.True(await _gameService.IsUserInGameAsync(gameId, host.User.Id));
        Assert.True(await _gameService.IsUserHostOfGameAsync(gameId, host.User.Id));
    }

    // Confirms a signed-in stranger is not in somebody else's game.
    [Fact]
    public async Task AnOutsiderIsNotInSomebodyElsesGame()
    {
        var game = await CreateGameAsync(2);
        var outsider = await CreateUserWithPlayerAsync("Outsider");

        Assert.False(await _gameService.IsUserInGameAsync(game.GameId, outsider.User.Id));
        Assert.False(await _gameService.IsUserHostOfGameAsync(game.GameId, outsider.User.Id));
    }

    // Confirms a game that does not exist answers the same as an outsider.
    //
    // Both must be false, so probing this cannot be used to work out which game
    // ids are real.
    [Fact]
    public async Task AnUnknownGameIsIndistinguishableFromBeingAnOutsider()
    {
        var stranger = await CreateUserWithPlayerAsync("Stranger");

        Assert.False(await _gameService.IsUserInGameAsync(Guid.NewGuid(), stranger.User.Id));
    }

    // Confirms an anonymous caller is never in a game.
    [Fact]
    public async Task AnEmptyUserIdIsNeverInAGame()
    {
        var game = await CreateGameAsync(2);

        Assert.False(await _gameService.IsUserInGameAsync(game.GameId, string.Empty));
        Assert.False(await _gameService.IsUserHostOfGameAsync(game.GameId, string.Empty));
    }

    // Confirms being in one game grants nothing in another.
    [Fact]
    public async Task MembershipOfOneGameGrantsNothingInAnother()
    {
        var ownGame = await CreateGameAsync(2);
        var otherGame = await CreateGameAsync(2);

        Assert.False(await _gameService.IsUserInGameAsync(otherGame.GameId, ownGame.Participants[1].User.Id));
    }

    // Confirms a participant who is not the host is refused host powers.
    [Fact]
    public async Task AParticipantIsNotTreatedAsTheHost()
    {
        var game = await CreateGameAsync(3);

        Assert.True(await _gameService.IsUserInGameAsync(game.GameId, game.Participants[2].User.Id));
        Assert.False(await _gameService.IsUserHostOfGameAsync(game.GameId, game.Participants[2].User.Id));
    }

    // Confirms the listing shows only the caller's own tournaments.
    //
    // This is the leak the change was made for: the list used to be every game on
    // the server, so any account that registered could take a roll-call of every
    // tournament running and how many people were in each.
    [Fact]
    public async Task TheListingShowsOnlyTheCallersOwnTournaments()
    {
        var ownGame = await CreateGameAsync(2);
        var strangersGame = await CreateGameAsync(4);

        var summaries = await _gameService.GetGameSummariesAsync(ownGame.Participants[1].User.Id);

        Assert.Contains(summaries, summary => summary.GameId == ownGame.GameId);
        Assert.DoesNotContain(summaries, summary => summary.GameId == strangersGame.GameId);
    }

    // Confirms somebody in no games sees an empty list rather than everyone's.
    [Fact]
    public async Task SomebodyInNoTournamentsSeesAnEmptyListing()
    {
        await CreateGameAsync(2);
        await CreateGameAsync(4);

        var newcomer = await CreateUserWithPlayerAsync("Newcomer");

        var summaries = await _gameService.GetGameSummariesAsync(newcomer.User.Id);

        Assert.Empty(summaries);
    }

    // Confirms a host sees a game they created but have not joined.
    [Fact]
    public async Task AHostSeesAGameTheyHaveNotJoinedYet()
    {
        var host = await CreateUserWithPlayerAsync("Host");

        var gameId = await _gameService.CreateGame(
            new CreateGameOptions(BracketMode.SINGLE_ELIMINATION, 4),
            host.User.Id);

        var summaries = await _gameService.GetGameSummariesAsync(host.User.Id);

        var summary = Assert.Single(summaries);
        Assert.Equal(gameId, summary.GameId);
        Assert.True(summary.IsHost);
        Assert.False(summary.HasJoined);
    }

    // Confirms an anonymous caller is shown nothing at all.
    [Fact]
    public async Task AnEmptyUserIdSeesNoTournaments()
    {
        await CreateGameAsync(2);

        Assert.Empty(await _gameService.GetGameSummariesAsync(string.Empty));
    }

    // Confirms the listing counts only players of the caller's own games.
    [Fact]
    public async Task TheListingReportsTheRightPlayerCountWhenOtherGamesExist()
    {
        var ownGame = await CreateGameAsync(3);
        await CreateGameAsync(8);

        var summaries = await _gameService.GetGameSummariesAsync(ownGame.Participants[0].User.Id);
        var summary = summaries.Single(current => current.GameId == ownGame.GameId);

        Assert.Equal(3, summary.PlayerCount);
    }

    // Confirms a player who leaves a game stops seeing it.
    //
    // Membership is read from storage on every request rather than remembered, so
    // losing the player row loses the access with it.
    [Fact]
    public async Task AccessFollowsThePlayerRowRatherThanBeingRemembered()
    {
        var game = await CreateGameAsync(2);
        var departing = game.Participants[1];

        Assert.True(await _gameService.IsUserInGameAsync(game.GameId, departing.User.Id));

        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var playerManager = scope.ServiceProvider.GetRequiredService<IPlayerManager>();
            await playerManager.DeleteAsync(departing.Player.Id);
        }

        Assert.False(await _gameService.IsUserInGameAsync(game.GameId, departing.User.Id));
        Assert.Empty(await _gameService.GetGameSummariesAsync(departing.User.Id));
    }
}
