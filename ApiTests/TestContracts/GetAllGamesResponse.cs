namespace ApiTests.TestContracts;

using Entities;

// Represents the API response payload for fetching all games.
public sealed class GetAllGamesResponse
{
    public List<Game> Games { get; set; } = new List<Game>();
}
