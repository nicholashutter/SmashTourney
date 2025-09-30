using System.Net;
using ApiTests;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Services;

public class RealTimeTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    private const string IN_MEMORY_HUB_URL = "wss://localhost/hubs/GameServiceHub";

    public RealTimeTest(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;

    }

    private HubConnection CreateClient()
    {
        var client = _factory.CreateClient();
        var server = _factory.Server;
        return new HubConnectionBuilder()
        .WithUrl(IN_MEMORY_HUB_URL, options =>
        {
            options.HttpMessageHandlerFactory = _ => server.CreateHandler();
        }).Build();
    }

    [Fact]
    public async Task HubRouteShouldBeReachable()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("wss://localhost/hubs/GameServiceHub");
        Assert.False(response.StatusCode == HttpStatusCode.NotFound, "Hub route is not registered");
    }


    [Fact]
    public async Task SuccessMessageOnPlayerConnection()
    {

        const string EXPECTED_RESULT = "Success";

        TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();


        var _connection = CreateClient();

        _connection.On("Successfully Joined", () =>
        {
            taskCompletionSource.TrySetResult(EXPECTED_RESULT);
        });

        await _connection.StartAsync();

        string result = await taskCompletionSource.Task;

        Assert.Equal(EXPECTED_RESULT, result);
    }




}