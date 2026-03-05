namespace Services.Brackets;

using Contracts;
using Entities;
using Enums;

// Runs single-elimination bracket progression and match state transitions.
internal sealed class SingleEliminationEngine : IBracketEngine
{
    public BracketMode Mode => BracketMode.SINGLE_ELIMINATION;

    // Builds the initial single-elimination runtime state from registered players.
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

        SeedInitialRound(state, state.Players.Select(player => player.PlayerId).ToList());
        return state;
    }

    // Applies a completed match result and advances winners to the next round.
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
        AddToWinnersPool(state, match.Round + 1, winnerPlayerId);
        EvaluateChampionTransition(state);
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

    // Seeds first-round winners-bracket matches from the ordered player list.
    private static void SeedInitialRound(BracketRuntimeState state, List<Guid> initialPlayers)
    {
        for (int index = 0; index < initialPlayers.Count; index += 2)
        {
            var playerOne = initialPlayers[index];
            var playerTwo = index + 1 < initialPlayers.Count ? initialPlayers[index + 1] : Guid.Empty;

            if (playerTwo == Guid.Empty)
            {
                AddToWinnersPool(state, 2, playerOne);
                continue;
            }

            state.Matches.Add(new BracketMatchRuntime
            {
                MatchId = Guid.NewGuid(),
                Lane = BracketLane.WINNERS,
                Round = 1,
                MatchNumber = ++state.WinnersMatchCounter,
                PlayerOneId = playerOne,
                PlayerTwoId = playerTwo,
                Status = BracketMatchStatus.READY
            });
        }
    }

    // Queues winners into the next round and materializes matches when pairs are available.
    private static void AddToWinnersPool(BracketRuntimeState state, int round, Guid playerId)
    {
        if (!state.WinnersPools.ContainsKey(round))
        {
            state.WinnersPools[round] = new List<Guid>();
        }

        var pool = state.WinnersPools[round];

        pool.Add(playerId);

        while (pool.Count >= 2)
        {
            var playerOne = pool[0];
            var playerTwo = pool[1];
            pool.RemoveRange(0, 2);

            state.Matches.Add(new BracketMatchRuntime
            {
                MatchId = Guid.NewGuid(),
                Lane = BracketLane.WINNERS,
                Round = round,
                MatchNumber = ++state.WinnersMatchCounter,
                PlayerOneId = playerOne,
                PlayerTwoId = playerTwo,
                Status = BracketMatchStatus.READY
            });
        }
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

    // Detects bracket completion and resolves the winners champion identity.
    private static void EvaluateChampionTransition(BracketRuntimeState state)
    {
        var hasOpenMatches = state.Matches.Any(match =>
            match.Lane == BracketLane.WINNERS &&
            (match.Status == BracketMatchStatus.READY ||
             match.Status == BracketMatchStatus.IN_PROGRESS ||
             match.Status == BracketMatchStatus.PENDING));

        var hasPendingPoolParticipants = state.WinnersPools.Values.Any(pool => pool.Count > 0);

        if (hasOpenMatches || hasPendingPoolParticipants)
        {
            return;
        }

        var champion = state.Players.FirstOrDefault(player => !player.Eliminated);
        if (champion is not null)
        {
            state.WinnersChampionId = champion.PlayerId;
        }
    }
}
