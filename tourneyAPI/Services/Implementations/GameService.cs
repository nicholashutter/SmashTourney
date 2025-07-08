namespace Services;

/* GameService implements game operation logic */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;
using CustomExceptions;
using Microsoft.EntityFrameworkCore;
using Serilog;

public class GameService : IGameService
{
    private readonly IServiceProvider _serviceProvider;

    private readonly IServiceScopeFactory _scopeFactory;

    //list that holds all currently played games in memory
    private List<Game> _games;

    //list that represents users authenticated but not in a specific game
    public List<ApplicationUser> _lobby;

    //GameService has a singleton lifetime and is created on application start
    public GameService(IServiceScopeFactory scopeFactory, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _scopeFactory = scopeFactory;
        _games = new List<Game>();
        _lobby = new List<ApplicationUser>();
    }

    //api route /NewGame 
    public Task<Guid> CreateGame()
    {

        Log.Information($"Info: CreateGame");

        Task<Guid> result = Task.Run(() =>
        {
            Game game = new Game
            {
                Id = Guid.NewGuid()
            };
            _games.Add(game);
            Log.Information($"Info: New Game with gameId {game.Id}");
            return game.Id;
        });
        return result;
    }

    //api route /EndGame
    public Task<bool> EndGameAsync(Guid endGameId)
    {
        Log.Information($"Info: End Game {endGameId}");

        Task<bool> result = Task.Run(() =>
        {
            foreach (Game g in _games)
            {
                if (endGameId == g.Id)
                {
                    _games.Remove(g);
                    Log.Information($"Info: Game with gameId {g.Id} removed");
                    return true;
                }
            }
            return false;
        });
        return result;
    }

    //api route /GetGameById (debug)
    public async Task<Game?> GetGameByIdAsync(Guid gameId)
    {
        Log.Information($"Info: Get Game By Id {gameId}");

        using (var scope = _serviceProvider.CreateScope())
        {
            var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var foundGame = new Game();
            try
            {
                //check memory first
                foundGame = _games.Find(g => gameId == g.Id);
                //check db if not found in memory
                if (foundGame is null)
                {
                    foundGame = await _db.Games.FindAsync(gameId);
                }
                //if not found in memory or db throw exception
                if (foundGame is null)
                {
                    throw new GameNotFoundException("GetGameByIdAsync");
                }
                return foundGame;
            }
            catch (GameNotFoundException e)
            {
                Log.Information($"Error: Game not found or otherwise null\n {e}");
                return null;
            }
        }


    }

    //api route /GetAllGames (debug)
    public async Task<List<Game>?> GetAllGamesAsync()
    {
        Log.Information($"Info: Get All Games");

        using (var scope = _serviceProvider.CreateScope())
        {
            var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var Games = new List<Game>();

            try
            {

                if (_games.Count > 0)
                {
                    return _games;
                }
                else
                {
                    Games = await _db.Games.ToListAsync();
                    if (Games.Count == 0)
                    {
                        throw new EmptyGamesCollectionException("GetAllGamesAsync");
                    }
                }

                return Games;
            }
            catch (EmptyGamesCollectionException e)
            {
                Log.Warning($"All Games Returns Zero, Did You Just Reset The DB? \n {e}");
                return null;
            }
        }
    }

    //Api route /AddUserToLobby
    //will only let authenticated users further inside the application if the gameID presented is valid
    public Task<bool> AddUserToLobby(ApplicationUser addUser, Guid gameId)
    {
        Log.Information($"Info: Adding User {addUser.UserName} to game lobby.");

        Task<bool> result = Task.Run(() =>
        {
            try
            {
                if (addUser is null)
                {
                    Log.Error($"Unable to add User to game: addUser is null");

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
                            Log.Error($"Unable To Locate Game With GameId {gameId}");
                            throw new GameNotFoundException("AddUserToLobby");
                        }
                        else
                        {
                            _lobby.Add(addUser);
                            Log.Information($"User {addUser.UserName} Added To Game {gameId} lobby");
                            return true;
                        }
                    }
                }
            }
            catch (GameNotFoundException e)
            {
                Log.Error($"{e}");
                return false;
            }
            catch (InvalidArgumentException e)
            {
                Log.Error($"{e}");
                return false;
            }
        });
        return result;

    }

    //api route /AllPlayersIn
    //players list should come from the httprequest from the front end
    public Task<bool> AddPlayersToGameAsync(List<Player> players, Guid gameId)
    {
        Log.Information($"Info: Loading Users and Creating Their Corresponding Players");

        Task<bool> result = Task.Run(() =>
        {
            try
            {
                //only adds users already in lobby to game if their UserId from the back end and 
                //userId from the player object submitted by the front end agree
                var foundGame = _games.Find(g => g.Id == gameId);

                if (foundGame is null)
                {
                    throw new GameNotFoundException("AddPlayersToGameAsync");
                }

                foreach (ApplicationUser user in _lobby)
                {
                    foreach (Player player in players)
                    {
                        if (user.Id.Equals(player.UserId))
                        {
                            foundGame.currentPlayers.Add(player);
                        }
                    }
                }
                Log.Information($"Info: Players Created From Users");
                return true;
            }
            catch (GameNotFoundException e)
            {
                Log.Information($"Error: Failed to Create Players from Users \n {e}");
                return false;
            }
        });
        return result;
    }
    //called by StartGame
    public Task<bool> GenerateBracketAsync(Guid gameId)
    {
        Log.Information($"Info: Generate Bracket {gameId}");

        Task<bool> result = Task.Run(() =>
        {
            try
            {
                var foundGame = _games.Find(g => g.Id == gameId);

                if (foundGame is null)
                {
                    throw new GameNotFoundException("GenerateBracketAsync");
                }

                if (foundGame.currentPlayers.Count < 2)
                {
                    throw new BracketGenrationException($"Bracket size invalid {foundGame.currentPlayers.Count}");
                }
                else if (foundGame.currentPlayers.Count < 4)
                {
                    foundGame.byes = Math.Abs(foundGame.currentPlayers.Count - 4);
                }
                else if (foundGame.currentPlayers.Count < 8)
                {
                    foundGame.byes = Math.Abs(foundGame.currentPlayers.Count - 8);
                }
                else if (foundGame.currentPlayers.Count < 16)
                {
                    foundGame.byes = Math.Abs(foundGame.currentPlayers.Count - 16);
                }
                else if (foundGame.currentPlayers.Count < 32)
                {
                    foundGame.byes = Math.Abs(foundGame.currentPlayers.Count - 32);
                }
                else if (foundGame.currentPlayers.Count < 64)
                {
                    foundGame.byes = Math.Abs(foundGame.currentPlayers.Count - 64);
                }
                else if (foundGame.currentPlayers.Count < 128)
                {
                    foundGame.byes = Math.Abs(foundGame.currentPlayers.Count - 128);
                }
                else if (foundGame.currentPlayers.Count < 256)
                {
                    foundGame.byes = Math.Abs(foundGame.currentPlayers.Count - 256);
                }
                else
                {
                    throw new BracketGenrationException($"Bracket size invalid {foundGame.currentPlayers.Count}");
                }

                for (int i = 0; i < foundGame.byes; i++)
                {

                    //use null object pattern or dummy player 
                    //who always loses and is randomly inserted bye number of times
                    foundGame.currentPlayers.Add(new Player());
                }

                Random rnd = new Random();

                int n = foundGame.currentPlayers.Count;
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
                        (foundGame.currentPlayers[k], foundGame.currentPlayers[n]) = (foundGame.currentPlayers[n], foundGame.currentPlayers[k]);
                    }

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

    //api route StartGame
    public Task<bool> StartGameAsync(Guid existingGameId)
    {

        Log.Information($"Info: End Game {existingGameId}");

        Task<bool> result = Task.Run(async () =>
        {
            try
            {
                var foundGame = await GetGameByIdAsync(existingGameId);
                var success = false;
                if (foundGame is null)
                {
                    success = await LoadGameAsync(existingGameId);
                    if (!success)
                    {
                        foundGame = _games.Find(g => g.Id == existingGameId);
                        if (foundGame is null)
                        {
                            throw new GameNotFoundException("StartGameAsync");
                        }
                    }
                }
                success = await GenerateBracketAsync(existingGameId);
                if (!success)
                {
                    throw new InvalidFunctionResponseException("StartGameAsync");
                }
                return true;
            }
            catch (InvalidFunctionResponseException e)
            {
                Log.Error($"{e}");
                return false;
            }
            catch (GameNotFoundException e)
            {
                Log.Error($"{e}");
                return false;
            }

        });
        return result;
    }
    //called by SaveGameAsync or EndGameAsync
    public Task<bool> UpdateUserScore(Guid gameId)
    {
        Log.Information($"Info: UpdateUserScore {gameId}");

        Task<bool> result = Task.Run(async () =>
        {
            //create userService context
            await using (var scope = _scopeFactory.CreateAsyncScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                try
                {
                    var foundGame = _games.Find(g => g.Id == gameId);

                    if (foundGame is null)
                    {
                        throw new GameNotFoundException("UpdateUserScore");
                    }

                    //foundGame is GetGameByIdAsync()
                    //if foundGame is not null

                    Player highestScore = new Player();

                    foreach (Player player in foundGame.currentPlayers)
                    {
                        //iterate over players, get their userIds
                        //iterate over list<userIds> use current player score values
                        var currentUser = await userRepository.GetUserByIdAsync(player.UserId);
                        if (currentUser is null)
                        {
                            throw new UserNotFoundException("UpdateUserScore");
                        }
                        //bubble sort to determine who has highest score
                        if (player.CurrentScore > highestScore.CurrentScore)
                        {
                            highestScore = player;
                        }

                        //have userService update these values
                        //set new timestamps for relevant timestamp fields
                        //increment and decrement totals wins / losses / games played
                        if (foundGame.currentRound == player.CurrentRound)
                        {
                            currentUser.AllTimeMatches = player.CurrentRound + currentUser.AllTimeMatches;
                            var currentWins = Math.Abs(player.CurrentScore - player.CurrentRound);
                            currentUser.AllTimeWins = currentWins + currentUser.AllTimeWins;
                            var currentLosses = Math.Abs(player.CurrentRound - currentWins);

                            if (currentLosses + currentWins != player.CurrentRound)
                            {
                                throw new InvalidObjectStateException("UpdateUserScore");
                            }

                        }
                        else
                        {
                            Log.Warning("Warning: UpdateUserScore called, but some users aren't on the same round as their game");
                        }
                        await userRepository.UpdateUserAsync(currentUser);
                    }
                }
                catch (GameNotFoundException e)
                {
                    Log.Error($"{e}");
                }
            }
            return true;
        });

        return result;
    }

    //LoadGameAsync will check against the db and attempt to restore
    //game state from games persisted in db
    public Task<bool> LoadGameAsync(Guid gameId)
    {
        Log.Information($"Info: Create New Game Async");

        Task<bool> result = Task.Run(async () =>
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    var foundGame = await _db.Games.FindAsync(gameId);

                    if (foundGame is null)
                    {
                        throw new GameNotFoundException("LoadGameAsync");
                    }
                    else
                    {
                        _games.Add(foundGame);
                    }
                    return true;
                }
                catch (GameNotFoundException e)
                {
                    Log.Warning($"Error: foundGame is null. Unable to load game not found \n {e}");
                    return false;
                }
            }
        });

        return result;
    }

    //api route /SaveGame
    //SaveGame will persist current game state to the database
    public Task<bool> SaveGameAsync(Guid gameId)
    {
        Log.Information($"Info: Create New Game Async");

        Task<bool> result = Task.Run(async () =>
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var foundGame = _games.Find(g => g.Id == gameId);
                if (foundGame is null)
                {
                    foundGame = await _db.Games.FindAsync(gameId);
                    if (foundGame is null)
                    {
                        throw new GameNotFoundException("SaveGameAsync");
                    }
                    await _db.Games.AddAsync(foundGame);
                }
                else if (foundGame is not null)
                {
                    _db.Games.Update(foundGame);
                    await _db.SaveChangesAsync();
                }
                return true;
            }
        });

        return result;
    }




}