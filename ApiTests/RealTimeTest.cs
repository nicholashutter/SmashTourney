namespace ApiTests;

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Services;

// Verifies realtime broadcast behavior that keeps clients synchronized during tournament play.
public class RealTimeTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private const string InMemoryHubUrl = "wss://localhost/hubs/GameServiceHub";

    // Initializes realtime test host access and ensures database availability.
    //
    // These tests previously never touched the database, because an anonymous
    // hub connection needed no account. Now that the hub requires a session
    // they register a user first, so the identity schema has to exist.
    public RealTimeTest(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;

        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        database.Database.EnsureCreated();
    }

    // Attaches a captured identity cookie to every hub request.
    //
    // The test server's handler carries no cookie jar, so the negotiate call
    // would arrive anonymous and be refused. Replaying the cookie by hand is the
    // smallest way to give the hub connection a real session.
    private sealed class AuthenticatedHubHandler : DelegatingHandler
    {
        private readonly string _cookieHeader;

        public AuthenticatedHubHandler(HttpMessageHandler innerHandler, string cookieHeader)
            : base(innerHandler)
        {
            _cookieHeader = cookieHeader;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Remove("Cookie");
            request.Headers.Add("Cookie", _cookieHeader);
            return base.SendAsync(request, cancellationToken);
        }
    }

    // Registers a user, signs in, and returns the resulting cookie header.
    private async Task<string> SignInAndCaptureCookieAsync()
    {
        using var client = new HttpClient(_factory.Server.CreateHandler())
        {
            BaseAddress = new Uri("https://localhost")
        };

        var credentials = new
        {
            Email = $"realtime_{Guid.NewGuid()}@example.com",
            Password = "SecureP@ssw0rd123!"
        };

        var registerResponse = await client.PostAsJsonAsync("/register", credentials);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/login?useCookies=true", credentials);
        loginResponse.EnsureSuccessStatusCode();

        var setCookieValues = loginResponse.Headers.GetValues("Set-Cookie");
        return string.Join("; ", setCookieValues.Select(value => value.Split(';')[0]));
    }

    // Creates an authenticated SignalR client connected to the in-memory server.
    private async Task<HubConnection> CreateClientAsync()
    {
        var server = _factory.Server;
        var cookieHeader = await SignInAndCaptureCookieAsync();

        return new HubConnectionBuilder()
            .WithUrl(InMemoryHubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ =>
                    new AuthenticatedHubHandler(server.CreateHandler(), cookieHeader);
            })
            .Build();
    }

    // Confirms the hub route is registered and reachable.
    [Fact]
    public async Task HubRouteShouldBeReachable()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(InMemoryHubUrl);

        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Confirms an anonymous client cannot open a hub connection.
    [Fact]
    public async Task HubRefusesUnauthenticatedConnection()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(InMemoryHubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await Assert.ThrowsAnyAsync<Exception>(() => connection.StartAsync());
    }

    // Confirms a newly connected client receives the join acknowledgment event.
    [Fact]
    public async Task SuccessMessageOnPlayerConnection()
    {
        const string expectedResult = "Success";

        var completionSource = new TaskCompletionSource<string>();
        var connection = await CreateClientAsync();

        connection.On("Successfully Joined", () =>
        {
            completionSource.TrySetResult(expectedResult);
        });

        await connection.StartAsync();

        var actualResult = await completionSource.Task;

        Assert.Equal(expectedResult, actualResult);
    }

    // Confirms game-start notifications are broadcast to all clients in the same game group.
    [Fact]
    public async Task NotifyGameStartedBroadcastsToAllClientsInGroup()
    {
        var gameId = Guid.NewGuid().ToString();

        var hostConnection = await CreateClientAsync();
        var guestConnection = await CreateClientAsync();

        var hostReceived = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var guestReceived = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        hostConnection.On<string>("GameStarted", receivedGameId =>
        {
            hostReceived.TrySetResult(receivedGameId);
        });

        guestConnection.On<string>("GameStarted", receivedGameId =>
        {
            guestReceived.TrySetResult(receivedGameId);
        });

        await hostConnection.StartAsync();
        await guestConnection.StartAsync();

        await hostConnection.InvokeAsync("JoinGameGroup", gameId);
        await guestConnection.InvokeAsync("JoinGameGroup", gameId);

        await hostConnection.InvokeAsync("NotifyGameStarted", gameId);

        var hostGameStarted = await hostReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var guestGameStarted = await guestReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var bothClientsReceivedExpectedGameId = hostGameStarted == gameId && guestGameStarted == gameId;
        Assert.True(bothClientsReceivedExpectedGameId);

        await hostConnection.DisposeAsync();
        await guestConnection.DisposeAsync();
    }
}
