namespace Services.Brackets;

using Contracts;
using Entities;
using Enums;

internal interface IBracketEngine
{
    BracketMode Mode { get; }

    BracketRuntimeState Initialize(Guid gameId, IReadOnlyList<Player> players);

    bool TryReportMatch(BracketRuntimeState state, Guid matchId, Guid winnerPlayerId);

    BracketSnapshotResponse BuildSnapshot(BracketRuntimeState state);

    CurrentMatchResponse? BuildCurrentMatch(BracketRuntimeState state);
}
