namespace Helpers;

//class to define all constants for application
public static class AppConstants
{
    public const string ByeUserId = "00000000-0000-0000-0000-000000000000";
    public const string ServerURL = "http://localhost:5280";
    public const string HubURL = "/hubs/GameServiceHub";
    public const string ClientUrl = "http://localhost:5173";

    public const string DemoUserName = "demoUser";
    public const string DemoUserEmail = "demo@smashtourney.local";
    public const string DemoUserPassword = "DemoP@ssword123!";

    public const int DummyUserSeedCount = 16;
    public const string DummyUserNamePrefix = "dummy";

    // Dev-only: dummy users are seeded with a blank password so the dev launch can
    // be exercised without knowing any credentials. Sign-in works by submitting an
    // empty password field; the auth pipeline hashes "" and matches the stored hash.
    // Production-style validators (12 chars, mixed classes) are bypassed for this
    // path by writing PasswordHash directly instead of going through CreateAsync.
    public const string DummyUserPassword = "";

    public const string EnableDummyUsersConfigKey = "DevelopmentSeed:EnableDummyUsers";

    // Rate limiter applied to sign-in, registration, and the endpoints that will
    // send email to whatever address the caller names.
    public const string AuthRateLimiterPolicy = "auth";
}