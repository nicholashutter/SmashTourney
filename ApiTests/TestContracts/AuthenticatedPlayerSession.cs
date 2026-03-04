namespace ApiTests.TestContracts;

using Entities;

// Represents one authenticated test client and the player it controls.
public sealed class AuthenticatedPlayerSession
{
    public HttpClient Client { get; set; } = null!;
    public Player Player { get; set; } = new Player();
}
