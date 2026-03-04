namespace ApiTests.TestContracts;

using Entities;

// Represents one complete game setup result used by service-level tests.
public sealed class GameSetupResult
{
    public Guid GameId { get; set; }
    public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public List<Player> Players { get; set; } = new List<Player>();
}
