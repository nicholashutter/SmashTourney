namespace Services.Brackets;

using Contracts;
using Entities;
using Enums;

// Runs single-elimination bracket progression and match state transitions.
internal sealed class SingleEliminationEngine : IBracketEngine
{
    public BracketMode Mode => BracketMode.SINGLE_ELIMINATION;

    // Builds the initial single-elimination runtime state from registered players.
    //
    // The full tree is built here rather than grown as results arrive. See
    // BracketTreeBuilder for why.
    public BracketRuntimeState Initialize(Guid gameId, IReadOnlyList<Player> players)
    {
        var seededPlayers = players
            .Where(player => player.Id != Guid.Empty)
            .DistinctBy(player => player.Id)
            .ToList();

        var state = new BracketRuntimeState
        {
            GameId = gameId,
            Mode = Mode,
            GameStarted = true,
            Players = seededPlayers.Select((player, index) => new BracketPlayerRuntime
            {
                PlayerId = player.Id,
                DisplayName = string.IsNullOrWhiteSpace(player.DisplayName) ? $"Player {index + 1}" : player.DisplayName,
                Seed = index + 1,
                Losses = 0,
                Eliminated = false
            }).ToList()
        };

        if (state.Players.Count < 2)
        {
            return state;
        }

        BracketTreeBuilder.BuildWinnersLane(
            state,
            state.Players.Select(player => player.PlayerId).ToList());

        return state;
    }

    // Applies a completed match result and advances the winner along its link.
    public bool TryReportMatch(BracketRuntimeState state, Guid matchId, Guid winnerPlayerId)
    {
        var match = state.Matches.FirstOrDefault(existingMatch => existingMatch.MatchId == matchId);
        if (match is null || match.Status is BracketMatchStatus.PENDING)
        {
            return false;
        }

        if (match.Status is BracketMatchStatus.COMPLETE)
        {
            return match.WinnerId == winnerPlayerId;
        }

        if (match.PlayerOneId is null || match.PlayerTwoId is null)
        {
            return false;
        }

        if (winnerPlayerId != match.PlayerOneId && winnerPlayerId != match.PlayerTwoId)
        {
            return false;
        }

        var loserPlayerId = winnerPlayerId == match.PlayerOneId ? match.PlayerTwoId.Value : match.PlayerOneId.Value;

        match.WinnerId = winnerPlayerId;
        match.Status = BracketMatchStatus.COMPLETE;

        MarkPlayerEliminated(state, loserPlayerId);

        if (match.NextMatchForWinner is null)
        {
            // No onward link means this was the final: its winner takes the
            // bracket.
            state.WinnersChampionId = winnerPlayerId;
            return true;
        }

        var nextMatch = state.Matches.FirstOrDefault(candidate => candidate.MatchId == match.NextMatchForWinner);
        if (nextMatch is not null)
        {
            BracketTreeBuilder.SeatPlayer(nextMatch, match.NextSlotForWinner, winnerPlayerId);
        }

        return true;
    }

    // Creates a read model snapshot of the current single-elimination bracket state.
    public BracketSnapshotResponse BuildSnapshot(BracketRuntimeState state)
    {
        return new BracketSnapshotResponse(
            state.GameId,
            state.Mode,
            state.GameStarted,
            state.IsGrandFinalResetRequired,
            state.Players.Select(player => new BracketPlayerView(
                player.PlayerId,
                player.DisplayName,
                player.Seed,
                player.Losses,
                player.Eliminated
            )).ToList(),
            state.Matches.Select(match => new BracketMatchView(
                match.MatchId,
                match.Lane,
                match.Round,
                match.MatchNumber,
                match.PlayerOneId,
                match.PlayerTwoId,
                match.WinnerId,
                match.Status,
                match.NextMatchForWinner,
                match.NextMatchForLoser
            )).ToList()
        );
    }

    // Returns the next ready winners-bracket match for gameplay orchestration.
    //
    // Only one match is offered at a time. Everyone is in the same room sharing
    // one console, so a second concurrent match would have nowhere to be played.
    public CurrentMatchResponse? BuildCurrentMatch(BracketRuntimeState state)
    {
        var currentMatch = state.Matches
            .Where(match => match.Lane == BracketLane.WINNERS && match.Status == BracketMatchStatus.READY)
            .OrderBy(match => match.Round)
            .ThenBy(match => match.MatchNumber)
            .FirstOrDefault();

        if (currentMatch is null || currentMatch.PlayerOneId is null || currentMatch.PlayerTwoId is null)
        {
            return null;
        }

        return new CurrentMatchResponse(
            state.GameId,
            currentMatch.MatchId,
            currentMatch.Lane,
            currentMatch.Round,
            currentMatch.MatchNumber,
            currentMatch.PlayerOneId.Value,
            currentMatch.PlayerTwoId.Value
        );
    }

    // Marks a player as eliminated after a recorded bracket loss.
    private static void MarkPlayerEliminated(BracketRuntimeState state, Guid playerId)
    {
        var player = state.Players.FirstOrDefault(candidate => candidate.PlayerId == playerId);
        if (player is null)
        {
            return;
        }

        player.Losses = 1;
        player.Eliminated = true;
    }
}
