namespace Services;

using System;
using System.Collections.Generic;
using System.Data.Common; 
using System.Threading.Tasks;
using Entities;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.Logging; 
public class GameService : IGameService
{
    private readonly ILogger<GameService> _logger;
    private readonly AppDBContext _db;

    private PlayerService _playerService;

    private UserService _userService;

    private List<Game> _games;
    
    public List<User> _users;
    public List<Player> _players;

    public GameService(ILogger<GameService> logger, AppDBContext db, PlayerService playerService, UserService userService)
    {
        _logger = logger;
        _db = db;
        _playerService = playerService;
        _userService = userService;
        _games = new List<Game>();
        _users = new List<User>();
        _players = new List<Player>(); 
    }

    public async Task<bool> CreateNewGameAsync(Game newGame)
    {
        _logger.LogInformation("Info: Create New Game Async");

        try
        {
            if (newGame is null)
            {
                throw new NullReferenceException();
            }
            else
            {
                await _db.Games.AddAsync(newGame);
                await _db.SaveChangesAsync();
                return true;
            }
        }
        catch (NullReferenceException e)
        {
            _logger.LogError("Error: newGame is null. Unable to create new game\n {e}", e.ToString());
            return false;
        }
    }
    
    //this input players list should come from the httprequest from the front end
    public bool UsersToPlayersAsync(List<Player> players)
    {
        _logger.LogInformation("Info: Loading Users and Creating Their Corresponding Players");
        try
        {

            foreach (User user in _users)
            {
                foreach (Player player in players)
                {
                    if (user.Id == player.UserId)
                    {
                        _players.Add(player);
                    }
                }
            }
            _logger.LogInformation("Info: Players Created From Users");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogInformation($"Error: Failed to Create Players from Users{e.ToString}");
            return false; 
        }
    }

    public bool AddUserToGame(User addUser, Guid gameId)
    {
        _logger.LogInformation($"Info: Adding User {addUser.Username} to game.");
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
                        _users.Add(addUser);
                        _logger.LogInformation($"User {addUser.Username} Added To Game {gameId}");
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
    }

    public async Task<Game?> GetGameByIdAsync(Guid Id)
    {
        _logger.LogInformation("Info: Get Game By Id {Id}", Id);

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

    public async Task<IEnumerable<Game>?> GetAllGamesAsync()
    {
        _logger.LogInformation("Info: Get All Games");

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

    public Task<bool> StartGameAsync(Game game)
    {
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

    public Task<bool> EndGameAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddPlayerToGameAsync(Game game, Player player)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemovePlayerFromGameAsync(Game game, Guid playerId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SaveGameAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task<bool> LoadGameAsync(Game game)
    {
        throw new NotImplementedException();
    }

    public Task<bool> VoteHandlerAsync(Guid playerID, Guid playerToVoteForID)
    {
        throw new NotImplementedException();
    }
}