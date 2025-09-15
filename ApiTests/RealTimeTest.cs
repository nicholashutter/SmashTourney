using ApiTests;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Services;

public class RealTimeTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    private readonly IHubContext<ConnectionHub> _hubContext;

    public RealTimeTest()
    {
        _factory = new CustomWebApplicationFactory<Program>();

        using var scope = _factory.Services.CreateScope();

        _hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ConnectionHub>>();
    }

    [Fact]
    public async Task SuccessMessageOnPlayerConnection()
    {
        var received = false;

        var client = new HubConnectionBuilder()
            .WithUrl("http://localhost/hub", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .WithAutomaticReconnect()
            .Build();

        client.On("Successfully Joined", () =>
        {
            received = true;
        });
        await client.StartAsync();

        await Task.Delay(500);

        Assert.True(received);
    }

    [Fact]
    public async Task UpdateOthersOnPlayerConnection()
    {

        var receivedPayload = string.Empty;

        var client1 = new HubConnectionBuilder()
            .WithUrl("http://localhost/hub", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .WithAutomaticReconnect()
            .Build();

        var client2 = new HubConnectionBuilder()
            .WithUrl("http://localhost/hub", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .WithAutomaticReconnect()
            .Build();

        client1.On<string>("ReceiveMessage", payload =>
        {
            receivedPayload = payload;
        });

        await client1.StartAsync();
        await client2.StartAsync();


        var testGameId = Guid.NewGuid().ToString();
        await client2.InvokeAsync("UpdatePlayers", testGameId);


        await Task.Delay(500);


        Assert.False(string.IsNullOrEmpty(receivedPayload));
    }

    [Fact]
    public async Task NofityOthersOnPlayerConnection()
    {

        var receivedMessage = string.Empty;

        var client1 = new HubConnectionBuilder()
            .WithUrl("http://localhost/hub", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .WithAutomaticReconnect()
            .Build();

        var client2 = new HubConnectionBuilder()
            .WithUrl("http://localhost/hub", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .WithAutomaticReconnect()
            .Build();

        client1.On<string>("ReceiveMessage", message =>
        {
            receivedMessage = message;
        });

        await client1.StartAsync();
        await client2.StartAsync();


        var playerName = "Nicholas";
        await client2.InvokeAsync("NotifyOthers", playerName);


        await Task.Delay(500);


        Assert.Equal($"{playerName} Joined!", receivedMessage);
    }
}