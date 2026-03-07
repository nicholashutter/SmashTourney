namespace Services.Brackets;

using Contracts;
using Entities;
using Enums;

// Runs double-elimination bracket progression across winners, losers, and finals lanes.
internal sealed class DoubleEliminationEngine : IBracketEngine
{
    public BracketMode Mode => BracketMode.DOUBLE_ELIMINATION;

    // Builds the initial double-elimination runtime state from registered players.
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
                UserId = player.UserId,
                DisplayName = string.IsNullOrWhiteSpace(player.DisplayName) ? $"Player {index + 1}" : player.DisplayName,
                Seed = index + 1,
                Losses = 0,
                Eliminated = false
            }).ToList()
        };

        SeedInitialWinnersRound(state, state.Players.Select(player => player.PlayerId).ToList());
        return state;
    }

    // Applies a completed match result and routes players through double-elimination lanes.
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

        switch (match.Lane)
        {
            case BracketLane.WINNERS:
                AddToWinnersPool(state, match.Round + 1, winnerPlayerId);
                RegisterLossAndDrop(state, loserPlayerId, match.Round);
                break;
            case BracketLane.LOSERS:
                AddToLosersPool(state, match.Round + 1, winnerPlayerId);
                RegisterLossAndDrop(state, loserPlayerId, match.Round + 1, isLosersMatch: true);
                break;
            case BracketLane.GRAND_FINALS:
                HandleGrandFinalResult(state, winnerPlayerId, loserPlayerId);
                break;
            case BracketLane.GRAND_FINALS_RESET:
                HandleGrandFinalResetResult(state, loserPlayerId);
                break;
        }

        EvaluateChampionTransitions(state);
        return true;
    }

    // Creates a read model snapshot of the current double-elimination bracket state.
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

    // Returns the next ready bracket match based on lane and round priority.
    public CurrentMatchResponse? BuildCurrentMatch(BracketRuntimeState state)
    {
        var currentMatch = state.Matches
            .Where(match => match.Status == BracketMatchStatus.READY)
            .OrderBy(match => MatchLanePriority(match.Lane))
            .ThenBy(match => match.Round)
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
    private static void SeedInitialWinnersRound(BracketRuntimeState state, List<Guid> initialPlayers)
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

    // Queues winners-lane players and materializes matches when pairs are available.
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

    // Queues losers-lane players and materializes matches when pairs are available.
    private static void AddToLosersPool(BracketRuntimeState state, int round, Guid playerId)
    {
        if (!state.LosersPools.ContainsKey(round))
        {
            state.LosersPools[round] = new List<Guid>();
        }

        var pool = state.LosersPools[round];

        pool.Add(playerId);

        while (pool.Count >= 2)
        {
            var playerOne = pool[0];
            var playerTwo = pool[1];
            pool.RemoveRange(0, 2);

            state.Matches.Add(new BracketMatchRuntime
            {
                MatchId = Guid.NewGuid(),
                Lane = BracketLane.LOSERS,
                Round = round,
                MatchNumber = ++state.LosersMatchCounter,
                PlayerOneId = playerOne,
                PlayerTwoId = playerTwo,
                Status = BracketMatchStatus.READY
            });
        }
    }

    // Tracks player losses and drops eligible competitors into the losers bracket.
    private static void RegisterLossAndDrop(BracketRuntimeState state, Guid playerId, int round, bool isLosersMatch = false)
    {
        var player = state.Players.FirstOrDefault(candidate => candidate.PlayerId == playerId);
        if (player is null)
        {
            return;
        }

        player.Losses += 1;
        if (player.Losses >= 2)
        {
            player.Eliminated = true;
            return;
        }

        var losersRound = isLosersMatch ? round + 1 : Math.Max(1, round);
        AddToLosersPool(state, losersRound, playerId);
    }

    // Resolves lane champions and creates grand finals matches when ready.
    private static void EvaluateChampionTransitions(BracketRuntimeState state)
    {
        if (state.WinnersChampionId is null && !HasOpenMatches(state, BracketLane.WINNERS))
        {
            state.WinnersChampionId = TryResolveLaneChampion(state, BracketLane.WINNERS);
        }

        if (state.LosersChampionId is null && !HasOpenMatches(state, BracketLane.LOSERS))
        {
            state.LosersChampionId = TryResolveLosersChampion(state);
        }

        if (state.WinnersChampionId is not null && state.LosersChampionId is not null)
        {
            var finalsExists = state.Matches.Any(match =>
                match.Lane is BracketLane.GRAND_FINALS or BracketLane.GRAND_FINALS_RESET);

            if (!finalsExists)
            {
                state.Matches.Add(new BracketMatchRuntime
                {
                    MatchId = Guid.NewGuid(),
                    Lane = BracketLane.GRAND_FINALS,
                    Round = 1,
                    MatchNumber = ++state.FinalsMatchCounter,
                    PlayerOneId = state.WinnersChampionId,
                    PlayerTwoId = state.LosersChampionId,
                    Status = BracketMatchStatus.READY
                });
            }
        }
    }

    // Processes grand finals results and schedules reset finals when required.
    private static void HandleGrandFinalResult(BracketRuntimeState state, Guid winnerPlayerId, Guid loserPlayerId)
    {
        if (winnerPlayerId == state.WinnersChampionId)
        {
            MarkPlayerEliminated(state, loserPlayerId);
            return;
        }

        state.IsGrandFinalResetRequired = true;

        state.Matches.Add(new BracketMatchRuntime
        {
            MatchId = Guid.NewGuid(),
            Lane = BracketLane.GRAND_FINALS_RESET,
            Round = 1,
            MatchNumber = ++state.FinalsMatchCounter,
            PlayerOneId = state.WinnersChampionId,
            PlayerTwoId = state.LosersChampionId,
            Status = BracketMatchStatus.READY
        });
    }

    // Processes grand finals reset results to finalize elimination outcomes.
    private static void HandleGrandFinalResetResult(BracketRuntimeState state, Guid loserPlayerId)
    {
        MarkPlayerEliminated(state, loserPlayerId);
    }

    // Marks a player as eliminated once bracket loss conditions are met.
    private static void MarkPlayerEliminated(BracketRuntimeState state, Guid playerId)
    {
        var player = state.Players.FirstOrDefault(candidate => candidate.PlayerId == playerId);
        if (player is null)
        {
            return;
        }

        player.Losses = Math.Max(player.Losses, 2);
        player.Eliminated = true;
    }

    // Checks whether a lane still has ready, active, or pending matches.
    private static bool HasOpenMatches(BracketRuntimeState state, BracketLane lane)
    {
        return state.Matches.Any(match =>
            match.Lane == lane &&
            (match.Status == BracketMatchStatus.READY ||
             match.Status == BracketMatchStatus.IN_PROGRESS ||
             match.Status == BracketMatchStatus.PENDING));
    }

    // Resolves a lane champion from the latest completed match outcomes.
    private static Guid? TryResolveLaneChampion(BracketRuntimeState state, BracketLane lane)
    {
        var completedMatches = state.Matches
            .Where(match => match.Lane == lane && match.Status == BracketMatchStatus.COMPLETE && match.WinnerId is not null)
            .OrderByDescending(match => match.Round)
            .ThenByDescending(match => match.MatchNumber)
            .ToList();

        return completedMatches.FirstOrDefault()?.WinnerId;
    }

    // Resolves the losers-lane champion from active eligibility and outcomes.
    private static Guid? TryResolveLosersChampion(BracketRuntimeState state)
    {
        var eligible = state.Players
            .Where(player => !player.Eliminated && player.Losses == 1)
            .Select(player => player.PlayerId)
            .ToList();

        if (eligible.Count == 1)
        {
            return eligible[0];
        }

        var lastLosersWinner = state.Matches
            .Where(match => match.Lane == BracketLane.LOSERS && match.Status == BracketMatchStatus.COMPLETE && match.WinnerId is not null)
            .OrderByDescending(match => match.Round)
            .ThenByDescending(match => match.MatchNumber)
            .FirstOrDefault();

        return lastLosersWinner?.WinnerId;
    }

    // Defines lane ordering used to choose the next playable bracket match.
    private static int MatchLanePriority(BracketLane lane)
    {
        return lane switch
        {
            BracketLane.WINNERS => 0,
            BracketLane.LOSERS => 1,
            BracketLane.GRAND_FINALS => 2,
            BracketLane.GRAND_FINALS_RESET => 3,
            _ => 4
        };
    }
}
