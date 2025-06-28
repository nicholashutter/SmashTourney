namespace Services;

public abstract class GameServiceComponent
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IServiceScopeFactory _scopeFactory;

    protected readonly IGameService _gameService;
    public GameServiceComponent(IServiceScopeFactory scopeFactory, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _scopeFactory = scopeFactory;
        _gameService = _serviceProvider.GetRequiredService<IGameService>();
    }
}