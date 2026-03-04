namespace ApiTests.TestContracts;

// Represents credentials used for register and login test flows.
public sealed class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
