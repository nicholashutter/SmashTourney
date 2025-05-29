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
        Task<Guid> result = Task.Run(async () =>
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
    public Task<bool> StartGameAsync(Guid existingGameId)
    {

        try
        {
            var foundGame = GetGameByIdAsync(existingGameId);
            GenerateBracketAsync(existingGameId);
        }
        catch (Exception e)
        {

        }
        //if result is null
        //startRoundAsync
        //else
        //await LoadGameAsync
        //startRound  
        throw new NotImplementedException();
    }

    public Task<List<Player>> StartRoundAsync(Guid gameId)
    {
        //load two new players
        //increment currentGame.currentRound
        //return those players to the front end
        throw new NotImplementedException();
    }


    public Task<bool> EndRoundAsync(Guid gameId, Player RoundWinner, Player RoundLoser)
    {
        //call voteHandlerAsync()
        //call UpdatePlayerScoreAsync()
        //call UpdateBracketAsync()
        throw new NotImplementedException();
    }


    //LoadGameAsync will check against the db and attempt to restore
    //game state from games persisted in db
    public async Task<bool> LoadGameAsync(Guid gameId)
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
                return true;
            }
            catch (GameNotFoundException e)
            {
                _logger.LogError($"Error: foundGame is null. Unable to create new game \n {e}");
                return false;
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
        _logger.LogInformation($"Info: Loading Users and Creating Their Corresponding Players");

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
                    throw new GameNotFoundException("AddPlayersToGameAsync");
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
                _logger.LogInformation($"Info: Players Created From Users");
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                _logger.LogError($"Error: Index Value out of bounds");
                return false;
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