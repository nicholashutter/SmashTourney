namespace ApiTests;

using Contracts;
using Entities;
using Enums;
using Microsoft.Extensions.DependencyInjection;
using Services;

// Verifies that a dropped player can be put back where they were, and that a
// restarted server still knows what the tournament was.
//
// Both halves of that depend on the same thing: nothing about a running game
// may live only in a browser tab or only in a process. These tests take those
// two things away on purpose and check what is left.
public class SessionResumeTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly IGameService _gameService;

    public SessionResumeTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();

        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        database.Database.EnsureCreated();

        _gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
    }

    // Builds a game service with no in-memory state, as a restarted process has.
    //
    // The test database is an in-memory SQLite connection held open by the host,
    // so it outlives this instance exactly the way a real database file outlives
    // a process. Anything this service can answer, it answered from storage.
    private IGameService CreateColdGameService()
    {
        return new GameService(_factory.Services);
    }

    // Creates a user and a matching player row.
    private async Task<(ApplicationUser User, Player Player)> CreateUserWithPlayerAsync(string displayName)
    {
        using var scope = _factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        var playerManager = scope.ServiceProvider.GetRequiredService<IPlayerManager>();

        var userId = Guid.NewGuid();
        var email = $"resume{userId}@email.com";

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

    // Creates a game owned by the first user, with every user joined to it.
    private async Task<(Guid GameId, List<(ApplicationUser User, Player Player)> Participants)> CreateGameWithParticipantsAsync(int participantCount)
    {
        var participants = new List<(ApplicationUser User, Player Player)>();

        for (var index = 0; index < participantCount; index++)
        {
            participants.Add(await CreateUserWithPlayerAsync($"Player {index + 1}"));
        }

        var hostUserId = participants[0].User.Id;

        var gameId = await _gameService.CreateGame(
            new CreateGameOptions(BracketMode.SINGLE_ELIMINATION, participantCount),
            hostUserId);

        foreach (var participant in participants)
        {
            _gameService.AddPlayerToGame(participant.Player, gameId, participant.User.Id);
        }

        return (gameId, participants);
    }

    // Confirms a returning player is identified from their game and user alone.
    [Fact]
    public async Task GetPlayerSessionResolvesTheReturningPlayer()
    {
        var (gameId, participants) = await CreateGameWithParticipantsAsync(2);
        var returning = participants[1];

        var session = await _gameService.GetPlayerSessionAsync(gameId, returning.User.Id);

        Assert.NotNull(session);
        Assert.Equal(gameId, session!.GameId);
        Assert.Equal(returning.Player.Id, session.PlayerId);
        Assert.Equal("Player 2", session.DisplayName);
    }

    // Confirms the creator is recognised as host after losing their tab.
    //
    // Host-ness used to be a flag in sessionStorage, so a host who dropped came
    // back as an ordinary player and the tournament could not be started by
    // anyone.
    [Fact]
    public async Task GetPlayerSessionRestoresHostForTheGameCreator()
    {
        var (gameId, participants) = await CreateGameWithParticipantsAsync(2);

        var hostSession = await _gameService.GetPlayerSessionAsync(gameId, participants[0].User.Id);
        var guestSession = await _gameService.GetPlayerSessionAsync(gameId, participants[1].User.Id);

        Assert.True(hostSession!.IsHost);
        Assert.False(guestSession!.IsHost);
    }

    // Confirms a user with no player in the game gets nothing back.
    [Fact]
    public async Task GetPlayerSessionReturnsNullForSomeoneNotInTheGame()
    {
        var (gameId, _) = await CreateGameWithParticipantsAsync(2);
        var outsider = await CreateUserWithPlayerAsync("Outsider");

        var session = await _gameService.GetPlayerSessionAsync(gameId, outsider.User.Id);

        Assert.Null(session);
    }

    // Confirms an unknown game is not mistaken for a session.
    [Fact]
    public async Task GetPlayerSessionReturnsNullForAnUnknownGame()
    {
        var participant = await CreateUserWithPlayerAsync("Nobody");

        var session = await _gameService.GetPlayerSessionAsync(Guid.NewGuid(), participant.User.Id);

        Assert.Null(session);
    }

    // Confirms a returning player is told which screen their game is on.
    [Fact]
    public async Task GetPlayerSessionReportsLobbyBeforeTheGameStarts()
    {
        var (gameId, participants) = await CreateGameWithParticipantsAsync(2);

        var session = await _gameService.GetPlayerSessionAsync(gameId, participants[0].User.Id);

        Assert.Equal(GameState.LOBBY_WAITING, session!.State);
        Assert.False(session.GameStarted);
    }

    // Confirms a player who drops mid-tournament is sent back into the match.
    [Fact]
    public async Task GetPlayerSessionReportsAnActiveMatchAfterTheGameStarts()
    {
        var (gameId, participants) = await CreateGameWithParticipantsAsync(2);
        await _gameService.StartGameAsync(gameId);

        var session = await _gameService.GetPlayerSessionAsync(gameId, participants[1].User.Id);

        Assert.True(session!.GameStarted);
        Assert.Equal(GameState.IN_MATCH_ACTIVE, session.State);
    }

    // Confirms a restarted server can still resume a session.
    //
    // Nothing in the answer came from the instance that started the game, so
    // this is the reconnect path working across a process boundary.
    [Fact]
    public async Task GetPlayerSessionSurvivesAServiceRestart()
    {
        var (gameId, participants) = await CreateGameWithParticipantsAsync(2);
        await _gameService.StartGameAsync(gameId);

        var coldService = CreateColdGameService();
        var session = await coldService.GetPlayerSessionAsync(gameId, participants[0].User.Id);

        Assert.NotNull(session);
        Assert.True(session!.IsHost);
        Assert.True(session.GameStarted);
        Assert.Equal(participants[0].Player.Id, session.PlayerId);
    }

    // Confirms the bracket itself is rebuilt from storage after a restart.
    [Fact]
    public async Task BracketSnapshotSurvivesAServiceRestart()
    {
        var (gameId, _) = await CreateGameWithParticipantsAsync(4);
        await _gameService.StartGameAsync(gameId);

        var beforeRestart = await _gameService.GetBracketSnapshotAsync(gameId);
        Assert.NotNull(beforeRestart);

        var coldService = CreateColdGameService();
        var afterRestart = await coldService.GetBracketSnapshotAsync(gameId);

        Assert.NotNull(afterRestart);
        Assert.True(afterRestart!.GameStarted);
        Assert.Equal(beforeRestart!.Matches.Count, afterRestart.Matches.Count);
        Assert.Equal(beforeRestart.Players.Count, afterRestart.Players.Count);
    }

    // Confirms results already reported are still reported after a restart.
    [Fact]
    public async Task ReportedMatchResultsSurviveAServiceRestart()
    {
        var (gameId, participants) = await CreateGameWithParticipantsAsync(4);
        await _gameService.StartGameAsync(gameId);

        var firstMatch = await _gameService.GetCurrentMatchAsync(gameId);
        Assert.NotNull(firstMatch);

        var winnerId = firstMatch!.PlayerOneId;
        var playerOne = participants.Single(participant => participant.Player.Id == firstMatch.PlayerOneId);
        var playerTwo = participants.Single(participant => participant.Player.Id == firstMatch.PlayerTwoId);
        var voteRequest = new SubmitMatchVoteRequest(firstMatch.MatchId, winnerId);

        var pendingVote = await _gameService.SubmitMatchVoteAsync(gameId, playerOne.User.Id, voteRequest);
        Assert.Equal(SubmitMatchVoteStatus.PENDING, pendingVote.Status);

        var committingVote = await _gameService.SubmitMatchVoteAsync(gameId, playerTwo.User.Id, voteRequest);
        Assert.Equal(SubmitMatchVoteStatus.COMMITTED, committingVote.Status);

        var coldService = CreateColdGameService();
        var snapshot = await coldService.GetBracketSnapshotAsync(gameId);

        var reportedMatch = snapshot!.Matches.Single(match => match.MatchId == firstMatch.MatchId);
        Assert.Equal(winnerId, reportedMatch.WinnerId);
    }

    // Confirms a vote waiting on the other player is not lost to a restart.
    //
    // This is the state most exposed to an interruption: one player has tapped a
    // winner and the other is still deciding. It used to live in a field on the
    // service, so a restart in that window silently discarded the first vote.
    // The restarted service rejecting the same vote as a duplicate is proof it
    // was written down.
    [Fact]
    public async Task PendingVoteSurvivesAServiceRestart()
    {
        var (gameId, participants) = await CreateGameWithParticipantsAsync(2);
        await _gameService.StartGameAsync(gameId);

        var activeMatch = await _gameService.GetCurrentMatchAsync(gameId);
        Assert.NotNull(activeMatch);

        var voter = participants[0];
        var firstVote = await _gameService.SubmitMatchVoteAsync(
            gameId,
            voter.User.Id,
            new SubmitMatchVoteRequest(activeMatch!.MatchId, activeMatch.PlayerOneId));

        Assert.Equal(SubmitMatchVoteStatus.PENDING, firstVote.Status);

        var coldService = CreateColdGameService();
        var repeatVote = await coldService.SubmitMatchVoteAsync(
            gameId,
            voter.User.Id,
            new SubmitMatchVoteRequest(activeMatch.MatchId, activeMatch.PlayerOneId));

        Assert.Equal(SubmitMatchVoteStatus.DUPLICATE_VOTE, repeatVote.Status);
    }

    // Confirms the second player's agreement still commits the match after a restart.
    //
    // Surviving is not enough on its own — the recovered vote has to still count
    // toward consensus, or a restart would quietly strand the match forever.
    [Fact]
    public async Task RecoveredVoteStillCountsTowardConsensusAfterARestart()
    {
        var (gameId, participants) = await CreateGameWithParticipantsAsync(2);
        await _gameService.StartGameAsync(gameId);

        var activeMatch = await _gameService.GetCurrentMatchAsync(gameId);
        Assert.NotNull(activeMatch);

        var agreedWinnerId = activeMatch!.PlayerOneId;

        var firstVote = await _gameService.SubmitMatchVoteAsync(
            gameId,
            participants[0].User.Id,
            new SubmitMatchVoteRequest(activeMatch.MatchId, agreedWinnerId));

        Assert.Equal(SubmitMatchVoteStatus.PENDING, firstVote.Status);

        var coldService = CreateColdGameService();
        var secondVote = await coldService.SubmitMatchVoteAsync(
            gameId,
            participants[1].User.Id,
            new SubmitMatchVoteRequest(activeMatch.MatchId, agreedWinnerId));

        Assert.Equal(SubmitMatchVoteStatus.COMMITTED, secondVote.Status);
        Assert.Equal(agreedWinnerId, secondVote.CommittedWinnerPlayerId);
    }

    // Confirms simultaneous votes from both phones resolve to one outcome.
    //
    // Two players tapping at the same moment is the normal case here, not a rare
    // race. Without a gate around read-modify-persist, both requests could read
    // the same state and each write their own version of what happened.
    [Fact]
    public async Task ConcurrentVotesFromBothPlayersCommitExactlyOnce()
    {
        var (gameId, participants) = await CreateGameWithParticipantsAsync(2);
        await _gameService.StartGameAsync(gameId);

        var activeMatch = await _gameService.GetCurrentMatchAsync(gameId);
        Assert.NotNull(activeMatch);

        var agreedWinnerId = activeMatch!.PlayerOneId;
        var request = new SubmitMatchVoteRequest(activeMatch.MatchId, agreedWinnerId);

        var votes = await Task.WhenAll(
            _gameService.SubmitMatchVoteAsync(gameId, participants[0].User.Id, request),
            _gameService.SubmitMatchVoteAsync(gameId, participants[1].User.Id, request));

        Assert.Single(votes, vote => vote.Status == SubmitMatchVoteStatus.COMMITTED);
        Assert.Single(votes, vote => vote.Status == SubmitMatchVoteStatus.PENDING);

        var snapshot = await _gameService.GetBracketSnapshotAsync(gameId);
        var votedMatch = snapshot!.Matches.Single(match => match.MatchId == activeMatch.MatchId);
        Assert.Equal(agreedWinnerId, votedMatch.WinnerId);
    }
}
