namespace ApiTests.TestContracts;

using Entities;

// Represents the API response payload for fetching one game by identifier.
public sealed class GetGameByIdResponse
{
    public Game Game { get; set; } = new Game();
}
