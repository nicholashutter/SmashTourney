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
    public const string DummyUserPasswordPrefix = "DummyPass!";

    public const string EnableDummyUsersConfigKey = "DevelopmentSeed:EnableDummyUsers";
}