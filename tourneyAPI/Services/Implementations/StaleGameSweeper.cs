namespace Services;

using Microsoft.Extensions.Hosting;
using Serilog;

// Deletes finished and abandoned tournaments on a timer.
//
// The sweep used to run inside the games-listing route, which made a GET delete
// rows — the request that draws a menu was also the request that destroyed data,
// and whether cleanup happened at all depended on somebody opening the browser.
// A timer is what that code actually wanted; the listing route only ever hosted
// it because there was nothing else to hang it off.
public sealed class StaleGameSweeper : BackgroundService
{
    // Long enough that it never competes with real traffic, and far longer than
    // any test run, so a suite is never racing a background delete.
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(15);

    private readonly IGameService _gameService;

    public StaleGameSweeper(IGameService gameService)
    {
        _gameService = gameService;
    }

    // Sweeps on each tick until the application shuts down.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // PeriodicTimer's first tick lands one interval in, which is what we
        // want: startup has enough to do, and nothing is urgent here.
        using var sweepTimer = new PeriodicTimer(SweepInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await sweepTimer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                var prunedCount = await _gameService.PruneStaleGamesAsync();

                if (prunedCount > 0)
                {
                    Log.Information("Stale game sweep removed {GameCount} game(s)", prunedCount);
                }
            }
            catch (Exception exception)
            {
                // A failed sweep must never take the host down with it. Games
                // outliving their welcome is untidy; an API that stops serving
                // a party mid-tournament because a delete failed is not.
                Log.Error(exception, "Stale game sweep failed. Retrying on the next tick.");
            }
        }
    }
}
