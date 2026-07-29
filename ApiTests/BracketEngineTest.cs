namespace ApiTests;

using Entities;
using Enums;
using Services.Brackets;

// Verifies bracket tree construction, seeding, and progression for both modes.
//
// These exercise the engines directly rather than through GameService. The
// engines are what own bracket shape, and shape is the thing under test here —
// going through the service would add game setup and persistence to every
// assertion without making any of them stronger.
public class BracketEngineTest
{
    // Builds a list of players with predictable display names for seeding checks.
    private static List<Player> BuildPlayers(int count)
    {
        var players = new List<Player>(count);

        for (var index = 0; index < count; index++)
        {
            players.Add(new Player
            {
                Id = Guid.NewGuid(),
                DisplayName = $"Seed {index + 1}"
            });
        }

        return players;
    }

    // Plays every available match, always awarding the win to player one.
    //
    // Returns the number of matches played so a caller can assert the bracket
    // terminated rather than stalling or looping.
    private static int PlayOutBracket(IBracketEngineHarness harness)
    {
        var played = 0;

        while (played < 200)
        {
            var currentMatch = harness.BuildCurrentMatch();
            if (currentMatch is null)
            {
                break;
            }

            var applied = harness.TryReportMatch(currentMatch.MatchId, currentMatch.PlayerOneId);
            if (!applied)
            {
                break;
            }

            played += 1;
        }

        return played;
    }

    // Wraps an engine and its state so play-out helpers stay readable.
    private interface IBracketEngineHarness
    {
        Contracts.CurrentMatchResponse? BuildCurrentMatch();

        bool TryReportMatch(Guid matchId, Guid winnerPlayerId);
    }

    // Binds one engine to one runtime state.
    private sealed class EngineHarness : IBracketEngineHarness
    {
        private readonly IBracketEngine _engine;
        private readonly BracketRuntimeState _state;

        public EngineHarness(IBracketEngine engine, BracketRuntimeState state)
        {
            _engine = engine;
            _state = state;
        }

        public Contracts.CurrentMatchResponse? BuildCurrentMatch() => _engine.BuildCurrentMatch(_state);

        public bool TryReportMatch(Guid matchId, Guid winnerPlayerId) =>
            _engine.TryReportMatch(_state, matchId, winnerPlayerId);
    }

    [Fact]
    // Confirms the seed order matches the standard bracket reflection pattern.
    public void SeedOrderFollowsStandardBracketPattern()
    {
        Assert.Equal(new[] { 1, 2 }, BracketTreeBuilder.BuildSeedOrder(2));
        Assert.Equal(new[] { 1, 4, 2, 3 }, BracketTreeBuilder.BuildSeedOrder(4));
        Assert.Equal(new[] { 1, 8, 4, 5, 2, 7, 3, 6 }, BracketTreeBuilder.BuildSeedOrder(8));
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(4, 3)]
    [InlineData(8, 7)]
    [InlineData(16, 15)]
    // Confirms the whole single-elimination tree exists before any match is played.
    public void SingleEliminationBuildsCompleteTreeUpFront(int playerCount, int expectedMatches)
    {
        var engine = new SingleEliminationEngine();
        var state = engine.Initialize(Guid.NewGuid(), BuildPlayers(playerCount));

        Assert.Equal(expectedMatches, state.Matches.Count);
    }

    [Fact]
    // Confirms the top seed is paired against the bottom seed in round one.
    //
    // This is what puts byes on the top seeds: GameService appends synthetic bye
    // players last, so they hold the highest seed numbers.
    public void SingleEliminationPairsTopSeedAgainstBottomSeed()
    {
        var players = BuildPlayers(8);
        var engine = new SingleEliminationEngine();
        var state = engine.Initialize(Guid.NewGuid(), players);

        var openingMatch = state.Matches
            .Where(match => match.Round == 1)
            .OrderBy(match => match.MatchNumber)
            .First();

        Assert.Equal(players[0].Id, openingMatch.PlayerOneId);
        Assert.Equal(players[7].Id, openingMatch.PlayerTwoId);
    }

    [Fact]
    // Confirms every non-final match knows which match its winner advances into.
    public void SingleEliminationLinksEveryWinnerToItsNextMatch()
    {
        var engine = new SingleEliminationEngine();
        var state = engine.Initialize(Guid.NewGuid(), BuildPlayers(8));

        var finalRound = state.Matches.Max(match => match.Round);

        foreach (var match in state.Matches.Where(match => match.Round < finalRound))
        {
            Assert.NotNull(match.NextMatchForWinner);
            Assert.InRange(match.NextSlotForWinner, 1, 2);
            Assert.Contains(state.Matches, candidate => candidate.MatchId == match.NextMatchForWinner);
        }

        var finalMatch = state.Matches.Single(match => match.Round == finalRound);
        Assert.Null(finalMatch.NextMatchForWinner);
    }

    [Fact]
    // Confirms only the opening round is playable before any result is reported.
    public void SingleEliminationStartsWithOnlyTheOpeningRoundReady()
    {
        var engine = new SingleEliminationEngine();
        var state = engine.Initialize(Guid.NewGuid(), BuildPlayers(8));

        Assert.All(
            state.Matches.Where(match => match.Round == 1),
            match => Assert.Equal(BracketMatchStatus.READY, match.Status));

        Assert.All(
            state.Matches.Where(match => match.Round > 1),
            match => Assert.Equal(BracketMatchStatus.PENDING, match.Status));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    // Confirms a single-elimination bracket plays to exactly one surviving champion.
    public void SingleEliminationPlaysOutToOneChampion(int playerCount)
    {
        var engine = new SingleEliminationEngine();
        var state = engine.Initialize(Guid.NewGuid(), BuildPlayers(playerCount));

        var played = PlayOutBracket(new EngineHarness(engine, state));

        Assert.Equal(playerCount - 1, played);
        Assert.NotNull(state.WinnersChampionId);
        Assert.Single(state.Players.Where(player => !player.Eliminated));
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(4, 6)]
    [InlineData(8, 14)]
    [InlineData(16, 30)]
    // Confirms the double-elimination tree holds the full 2n-2 match count.
    //
    // Every player but the champion must lose twice, and the champion may lose
    // once, which is what fixes the total at two per player less two.
    public void DoubleEliminationBuildsCompleteTreeUpFront(int playerCount, int expectedMatches)
    {
        var engine = new DoubleEliminationEngine();
        var state = engine.Initialize(Guid.NewGuid(), BuildPlayers(playerCount));

        Assert.Equal(expectedMatches, state.Matches.Count);
        Assert.Single(state.Matches.Where(match => match.Lane == BracketLane.GRAND_FINALS));
    }

    [Fact]
    // Confirms every winners-bracket match routes its loser into the losers lane.
    public void DoubleEliminationLinksWinnersLosersIntoLosersLane()
    {
        var engine = new DoubleEliminationEngine();
        var state = engine.Initialize(Guid.NewGuid(), BuildPlayers(8));

        var winnersMatches = state.Matches.Where(match => match.Lane == BracketLane.WINNERS).ToList();

        Assert.All(winnersMatches, match =>
        {
            Assert.NotNull(match.NextMatchForLoser);
            Assert.Contains(state.Matches, candidate => candidate.MatchId == match.NextMatchForLoser);
        });
    }

    [Fact]
    // Confirms the losers lane feeds the grand final rather than dead-ending.
    public void DoubleEliminationRoutesLosersFinalIntoGrandFinal()
    {
        var engine = new DoubleEliminationEngine();
        var state = engine.Initialize(Guid.NewGuid(), BuildPlayers(8));

        var grandFinal = state.Matches.Single(match => match.Lane == BracketLane.GRAND_FINALS);
        var losersFinal = state.Matches
            .Where(match => match.Lane == BracketLane.LOSERS)
            .OrderByDescending(match => match.Round)
            .First();

        Assert.Equal(grandFinal.MatchId, losersFinal.NextMatchForWinner);
        Assert.Equal(2, losersFinal.NextSlotForWinner);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    // Confirms a double-elimination bracket terminates with one player standing.
    public void DoubleEliminationPlaysOutToOneChampion(int playerCount)
    {
        var engine = new DoubleEliminationEngine();
        var state = engine.Initialize(Guid.NewGuid(), BuildPlayers(playerCount));

        PlayOutBracket(new EngineHarness(engine, state));

        Assert.Null(engine.BuildCurrentMatch(state));
        Assert.Single(state.Players.Where(player => !player.Eliminated));
    }

    [Fact]
    // Confirms no player is eliminated by a single winners-bracket loss.
    public void DoubleEliminationKeepsFirstLossPlayersAlive()
    {
        var engine = new DoubleEliminationEngine();
        var state = engine.Initialize(Guid.NewGuid(), BuildPlayers(8));

        var openingMatch = state.Matches
            .Where(match => match.Lane == BracketLane.WINNERS && match.Round == 1)
            .OrderBy(match => match.MatchNumber)
            .First();

        var loserId = openingMatch.PlayerTwoId!.Value;
        Assert.True(engine.TryReportMatch(state, openingMatch.MatchId, openingMatch.PlayerOneId!.Value));

        var loser = state.Players.Single(player => player.PlayerId == loserId);
        Assert.Equal(1, loser.Losses);
        Assert.False(loser.Eliminated);
    }

    [Fact]
    // Confirms a match cannot be reported before both of its players are known.
    public void PendingMatchesRejectResults()
    {
        var engine = new SingleEliminationEngine();
        var state = engine.Initialize(Guid.NewGuid(), BuildPlayers(8));

        var pendingMatch = state.Matches.First(match => match.Round == 2);

        Assert.False(engine.TryReportMatch(state, pendingMatch.MatchId, Guid.NewGuid()));
    }
}
