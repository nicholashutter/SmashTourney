namespace Services;

/* GameService implements game operation logic */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;
using Services;
using CustomExceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

enum Votes
{
    ZERO,
    ONE,
    TWO

}
public class GameService : IGameService
{
    //internal logger 
    private readonly ILogger<GameService> _logger;

    //internal reference to dbContextFactory which allows
    //for local access to db inside of singleton class
    private readonly IDbContextFactory<AppDBContext> _dbContextFactory;

    //list that holds all currently played games in memory
    private List<Game> _games;

    //list that represents users authenticated but not in a specific game
    public List<User> _lobby;


    //GameService has a singleton lifetime and is created on application start
    public GameService(ILogger<GameService> logger, IDbContextFactory<AppDBContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _games = new List<Game>();
        _lobby = new List<User>();
    }

    //api route NewGame 
    public Task<Guid> CreateGame()
    {
        Task<Guid> result = Task.Run(() =>
        {
            Game game = new Game();
            _games.Add(game);
            return game.Id;
        });
        return result;
    }

    //api route EndGame
    public Task<bool> EndGameAsync(Guid endGameId)
    {
        Task<bool> result = Task.Run(() =>
        {
            foreach (Game g in _games)
            {
                if (endGameId == g.Id)
                {
                    _games.Remove(g);
                    return true;
                }
            }
            return false;
        });
        return result;
    }

    //api route GetGameById (debug)
    public async Task<Game?> GetGameByIdAsync(Guid Id)
    {
        _logger.LogInformation($"Info: Get Game By Id {Id}");

        await using (var _db = await _dbContextFactory.CreateDbContextAsync())
        {
            var foundGame = new Game();
            try
            {
                foundGame = await _db.Games.FindAsync(Id);

                if (foundGame is null)
                {
                    throw new GameNotFoundException("GetGameByIdAsync");
                }
                else
                {
                    return foundGame;
                }
            }
            catch (GameNotFoundException e)
            {
                _logger.LogInformation($"Error: Game not found or otherwise null\n {e}");
                return null;
            }
        }


    }


    //api route GetAllGames (debug)
    public async Task<IEnumerable<Game>?> GetAllGamesAsync()
    {
        _logger.LogInformation($"Info: Get All Games");

        await using (var _db = await _dbContextFactory.CreateDbContextAsync())
        {
            var Games = new List<Game>();

            try
            {
                Games = await _db.Games.ToListAsync();

                if (Games.Count == 0)
                {
                    throw new EmptyGamesCollectionException("GetAllGamesAsync");
                }
                return Games;
            }
            catch (EmptyGamesCollectionException e)
            {
                _logger.LogWarning($"All Games Returns Zero, Did You Just Reset The DB? \n {e}");
                return null;
            }
        }


    }

    //api route StartGame
    //AddPlayersToGame() should have been called already
    public async Task<bool> StartGameAsync(Guid existingGameId)
    {

        try
        {
            var foundGame = await GetGameByIdAsync(existingGameId);
            if (foundGame is null)
            {
                foundGame = await LoadGameAsync(existingGameId);
                if (foundGame is null)
                {
                    throw new GameNotFoundException("StartGameAsync");
                }
            }
            var success = await GenerateBracketAsync(existingGameId);
            if (!success)
            {
                throw new InvalidFunctionResponseException("StartGameAsync");
            }
            return true;
        }
        catch (InvalidFunctionResponseException e)
        {
            _logger.LogError($"{e}");
            return false;
        }
        catch (GameNotFoundException e)
        {
            _logger.LogError($"{e}");
            return false;
        }
    }

    //api route StartRound
    //list of players should be unpacked by route and returned in HTTP response
    public Task<List<Player>> StartRoundAsync(Guid gameId)
    {
        //load two new players
        //increment currentGame.currentRound
        //return those players to the front end
        throw new NotImplementedException();
    }

    //api route EndRound
    //can also be called by api route EndGame 
    public Task<bool> EndRoundAsync(Guid gameId, Player RoundWinner, Player RoundLoser)
    {
        //call voteHandlerAsync()
        //call UpdatePlayerScoreAsync()
        //call UpdateBracketAsync()
        throw new NotImplementedException();
    }


    //LoadGameAsync will check against the db and attempt to restore
    //game state from games persisted in db
    public async Task<Game?> LoadGameAsync(Guid gameId)
    {
        _logger.LogInformation($"Info: Create New Game Async");

        await using (var _db = await _dbContextFactory.CreateDbContextAsync())
        {
            try
            {
                var foundGame = await _db.Games.FindAsync(gameId);
                if (foundGame is null)
                {
                    throw new GameNotFoundException("LoadGameAsyn");
                }
                return foundGame;
            }
            catch (GameNotFoundException e)
            {
                _logger.LogError($"Error: foundGame is null. Unable to create new game \n {e}");
                return null;
            }
        }
    }

    //route handler will call this method 
    //SaveGame will persist current game state to the database
    public async Task<bool> SaveGameAsync(Guid gameId)
    {
        _logger.LogInformation($"Info: Create New Game Async");

        await using (var _db = await _dbContextFactory.CreateDbContextAsync())
        {
            try
            {
                var foundGame = await _db.Games.FindAsync(gameId);

                if (foundGame is not null)
                {
                    _db.Games.Update(foundGame);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    throw new GameNotFoundException("SaveGameAsync");
                }

                return true;
            }
            catch (GameNotFoundException e)
            {

                //TODO write custom exception classes to specify entityNotFound exception 
                _logger.LogError($"Error: {e}");
                return false;
            }
        }


    }


    //route handler will call this method
    public Task<bool> GenerateBracketAsync(Guid gameId)
    {
        Task<bool> result = Task.Run(() =>
        {

            try
            {
                var foundGame = _games.Find(g => g.Id == gameId);

                if (foundGame is null)
                {
                    throw new GameNotFoundException("GenerateBracketAsync");
                }

                if (foundGame.CurrentPlayers.Count < 2)
                {
                    throw new BracketGenrationException($"Bracket size invalid {foundGame.CurrentPlayers.Count}");
                }
                else if (foundGame.CurrentPlayers.Count < 5)
                {
                    foundGame.byes = Math.Abs(foundGame.CurrentPlayers.Count - 4);
                }
                else if (foundGame.CurrentPlayers.Count < 9)
                {
                    foundGame.byes = Math.Abs(foundGame.CurrentPlayers.Count - 8);
                }
                else if (foundGame.CurrentPlayers.Count < 17)
                {
                    foundGame.byes = Math.Abs(foundGame.CurrentPlayers.Count - 16);
                }
                else if (foundGame.CurrentPlayers.Count < 33)
                {
                    foundGame.byes = Math.Abs(foundGame.CurrentPlayers.Count - 32);
                }
                else if (foundGame.CurrentPlayers.Count < 65)
                {
                    foundGame.byes = Math.Abs(foundGame.CurrentPlayers.Count - 64);
                }
                else if (foundGame.CurrentPlayers.Count < 129)
                {
                    foundGame.byes = Math.Abs(foundGame.CurrentPlayers.Count - 128);
                }
                else if (foundGame.CurrentPlayers.Count < 257)
                {
                    foundGame.byes = Math.Abs(foundGame.CurrentPlayers.Count - 256);
                }
                else
                {
                    throw new BracketGenrationException($"Bracket size invalid {foundGame.CurrentPlayers.Count}");
                }

                for (int i = 0; i < foundGame.byes; i++)
                {

                    //use null object pattern or dummy player 
                    //who always loses and is randomly inserted bye number of times
                    foundGame.CurrentPlayers.Add(new Player());
                }

                Random rnd = new Random();

                int n = foundGame.CurrentPlayers.Count;
                int l = rnd.Next(1, 10);

                //Swap elements in place to randomize bracket order
                //(No seed involved currently)
                //perform randomization calculation 1 to 10 times
                for (int i = 0; i < l; i++)
                {
                    while (n > 1)
                    {
                        n--;
                        int k = rnd.Next(n + 1);
                        (foundGame.CurrentPlayers[k], foundGame.CurrentPlayers[n]) = (foundGame.CurrentPlayers[n], foundGame.CurrentPlayers[k]);
                    }

                }



            }
            catch (GameNotFoundException e)
            {
                _logger.LogError($"{e}");
                return false;
            }
            return true;
        });

        return result;
    }

    //should be called by route handler
    public Task<bool> UpdateBracketAsync(Guid gameId)
    {
        Task<bool> result = Task.Run(() =>
       {
           //this method may or may not be needed
           return true;
       });

        return result;
    }

    //route handler will call this method
    //players list should come from the httprequest from the front end
    //only adds users already in lobby to game if their UserId from the back end and 
    //userId from the player object submitted by the front end agree
    public Task<bool> AddPlayersToGameAsync(List<Player> players, Guid gameId)
    {
        _logger.LogInformation($"Info: Loading Users and Creating Their Corresponding Players");

        Task<bool> result = Task.Run(() =>
        {
            try
            {
                var foundGame = _games.Find(g => g.Id == gameId);

                if (foundGame is null)
                {
                    throw new GameNotFoundException("AddPlayersToGameAsync");
                }

                foreach (User user in _lobby)
                {
                    foreach (Player player in players)
                    {
                        if (user.Id == player.UserId)
                        {
                            foundGame.CurrentPlayers.Add(player);
                        }
                    }
                }
                _logger.LogInformation($"Info: Players Created From Users");
                return true;
            }
            catch (GameNotFoundException e)
            {
                _logger.LogInformation($"Error: Failed to Create Players from Users \n {e}");
                return false;
            }
        });
        return result;
    }

    //called by route handler
    //will only let authenticated users further inside the application if the gameID presented is valid
    public Task<bool> AddUserToLobby(User addUser, Guid gameId)
    {
        _logger.LogInformation($"Info: Adding User {addUser.Username} to game lobby.");

        Task<bool> result = Task.Run(() =>
        {
            try
            {
                if (addUser is null)
                {
                    _logger.LogError($"Unable to add User to game: addUser is null");

                    throw new InvalidArgumentException("AddUserToLobby");
                }
                else
                {
                    if (_games.Count is 0)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        var foundGame = _games.FirstOrDefault(g => g.Id == gameId);

                        if (foundGame is null)
                        {
                            _logger.LogError($"Unable To Locate Game With GameId {gameId}");
                            throw new GameNotFoundException("AddUserToLobby");
                        }
                        else
                        {
                            _lobby.Add(addUser);
                            _logger.LogInformation($"User {addUser.Username} Added To Game {gameId} lobby");
                            return true;
                        }
                    }
                }
            }
            catch (GameNotFoundException e)
            {
                _logger.LogError($"{e}");
                return false;
            }
            catch (InvalidArgumentException e)
            {
                _logger.LogError($"{e}");
                return false;
            }
        });
        return result;

    }

    // called by endRoundAsync 
    //this method may become private
    //updatePlayerScore updates the score each round, in game
    public Task<bool> UpdatePlayerScoreAsync(Guid gameId, Player RoundWinner, Player RoundLoser)
    {
        Task<bool> result = Task.Run(() =>
        {
            //foundGame is _games.FindAsync(gameId)
            //take RoundWinner and RoundLoser and increment their respective wins and losses
            //and other relevant fields
            return true;
        });

        return result;
    }

    //updateUserScore updates the score either when the game ends
    // , or is saved and does persist the changes to the db
    //called by endRoundAsync
    //may become private
    public Task<bool> UpdateUserScore(Guid gameId)
    {
        Task<bool> result = Task.Run(() =>
        {
            //create userService context
            //foundGame is GetGameByIdAsync()
            //if foundGame is not null
            //iterate over players, get their userIds
            //iterate over list<userIds> use current player score values
            //to increment or decrement all time User score values
            //have userService update these values

            return true;
        });

        return result;
    }

    //called by endRoundAsync
    //may become private
    public Task<bool> VoteHandlerAsync(Guid playerID, Player RoundWinner, Player RoundLoser)
    {
        //only once two votes are received should the round move forward
        //only players must be valid and in game for the vote to count
        //both votes must agree on the winner for the vote to count
        throw new NotImplementedException();
    }
}