namespace Services;

using Entities;
using Serilog;
using CustomExceptions;


public class RoundService : GameServiceComponent, IRoundService
{
    
    public RoundService(IServiceScopeFactory scopeFactory, IServiceProvider serviceProvider):
    base(scopeFactory, serviceProvider)
    {
        
    }

    //api route //StartRound
    public Task<List<Player>?> StartRound(Guid gameId)
    {
        using (var _scope = _scopeFactory.CreateAsyncScope())
        {
            var roundService = _scope.ServiceProvider.GetRequiredService<IMatchService>();

            return roundService.StartMatch(gameId); 
        }    
    }

    //api route /EndRound
    public Task<bool> EndRoundAsync(Guid gameId, Player RoundWinner, Player RoundLoser)
    {

        Log.Information($"Info: EndRoundAsync {gameId}");

        Task<bool> result = Task.Run(async() =>
        {
            try
            {

                var _games = await _gameService.GetAllGamesAsync(); 

                var foundGame = _games.Find(g => g.Id == gameId);
                if (foundGame is null)
                {
                    throw new GameNotFoundException("EndRoundAsync");
                }
                //this should be the only method that iterates this property
                //probably should use a lock statement here to enforce that
                foundGame.currentRound++;
                return true;

            }
            catch (GameNotFoundException e)
            {
                Log.Error($"{e}");
                return false;
            }
        });

        return result;
    }

    //called by endRoundAsync
    //may become private
    public Task<bool> VoteHandlerAsync(Guid gameId, Player roundWinner, Player roundLoser)
    {

        Log.Information($"Info: VoteHandlerAsync {gameId}");

        Task<bool> result = Task.Run(async () =>
        {
            try
            {

                var _games = await _gameService.GetAllGamesAsync(); 

                //get game context and current players 
                var foundGame = _games.Find(g => g.Id == gameId);

                if (foundGame is null)
                    {
                        throw new GameNotFoundException("VoteHandlerAsync");
                    }

                roundWinner = foundGame.currentPlayers.Find(p => p.Id == roundWinner.Id);
                if (roundWinner is null)
                {
                    throw new PlayerNotFoundException("VoteHandlerAsync");
                }

                roundLoser = foundGame.currentPlayers.Find(p => p.Id == roundLoser.Id);
                if (roundLoser is null)
                {
                    throw new PlayerNotFoundException("VoteHandlerAsync");
                }

                var currentVotes = foundGame.GetVotes();
                //only once two votes are received should the round move forward
                //use the submitted players Id to increment the game VOTE enum 
                //set the players individual properties as winner and loser 
                foundGame.SetVotes((Votes)((int)currentVotes + 1));

                switch (currentVotes)
                {
                    case Votes.ZERO:
                        break;
                    case Votes.ONE:
                        break;
                    case Votes.TWO:
                        roundWinner.CurrentScore++;
                        break;
                }
            }
            catch (PlayerNotFoundException e)
            {
                Log.Error($"{e}");
                return false;
            }
            return true;
        });

        return result;
    }
    

}
 