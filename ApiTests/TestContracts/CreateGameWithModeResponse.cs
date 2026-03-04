namespace ApiTests.TestContracts;

using Enums;

// Represents the API response payload for creating a game with a bracket mode.
public sealed class CreateGameWithModeResponse
{
    public Guid GameId { get; set; }
    public BracketMode BracketMode { get; set; }
}
