namespace Services;

using Entities;
using Serilog;
using CustomExceptions;

public class MatchService : GameServiceComponent, IMatchService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IGameService _gameService;
    public MatchService(IServiceScopeFactory scopeFactory, IServiceProvider serviceProvider) :
    base(scopeFactory, serviceProvider)
    { }

    //api route StartMatch
    //list of players should be unpacked by route and returned in HTTP response
    public Task<List<Player>?> StartMatch(Guid gameId)
    {
        Log.Information($"Info: StartMatch {gameId}");

        Task<List<Player>?> result = Task.Run(async () =>
        {
            try
            {
                var _games = await _gameService.GetAllGamesAsync();

                var foundGame = _games.Find(g => g.Id == gameId);

                if (foundGame is null)
                {
                    throw new GameNotFoundException("StartMatch");
                }

                //load two new players
                List<Player> currentPlayers = new List<Player>();

                //two players loaded into return array based on round counter
                //so that two players are loaded every round and should stay in sync with bracket
                var currentPlayerOne = foundGame.currentPlayers[foundGame.currentMatch];
                var currentPlayerTwo = foundGame.currentPlayers[foundGame.currentMatch + 1];

                if (currentPlayerOne.CurrentRound != foundGame.currentRound)
                {
                    throw new RoundMismatchException("StartMatch");
                }
                else if (currentPlayerTwo.CurrentRound != foundGame.currentRound + 1)
                {
                    throw new RoundMismatchException("StartMatch");
                }

                currentPlayers.Add(currentPlayerOne);
                currentPlayers.Add(currentPlayerTwo);

                //players themselves also track their round so that round stays syncronized
                //all players who have played therefor have a higher round than currentRound
                currentPlayerOne.CurrentRound = currentPlayerOne.CurrentRound++;
                currentPlayerTwo.CurrentRound = currentPlayerTwo.CurrentRound++;


                //return those players to the front end
                foundGame.currentMatch++;
                return currentPlayers;

            }
            catch (GameNotFoundException e)
            {
                Log.Error($"{e}");
                return null;
            }
            catch (RoundMismatchException e)
            {
                Log.Error($"{e}");
                return null;
            }

        });
        return result;
    }
    
        //api route EndMatch
    public Task<bool> EndMatchAsync(Guid gameId, Player matchWinner, Player matchLoser)
    {

        Log.Information($"Info: End Match Async {gameId}");

        Task<bool> result = Task.Run(async () =>
        {
            try
            {
                var _games = await _gameService.GetAllGamesAsync(); 

                var foundGame = _games.Find(g => g.Id == gameId);
                if (foundGame is null)
                {
                    throw new GameNotFoundException("EndRoundAsync");
                }


                using (var _scope = _scopeFactory.CreateAsyncScope())
                {
                    var roundService = _scope.ServiceProvider.GetRequiredService<IRoundService>();
                    
                    var finalVoteSuccess = await roundService.VoteHandlerAsync(gameId, matchWinner, matchLoser);

                    if (!finalVoteSuccess)
                    {
                        throw new InvalidFunctionResponseException("EndMatchAsync");
                    }
                }
                

                var success = await _gameService.UpdateUserScore(foundGame.Id);

                if (!success)
                {
                    throw new InvalidFunctionResponseException("EndMatchAsync");
                }

                success = await _gameService.EndGameAsync(foundGame.Id);

                if (!success)
                {
                    throw new InvalidFunctionResponseException("EndMatchAsync");
                }

            }
            catch (GameNotFoundException e)
            {
                Log.Error($"{e}");
                return false;
            }
            return true;
        });



        return result;
    }
}


 
