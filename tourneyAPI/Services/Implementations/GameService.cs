namespace Services;

/* GameService implements game operation logic */
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Entities;
using Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Internal;

enum Votes
{
    ZERO,
    ONE,
    TWO

}

//TODO change all passing around game object references to 
//grabbing from the _games list to make sure _games is single
//source of truth
//TODO rebuild db migration to account for addition of vote property
//in game entity

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


    //this class has a singleton lifetime and is created on application start
    public GameService(ILogger<GameService> logger, IDbContextFactory<AppDBContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _games = new List<Game>();
        _lobby = new List<User>();
    }

    //this method will be called by the route handler
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

    //this method will be called by the route handler
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


    //this method should be called internally and 
    //may get changed to private
    public async Task<Game?> GetGameByIdAsync(Guid Id)
    {
        _logger.LogInformation("Info: Get Game By Id {Id}", Id);

        await using (var _db = await _dbContextFactory.CreateDbContextAsync())
        {
            var foundGame = new Game();
            try
            {
                foundGame = await _db.Games.FindAsync(Id);

                if (foundGame is null)
                {
                    throw new NullReferenceException();
                }
                else
                {
                    return foundGame;
                }
            }
            catch (NullReferenceException e)
            {
                _logger.LogInformation("Error: Game not found or otherwise null\n {e}", e.ToString());
                return null;
            }
        }


    }

    //this is a debugging method that can be called by route handler
    public async Task<IEnumerable<Game>?> GetAllGamesAsync()
    {
        _logger.LogInformation("Info: Get All Games");

        await using (var _db = await _dbContextFactory.CreateDbContextAsync())
        {
            var Games = new List<Game>();

            try
            {
                Games = await _db.Games.ToListAsync();

                if (Games.Count == 0)
                {
                    throw new NullReferenceException();
                }
                return Games;
            }
            catch (NullReferenceException e)
            {
                _logger.LogWarning("All Games Returns Zero, Did You Just Reset The DB?");
                return null;
            }
        }


    }

    //this route should be called by the route handler
    //AddPlayersToGame() should have been called already
    public Task<bool> StartGameAsync(Guid existingGameId)
    {
        //GenerateBracket()
        //result = GetGameByIdAsync(existingGameId)
        //if result is null
        //startRoundAsync
        //else
        //await LoadGameAsync
        //startRound  
        throw new NotImplementedException();
    }

    public Task<bool> StartRoundAsync(Game gameId)
    {
        //load two new players
        //increment currentGame.currentRound
        throw new NotImplementedException();
    }

    /* this might be redundant method
        public Task<bool> EndRoundAsync(Game game)
        {
            throw new NotImplementedException();
        } */


    //LoadGameAsync will check against the db and attempt to restore
    //game state from games persisted in db
    public async Task<bool> LoadGameAsync(Guid gameId)
    {
        _logger.LogInformation("Info: Create New Game Async");

        await using (var _db = await _dbContextFactory.CreateDbContextAsync())
        {
            try
            {
                var foundGame = await _db.Games.FindAsync(gameId);
                if (foundGame is null)
                {
                    _logger.LogError("Error: foundGame is null. Unable to create new game");
                    throw new NullReferenceException();
                }
                return true;
            }
            catch (NullReferenceException e)
            {
                _logger.LogError("Error: {e}", e.ToString());
                return false;
            }
        }
    }

    //route handler will call this method 
    //SaveGame will persist current game state to the database
    public async Task<bool> SaveGameAsync(Game saveGame)
    {
        _logger.LogInformation("Info: Create New Game Async");

        await using (var _db = await _dbContextFactory.CreateDbContextAsync())
        {
            try
            {
                bool flag = await _db.Games.ContainsAsync(saveGame);

                if (flag is false)
                {
                    await _db.Games.AddAsync(saveGame);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    _db.Games.Update(saveGame);
                }

                return true;
            }
            catch (NullReferenceException e)
            {
                _logger.LogError("Error: {e}", e.ToString());
                return false;
            }
        }


    }


    //route handler will call this method
    public Task<bool> GenerateBracketAsync(Guid gameId)
    {
        Task<bool> result = Task.Run(() =>
        {

            //get game from _games by guid
            //operate on currentGame.Players()
            //currentGame.players.Count to determine bracket size
            //always round up to higher power of 2
            //for every time we round up, we add one "bye"
            //use null object pattern or dummy player "loserPlayer"
            //who always loses and is randomly inserted bye number of times
            //iterate through players by Id and swap indexs in players in place
            //using Random() perform the processes a Random() number of times
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
        _logger.LogInformation("Info: Loading Users and Creating Their Corresponding Players");

        Task<bool> result = Task.Run(() =>
        {
            try
            {
                double index = -1;

                for (int i = 0; i < _games.Count; i++)
                {
                    if (_games[i].Id == gameId)
                    {
                        index = i;
                    }
                }

                if (index == -1)
                {
                    _logger.LogWarning($"Warning: Unable To Find Game With gameId {gameId}");
                    throw new Exception();
                }

                foreach (User user in _lobby)
                {
                    foreach (Player player in players)
                    {
                        if (user.Id == player.UserId)
                        {
                            if (index > 0)
                            {
                                _games[(int)index].AddPlayer(player);
                            }
                            else
                            {
                                throw new IndexOutOfRangeException();
                            }

                        }
                    }
                }
                _logger.LogInformation("Info: Players Created From Users");
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                _logger.LogError($"Error: Index Value out of bounds");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Error: Failed to Create Players from Users{e.ToString}");
                return false;
            }
        });
        return result;
    }

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

                    throw new NullReferenceException();
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
                            throw new NullReferenceException();
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
            catch (NullReferenceException)
            {
                return false;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Unable to add User to game: _games.count is {_games.Count}: {e.ToString()}");
                return false;
            }
        });
        return result;

    }

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
    //Should be called by route handler
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

    //should be called by route handler
    public Task<bool> VoteHandlerAsync(Guid playerID, Player RoundWinner, Player RoundLoser)
    {
        //player to vote for should come from front end
        //only once two votes are received should the round move forward
        //only players must be valid and in game for the vote to count
        //both votes must agree on the winner for the vote to count
        throw new NotImplementedException();
    }
}