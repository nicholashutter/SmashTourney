namespace Services.Brackets;

using Contracts;
using Entities;
using Enums;

internal sealed class SingleEliminationEngine : IBracketEngine
{
    public BracketMode Mode => BracketMode.SINGLE_ELIMINATION;

    public BracketRuntimeState Initialize(Guid gameId, IReadOnlyList<Player> players)
    {
        return new BracketRuntimeState
        {
            GameId = gameId,
            Mode = Mode,
            GameStarted = true,
            Players = players.Select((player, index) => new BracketPlayerRuntime
            {
                PlayerId = player.Id,
                DisplayName = player.DisplayName,
                Seed = index + 1,
                Losses = 0,
                Eliminated = false
            }).ToList()
        };
    }

    public bool TryReportMatch(BracketRuntimeState state, Guid matchId, Guid winnerPlayerId)
    {
        return false;
    }

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
            new List<BracketMatchView>()
        );
    }

    public CurrentMatchResponse? BuildCurrentMatch(BracketRuntimeState state)
    {
        return null;
    }
}
