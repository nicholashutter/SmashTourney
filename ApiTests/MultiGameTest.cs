namespace ApiTests;

using Contracts;
using Entities;
using Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Services;

// Verifies that several tournaments can run on one server without touching each
// other, that people can find them, and that only a host can end one.
//
// The engines were already keyed by game id, so the risk here is not that the
// brackets get confused — it is that the code added around them (listing,
// ending, sweeping) reaches across games it has no business touching.
public class MultiGameTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly IGameService _gameService;

    public MultiGameTest()
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
        var email = $"multi{userId}@email.com";

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

    // Confirms two tournaments running at once stay entirely independent.
    //
    // This is the whole promise of concurrent games: advancing one bracket must
    // not move, reveal, or disturb anything in another.
    [Fact]
    public async Task AdvancingOneTournamentLeavesAnotherUntouched()
    {
        var firstGame = await CreateGameAsync(4);
        var secondGame = await CreateGameAsync(4);

        await _gameService.StartGameAsync(firstGame.GameId);
        await _gameService.StartGameAsync(secondGame.GameId);

        var secondBefore = await _gameService.GetBracketSnapshotAsync(secondGame.GameId);
        var secondMatchBefore = await _gameService.GetCurrentMatchAsync(secondGame.GameId);

        var firstMatch = await _gameService.GetCurrentMatchAsync(firstGame.GameId);
        Assert.NotNull(firstMatch);

        var firstMatchPlayerOne = firstGame.Participants.Single(participant => participant.Player.Id == firstMatch!.PlayerOneId);
        var firstMatchPlayerTwo = firstGame.Participants.Single(participant => participant.Player.Id == firstMatch.PlayerTwoId);
        var firstMatchVoteRequest = new SubmitMatchVoteRequest(firstMatch.MatchId, firstMatch.PlayerOneId);

        var firstPendingVote = await _gameService.SubmitMatchVoteAsync(firstGame.GameId, firstMatchPlayerOne.User.Id, firstMatchVoteRequest);
        Assert.Equal(SubmitMatchVoteStatus.PENDING, firstPendingVote.Status);

        var firstCommittingVote = await _gameService.SubmitMatchVoteAsync(firstGame.GameId, firstMatchPlayerTwo.User.Id, firstMatchVoteRequest);
        Assert.Equal(SubmitMatchVoteStatus.COMMITTED, firstCommittingVote.Status);

        var secondAfter = await _gameService.GetBracketSnapshotAsync(secondGame.GameId);
        var secondMatchAfter = await _gameService.GetCurrentMatchAsync(secondGame.GameId);

        Assert.Equal(secondMatchBefore!.MatchId, secondMatchAfter!.MatchId);
        Assert.Equal(
            secondBefore!.Matches.Count(match => match.WinnerId is not null),
            secondAfter!.Matches.Count(match => match.WinnerId is not null));
    }

    // Confirms no match id is ever shared between two tournaments.
    [Fact]
    public async Task TournamentsDoNotShareMatchIdentities()
    {
        var firstGame = await CreateGameAsync(4);
        var secondGame = await CreateGameAsync(4);

        await _gameService.StartGameAsync(firstGame.GameId);
        await _gameService.StartGameAsync(secondGame.GameId);

        var firstSnapshot = await _gameService.GetBracketSnapshotAsync(firstGame.GameId);
        var secondSnapshot = await _gameService.GetBracketSnapshotAsync(secondGame.GameId);

        var firstMatchIds = firstSnapshot!.Matches.Select(match => match.MatchId).ToHashSet();
        var secondMatchIds = secondSnapshot!.Matches.Select(match => match.MatchId).ToHashSet();

        Assert.Empty(firstMatchIds.Intersect(secondMatchIds));
    }

    // Confirms a vote cannot be aimed at a match belonging to another game.
    [Fact]
    public async Task AVoteCannotReachAnotherTournamentsMatch()
    {
        var firstGame = await CreateGameAsync(2);
        var secondGame = await CreateGameAsync(2);

        await _gameService.StartGameAsync(firstGame.GameId);
        await _gameService.StartGameAsync(secondGame.GameId);

        var secondMatch = await _gameService.GetCurrentMatchAsync(secondGame.GameId);
        Assert.NotNull(secondMatch);

        // A participant of the first game votes on the second game's match id,
        // but sends it to the first game.
        var vote = await _gameService.SubmitMatchVoteAsync(
            firstGame.GameId,
            firstGame.Participants[0].User.Id,
            new SubmitMatchVoteRequest(secondMatch!.MatchId, secondMatch.PlayerOneId));

        Assert.Equal(SubmitMatchVoteStatus.MATCH_NOT_ACTIVE, vote.Status);
    }

    // Confirms a summary describes the caller's own game accurately.
    //
    // This used to also assert that a stranger's game appeared in the list with
    // its flags cleared, which is exactly the disclosure the listing was later
    // narrowed to prevent. A second tournament is still created here, so the
    // assertion that it stays out of the list is doing real work rather than
    // being trivially true — GameAccessControlTest covers the isolation itself.
    [Fact]
    public async Task GameSummariesDescribeTheCallersOwnTournament()
    {
        var hostedGame = await CreateGameAsync(4);
        var strangersGame = await CreateGameAsync(2);

        var host = hostedGame.Participants[0].User;

        var summaries = await _gameService.GetGameSummariesAsync(host.Id);

        var hostedSummary = summaries.Single(summary => summary.GameId == hostedGame.GameId);

        Assert.True(hostedSummary.IsHost);
        Assert.True(hostedSummary.HasJoined);
        Assert.Equal(4, hostedSummary.PlayerCount);
        Assert.Equal(GameState.LOBBY_WAITING, hostedSummary.State);

        Assert.DoesNotContain(summaries, summary => summary.GameId == strangersGame.GameId);
    }

    // Confirms listing games does not advance any of them.
    //
    // The obvious way to report each game's state would be to call
    // GetGameStateAsync, which auto-resolves byes and persists. Drawing a menu
    // would then quietly play out every tournament on the server.
    [Fact]
    public async Task ListingGamesDoesNotAdvanceAnyBracket()
    {
        var game = await CreateGameAsync(4);
        await _gameService.StartGameAsync(game.GameId);

        var before = await _gameService.GetCurrentMatchAsync(game.GameId);
        Assert.NotNull(before);

        await _gameService.GetGameSummariesAsync(game.Participants[0].User.Id);
        await _gameService.GetGameSummariesAsync(game.Participants[0].User.Id);

        var after = await _gameService.GetCurrentMatchAsync(game.GameId);

        Assert.Equal(before!.MatchId, after!.MatchId);
    }

    // Confirms a started tournament is reported as started in the listing.
    [Fact]
    public async Task GameSummariesReflectAStartedTournament()
    {
        var game = await CreateGameAsync(2);
        await _gameService.StartGameAsync(game.GameId);

        var summaries = await _gameService.GetGameSummariesAsync(game.Participants[0].User.Id);
        var summary = summaries.Single(current => current.GameId == game.GameId);

        Assert.Equal(GameState.IN_MATCH_ACTIVE, summary.State);
    }

    // Confirms the host can end their own tournament.
    [Fact]
    public async Task AHostCanEndTheirOwnTournament()
    {
        var game = await CreateGameAsync(2);

        var status = await _gameService.EndGameAsync(game.GameId, game.Participants[0].User.Id);

        Assert.Equal(EndGameStatus.ENDED, status);
        Assert.Null(await _gameService.GetGameByIdAsync(game.GameId));
    }

    // Confirms a participant who does not host cannot end the tournament.
    //
    // A game id travels in URLs and gets read aloud across a room, so it is not
    // a credential. Knowing it must not be enough to destroy everyone's evening.
    [Fact]
    public async Task ANonHostParticipantCannotEndTheTournament()
    {
        var game = await CreateGameAsync(2);

        var status = await _gameService.EndGameAsync(game.GameId, game.Participants[1].User.Id);

        Assert.Equal(EndGameStatus.NOT_HOST, status);
        Assert.NotNull(await _gameService.GetGameByIdAsync(game.GameId));
    }

    // Confirms hosting one tournament grants nothing over another.
    [Fact]
    public async Task HostingOneTournamentDoesNotAllowEndingAnother()
    {
        var ownGame = await CreateGameAsync(2);
        var otherGame = await CreateGameAsync(2);

        var status = await _gameService.EndGameAsync(otherGame.GameId, ownGame.Participants[0].User.Id);

        Assert.Equal(EndGameStatus.NOT_HOST, status);
        Assert.NotNull(await _gameService.GetGameByIdAsync(otherGame.GameId));
    }

    // Confirms ending one tournament leaves the others alone.
    [Fact]
    public async Task EndingOneTournamentLeavesOthersRunning()
    {
        var doomedGame = await CreateGameAsync(2);
        var survivingGame = await CreateGameAsync(4);

        await _gameService.StartGameAsync(survivingGame.GameId);

        await _gameService.EndGameAsync(doomedGame.GameId, doomedGame.Participants[0].User.Id);

        var survivingSnapshot = await _gameService.GetBracketSnapshotAsync(survivingGame.GameId);

        Assert.NotNull(survivingSnapshot);
        Assert.True(survivingSnapshot!.GameStarted);
        Assert.Equal(4, (await _gameService.GetPlayersInGame(survivingGame.GameId)).Count);
    }

    // Confirms ending an unknown game is reported as missing, not as a refusal.
    [Fact]
    public async Task EndingAnUnknownGameReportsNotFound()
    {
        var participant = await CreateUserWithPlayerAsync("Nobody");

        var status = await _gameService.EndGameAsync(Guid.NewGuid(), participant.User.Id);

        Assert.Equal(EndGameStatus.GAME_NOT_FOUND, status);
    }

    // Confirms listing games deletes nothing.
    //
    // The sweep used to run inside the listing route, which made a read delete
    // rows and tied cleanup to somebody opening the browser. A timer owns it
    // now, and this pins listing down as a pure read: a game well past its
    // silence window is still there afterwards, because only the sweeper retires
    // games.
    [Fact]
    public async Task ListingGamesRetiresNothing()
    {
        var game = await CreateGameAsync(2);
        var caller = game.Participants[0].User;

        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedGame = await dbContext.Games.FirstAsync(current => current.Id == game.GameId);
            storedGame.LastActivityUtc = DateTime.UtcNow.AddDays(-3);
            await dbContext.SaveChangesAsync();
        }

        var summaries = await _gameService.GetGameSummariesAsync(caller.Id);

        Assert.Contains(summaries, summary => summary.GameId == game.GameId);
        Assert.NotNull(await _gameService.GetGameByIdAsync(game.GameId));
    }

    // Confirms a live tournament is never swept as stale.
    [Fact]
    public async Task PruningLeavesActiveTournamentsAlone()
    {
        var game = await CreateGameAsync(4);
        await _gameService.StartGameAsync(game.GameId);

        await _gameService.PruneStaleGamesAsync();

        Assert.NotNull(await _gameService.GetGameByIdAsync(game.GameId));
    }

    // Confirms a lobby nobody has touched for long enough is swept.
    //
    // The clock is pushed back directly, because waiting out the real silence
    // window in a test is not an option.
    [Fact]
    public async Task PruningRemovesALongAbandonedLobby()
    {
        var abandonedGame = await CreateGameAsync(2);
        var liveGame = await CreateGameAsync(2);

        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedGame = await dbContext.Games.FirstAsync(game => game.Id == abandonedGame.GameId);
            storedGame.LastActivityUtc = DateTime.UtcNow.AddDays(-3);
            await dbContext.SaveChangesAsync();
        }

        var prunedCount = await _gameService.PruneStaleGamesAsync();

        Assert.True(prunedCount >= 1);
        Assert.Null(await _gameService.GetGameByIdAsync(abandonedGame.GameId));
        Assert.NotNull(await _gameService.GetGameByIdAsync(liveGame.GameId));
    }

    // Confirms sweeping a game takes its players with it.
    //
    // Player rows outliving their game would leak into every later count, since
    // players are found by CurrentGameID rather than through the game itself.
    [Fact]
    public async Task PruningRemovesThePlayersOfASweptGame()
    {
        var abandonedGame = await CreateGameAsync(3);

        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedGame = await dbContext.Games.FirstAsync(game => game.Id == abandonedGame.GameId);
            storedGame.LastActivityUtc = DateTime.UtcNow.AddDays(-3);
            await dbContext.SaveChangesAsync();
        }

        await _gameService.PruneStaleGamesAsync();

        Assert.Empty(await _gameService.GetPlayersInGame(abandonedGame.GameId));
    }

    // Confirms a swept game stops appearing in the browser.
    [Fact]
    public async Task ASweptGameNoLongerAppearsInTheListing()
    {
        var abandonedGame = await CreateGameAsync(2);
        var caller = abandonedGame.Participants[0].User;

        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedGame = await dbContext.Games.FirstAsync(game => game.Id == abandonedGame.GameId);
            storedGame.LastActivityUtc = DateTime.UtcNow.AddDays(-3);
            await dbContext.SaveChangesAsync();
        }

        await _gameService.PruneStaleGamesAsync();

        var summaries = await _gameService.GetGameSummariesAsync(caller.Id);

        Assert.DoesNotContain(summaries, summary => summary.GameId == abandonedGame.GameId);
    }

    // Confirms joining a lobby keeps it alive against the sweep.
    //
    // Before LastActivityUtc existed there was only a created date, so a lobby
    // people were actively filling aged exactly like one nobody ever opened.
    [Fact]
    public async Task JoiningALobbyResetsItsAbandonmentClock()
    {
        var game = await CreateGameAsync(2);

        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedGame = await dbContext.Games.FirstAsync(current => current.Id == game.GameId);
            storedGame.LastActivityUtc = DateTime.UtcNow.AddDays(-3);
            await dbContext.SaveChangesAsync();
        }

        var latecomer = await CreateUserWithPlayerAsync("Latecomer");
        _gameService.AddPlayerToGame(latecomer.Player, game.GameId, latecomer.User.Id);

        await _gameService.PruneStaleGamesAsync();

        Assert.NotNull(await _gameService.GetGameByIdAsync(game.GameId));
    }
}
