namespace Services.Brackets;

using Contracts;
using Entities;
using Enums;

// Defines bracket engine behavior for initialization, progression, and projections.
internal interface IBracketEngine
{
    // Returns the bracket mode supported by this engine.
    BracketMode Mode { get; }

    // Initializes runtime bracket state from game and player inputs.
    BracketRuntimeState Initialize(Guid gameId, IReadOnlyList<Player> players);

    // Applies one match result to the runtime bracket state.
    bool TryReportMatch(BracketRuntimeState state, Guid matchId, Guid winnerPlayerId);

    // Builds a full bracket snapshot for client rendering.
    BracketSnapshotResponse BuildSnapshot(BracketRuntimeState state);

    // Builds the current active match, if one exists.
    CurrentMatchResponse? BuildCurrentMatch(BracketRuntimeState state);
}
