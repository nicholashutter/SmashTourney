namespace Services.Brackets;

using Contracts;
using Entities;
using Enums;

// Runs double-elimination bracket progression across winners, losers, and finals lanes.
internal sealed class DoubleEliminationEngine : IBracketEngine
{
    public BracketMode Mode => BracketMode.DOUBLE_ELIMINATION;

    // Builds the initial double-elimination runtime state from registered players.
    //
    // Both lanes and the grand final are materialized here. Only the reset final
    // is created on demand, because whether it exists at all depends on who wins
    // the grand final.
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

        var bracketSize = state.Players.Count;
        var winnersRounds = BracketTreeBuilder.BuildWinnersLane(
            state,
            state.Players.Select(player => player.PlayerId).ToList());

        var losersRounds = BracketTreeBuilder.BuildLosersLane(state, bracketSize);
        BracketTreeBuilder.LinkWinnersLosersToLosersLane(winnersRounds, losersRounds);

        var grandFinal = new BracketMatchRuntime
        {
            MatchId = Guid.NewGuid(),
            Lane = BracketLane.GRAND_FINALS,
            Round = 1,
            MatchNumber = ++state.FinalsMatchCounter,
            Status = BracketMatchStatus.PENDING
        };

        state.Matches.Add(grandFinal);

        // The winners-bracket final sends its winner to the grand final.
        var winnersFinal = winnersRounds[^1][0];
        winnersFinal.NextMatchForWinner = grandFinal.MatchId;
        winnersFinal.NextSlotForWinner = 1;

        if (losersRounds.Count == 0)
        {
            // A two-player bracket has no losers lane, so the single winners
            // match sends its loser straight into the grand final.
            winnersFinal.NextMatchForLoser = grandFinal.MatchId;
            winnersFinal.NextSlotForLoser = 2;
        }
        else
        {
            var losersFinal = losersRounds[^1][0];
            losersFinal.NextMatchForWinner = grandFinal.MatchId;
            losersFinal.NextSlotForWinner = 2;
        }

        return state;
    }

    // Applies a completed match result and routes players through their links.
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
                if (match.NextMatchForWinner is null)
                {
                    state.WinnersChampionId = winnerPlayerId;
                }
                else
                {
                    AdvanceWinner(state, match, winnerPlayerId);
                    if (IsLaneFinal(state, match))
                    {
                        state.WinnersChampionId = winnerPlayerId;
                    }
                }

                DropLoser(state, match, loserPlayerId);
                break;

            case BracketLane.LOSERS:
                if (match.NextMatchForWinner is null)
                {
                    state.LosersChampionId = winnerPlayerId;
                }
                else
                {
                    AdvanceWinner(state, match, winnerPlayerId);
                    if (IsLaneFinal(state, match))
                    {
                        state.LosersChampionId = winnerPlayerId;
                    }
                }

                MarkPlayerEliminated(state, loserPlayerId);
                break;

            case BracketLane.GRAND_FINALS:
                HandleGrandFinalResult(state, winnerPlayerId, loserPlayerId);
                break;

            case BracketLane.GRAND_FINALS_RESET:
                MarkPlayerEliminated(state, loserPlayerId);
                break;
        }

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
    //
    // Only one match is offered at a time — everyone shares one console, so a
    // second concurrent match would have nowhere to be played.
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

    // Seats a winner into the slot reserved for it in the next match.
    private static void AdvanceWinner(BracketRuntimeState state, BracketMatchRuntime match, Guid winnerPlayerId)
    {
        var nextMatch = state.Matches.FirstOrDefault(candidate => candidate.MatchId == match.NextMatchForWinner);
        if (nextMatch is not null)
        {
            BracketTreeBuilder.SeatPlayer(nextMatch, match.NextSlotForWinner, winnerPlayerId);
        }
    }

    // Records a first loss and drops the player into their losers-lane slot.
    //
    // A winners-bracket loss is never elimination on its own — that is the point
    // of the format. A player only leaves once they have lost twice, or once
    // there is nowhere left to drop them.
    private static void DropLoser(BracketRuntimeState state, BracketMatchRuntime match, Guid loserPlayerId)
    {
        var player = state.Players.FirstOrDefault(candidate => candidate.PlayerId == loserPlayerId);
        if (player is null)
        {
            return;
        }

        player.Losses += 1;

        if (match.NextMatchForLoser is null)
        {
            player.Eliminated = true;
            return;
        }

        if (player.Losses >= 2)
        {
            player.Eliminated = true;
            return;
        }

        var nextMatch = state.Matches.FirstOrDefault(candidate => candidate.MatchId == match.NextMatchForLoser);
        if (nextMatch is not null)
        {
            BracketTreeBuilder.SeatPlayer(nextMatch, match.NextSlotForLoser, loserPlayerId);
        }
    }

    // Reports whether a match is the last one in its own lane.
    private static bool IsLaneFinal(BracketRuntimeState state, BracketMatchRuntime match)
    {
        return !state.Matches.Any(candidate =>
            candidate.Lane == match.Lane && candidate.Round > match.Round);
    }

    // Processes grand finals results and schedules reset finals when required.
    //
    // The winners-bracket champion arrives holding no losses, so a single defeat
    // here only levels the tie — the reset final is what actually decides it.
    private static void HandleGrandFinalResult(BracketRuntimeState state, Guid winnerPlayerId, Guid loserPlayerId)
    {
        if (winnerPlayerId == state.WinnersChampionId)
        {
            MarkPlayerEliminated(state, loserPlayerId);
            return;
        }

        state.IsGrandFinalResetRequired = true;

        var loserPlayer = state.Players.FirstOrDefault(candidate => candidate.PlayerId == loserPlayerId);
        if (loserPlayer is not null)
        {
            loserPlayer.Losses = Math.Max(loserPlayer.Losses, 1);
        }

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
