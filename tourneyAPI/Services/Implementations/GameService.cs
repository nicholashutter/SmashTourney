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

public class GameService : IGameService
{
    private readonly ILogger<GameService> _logger;
    private readonly IDbContextFactory<AppDBContext> _dbContextFactory;
    private List<Game> _games;
    public List<User> _lobby;

    public GameService(ILogger<GameService> logger, IDbContextFactory<AppDBContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _games = new List<Game>();
        _lobby = new List<User>();
    }

    public Guid CreateGame()
    {
        Game game = new Game();
        _games.Add(game);
        return game.Id;
    }

    public bool EndGameAsync(Game game)
    {
        foreach (Game g in _games)
        {
            if (game.Id == g.Id)
            {
                _games.Remove(game);
                return true;
            }
        }
        return false;
    }


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

    public Task<bool> StartGameAsync()
    {

        //AddPlayersToGame()
        throw new NotImplementedException();
    }

    public Task<bool> StartRoundAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EndRoundAsync(Game game)
    {
        throw new NotImplementedException();
    }


    public async Task<bool> LoadGameAsync(Game loadGame)
    {
        _logger.LogInformation("Info: Create New Game Async");

        await using (var _db = await _dbContextFactory.CreateDbContextAsync())
        {
            try
            {
                if (loadGame is null)
                {
                    _logger.LogError("Error: loadGame is null. Unable to create new game");
                    throw new NullReferenceException();
                }
                else
                {
                    var foundGame = await _db.Games.FindAsync(loadGame);
                    if (foundGame is null)
                    {
                        _logger.LogError("Error: foundGame is null. Unable to create new game");
                        throw new NullReferenceException();
                    }
                    return true;
                }
            }
            catch (NullReferenceException e)
            {
                _logger.LogError("Error: {e}", e.ToString());
                return false;
            }
        }
    }

    public async Task<bool> SaveGameAsync(Game saveGame)
    {
        _logger.LogInformation("Info: Create New Game Async");

        await using (var _db = await _dbContextFactory.CreateDbContextAsync())
        {
            try
            {
                if (saveGame is null)
                {
                    throw new NullReferenceException();
                }
                else
                {
                    await _db.Games.AddAsync(saveGame);
                    await _db.SaveChangesAsync();
                    _games.Add(saveGame);
                    return true;
                }
            }
            catch (NullReferenceException e)
            {
                _logger.LogError("Error: newGame is null. Unable to create new game\n {e}", e.ToString());
                return false;
            }
        }


    }

    public Task<bool> GenerateBracket(Guid gameId)
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

    //players list should come from the httprequest from the front end
    //only adds users already in lobby to game if their UserId from the back end and 
    //userId from the player object submitted by the front end agree
    public Task<bool> AddPlayersToGame(List<Player> players, Guid gameId)
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

    public Task<bool> UpdatePlayerScore(Guid gameId, Player RoundWinner, Player RoundLoser)
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

    public Task<bool> UpdateUserScore(Guid gameId)
    {
        Task<bool> result = Task.Run(() =>
        {
            //create userService context
            //foundGame is _games.FindAsync(gameId)
            //iterate over players, get their userIds
            //iterate over list<userIds> use current player score values
            //to increment or decrement all time User score values
            //have userService update these values

            return true;
        });

        return result;
    }

    public Task<bool> VoteHandlerAsync(Guid playerID, Guid playerToVoteForID)
    {
        throw new NotImplementedException();
    }
}