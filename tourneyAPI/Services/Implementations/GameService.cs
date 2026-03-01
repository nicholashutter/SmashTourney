namespace Services;

/* GameService implements game operation logic */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities;
using Helpers;
using CustomExceptions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

public class GameService : IGameService
{
    private readonly IServiceProvider _serviceProvider;

    private readonly IServiceScopeFactory _scopeFactory;

    //list that holds all currently played games in memory
    private List<Game> _games;

    //list that represents users authenticated but not in a specific game
    public List<ApplicationUser> _userSessions;


    //these lists are used to allow for game and player round syncronization
    private Stack<Player> _hasNotPlayed = new Stack<Player>();

    //GameService has a singleton lifetime and is created on application start
    public GameService(IServiceScopeFactory scopeFactory, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _scopeFactory = scopeFactory;
        _games = new List<Game>();
        _userSessions = new List<ApplicationUser>();
    }

    //api route /NewGame 
    public async Task<Guid> CreateGame()
    {
        Log.Information("CreateGame");

        var game = new Game { Id = Guid.NewGuid() };
        await InsertGameAsync(game);
        _games.Add(game);

        Log.Information($"New Game with gameId {game.Id} created");
        return game.Id;
    }


    private async Task InsertGameAsync(Game currentGame)
    {
        Log.Information("Insert Game");
        using var scope = _serviceProvider.CreateScope();
        var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _db.Games.Add(currentGame);
        await _db.SaveChangesAsync();
    }

    //api route /SaveGame
    //SaveGame will persist current game state to the database
    public async Task UpdateGameAsync(Guid gameId)
    {
        Log.Information("Update Game Async");
        using var scope = _serviceProvider.CreateScope();
        var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var foundGame = _games.Find(g => g.Id == gameId)
                         ?? throw new GameNotFoundException("UpdateGameAsync");

        var dbGame = await _db.Games.FindAsync(gameId)
                     ?? throw new GameNotFoundException($"Game with ID {gameId} does not exist.");

        _db.Entry(dbGame).CurrentValues.SetValues(foundGame);
        await _db.SaveChangesAsync();
    }

    //api route /ResumeGame
    public async Task<bool> LoadGameAsync(Guid gameId)
    {
        Log.Information($"Load Game {gameId}");
        using var scope = _serviceProvider.CreateScope();
        var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var foundGame = await _db.Games.FindAsync(gameId);
        if (foundGame is null)
        {
            Log.Warning("FoundGame is null. Unable to load game");
            return false;
        }

        _games.Add(foundGame);
        return true;
    }

    //api route /GetAllGames
    public async Task<List<Game>?> GetAllGamesAsync()
    {
        Log.Information("Get All Games");
        using var scope = _serviceProvider.CreateScope();
        var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (_games.Count > 0) return _games;
        var games = await _db.Games.ToListAsync();
        if (games.Count == 0)
        {
            Log.Warning("All Games Returns Zero, Did You Just Reset The DB?");
            return null;
        }
        return games;
    }

    //api route /GetGameById
    public async Task<Game?> GetGameByIdAsync(Guid gameId)
    {
        Log.Information($"Get Game By Id {gameId}");
        var foundGame = _games.FirstOrDefault(g => g.Id == gameId);
        if (foundGame != null) return foundGame;

        using var scope = _serviceProvider.CreateScope();
        var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await _db.Games.FindAsync(gameId);
    }

    public async Task<List<Player>> GetPlayersInGame(Guid gameId) =>
        (await GetGameByIdAsync(gameId))?.currentPlayers ?? new List<Player>();

    //Api route /CreateUserSession
    //will only let authenticated users further inside the application if the gameID presented is valid
    public bool CreateUserSession(ApplicationUser addUser)
    {
        if (addUser == null)
        {
            Log.Error("Unable to add User to game: addUser is null");
            return false;
        }
        _userSessions.Add(addUser);
        Log.Information($"User {addUser.UserName} added to user sessions.");
        return true;
    }

    public bool EndUserSession(ClaimsPrincipal user)
    {
        Log.Information($"Ending User Session for {user.Identity.Name}");
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            Log.Error("Unable to end User Session: userId could not be parsed from ClaimsPrincipal");
            return false;
        }
        if (_userSessions.All(u => u.Id != userId))
        {
            Log.Warning("Unable to end User Session: User not found in user sessions");
            return false;
        }
        return true;
    }

    //api route /AllPlayersIn
    //Player comes from the front end
    public bool AddPlayerToGame(Player player, Guid gameId, string userId)
    {
        var foundGame = _games.FirstOrDefault(g => g.Id == gameId);
        if (foundGame == null)
        {
            Log.Information($"Failed to Create Players from Users game {gameId} not found");
            return false;
        }
        player.UserId = userId;
        player.CurrentGameID = gameId;
        foundGame.currentPlayers.Add(player);
        Log.Information("Players Created From Users");
        return true;
    }


    //api route StartGame
    public async Task<bool> StartGameAsync(Guid existingGameId)
    {
        Log.Information($"End Game {existingGameId}");
        try
        {
            var foundGame = await GetGameByIdAsync(existingGameId)
                            ?? (_games.FirstOrDefault(g => g.Id == existingGameId)
                                ?? throw new GameNotFoundException("StartGameAsync"));

            GenerateBracket(existingGameId);
            return true;
        }
        catch (Exception e) when (e is GameNotFoundException || e is InvalidFunctionResponseException)
        {
            Log.Error(e.ToString());
            return false;
        }
    }

    //called by StartGame
    private void GenerateBracket(Guid gameId)
    {
        Log.Information($"Generate Bracket {gameId}");
        var foundGame = _games.FirstOrDefault(g => g.Id == gameId)
                        ?? throw new GameNotFoundException("GenerateBracketAsync");
        CalculateByes(foundGame);
        InsertByes(foundGame);
        ShuffleBracket(foundGame);
    }

    //calculate how many null objects to insert into foundGame based on currentPlayers
    private void CalculateByes(Game foundGame)
    {
        var count = foundGame.currentPlayers.Count;
        if (count < 2 || count >= 256)
            throw new BracketGenrationException($"Bracket size invalid: {count}");
        foreach (var threshold in new[] { 4, 8, 16, 32, 64, 128, 256 })
        {
            if (count < threshold)
            {
                foundGame.byes = threshold - count;
                break;
            }
        }
    }

    private void InsertByes(Game foundGame)
    {
        for (int i = 0; i < foundGame.byes; i++)
        {

            //use null object pattern or dummy player 
            //who always loses and is randomly inserted bye number of times
            foundGame.currentPlayers.Add(new Player
            {
                Id = Guid.NewGuid(),
                UserId = AppConstants.ByeUserId
            });
        }
    }

    private void ShuffleBracket(Game foundGame)
    {
        Random random = new Random();

        int numberOfPlayers = foundGame.currentPlayers.Count;

        //fisher yates randomization
        for (int i = numberOfPlayers - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);

            (foundGame.currentPlayers[j], foundGame.currentPlayers[i]) = (foundGame.currentPlayers[i], foundGame.currentPlayers[j]);
        }


    }

    //api route /EndGame
    public bool EndGame(Guid endGameId)
    {
        Log.Information($"End Game {endGameId}");
        var game = _games.FirstOrDefault(g => g.Id == endGameId);
        if (game == null) return false;
        _games.Remove(game);
        Log.Information($"Game with gameId {game.Id} removed");
        return true;
    }

    //get the game from gameId
    //foreach put each player in hasNotPlayed
    private void LoadPlayersForRound(Guid gameId)
    {
        var foundGame = _games.FirstOrDefault(g => g.Id == gameId)
                        ?? throw new GameNotFoundException("LoadPlayersForRound");
        _hasNotPlayed = new Stack<Player>(foundGame.currentPlayers);
    }
    //api route //StartRound
    public void StartRound(Guid gameId) => LoadPlayersForRound(gameId);



    //api route /EndRound
    public bool EndRound(Guid gameId)
    {
        Log.Information($"EndRoundAsync {gameId}");
        var foundGame = _games.FirstOrDefault(g => g.Id == gameId);
        if (foundGame == null) return false;
        foreach (Player player in foundGame.currentPlayers)
            player.CurrentRound++;
        foundGame.currentRound++;
        return true;
    }


    private List<Player> PreMatchSetup(Game foundGame)
    {
        if (_hasNotPlayed.Count == 0)
        {
            EndRound(foundGame.Id);
            return new List<Player>();
        }

        var currentPlayerOne = _hasNotPlayed.Pop();
        var currentPlayerTwo = _hasNotPlayed.Pop();

        if (currentPlayerOne.CurrentRound != foundGame.currentRound ||
            currentPlayerTwo.CurrentRound != foundGame.currentRound)
        {
            throw new RoundMismatchException("StartMatch");
        }

        foundGame.currentMatch++;
        return new List<Player> { currentPlayerOne, currentPlayerTwo };
    }
    //api route StartMatch
    //list of players should be unpacked by route and returned in HTTP response
    public List<Player>? StartMatch(Guid gameId)
    {
        Log.Information($"StartMatch {gameId}");
        var foundGame = _games.FirstOrDefault(g => g.Id == gameId);
        if (foundGame == null) return null;
        try
        {
            return PreMatchSetup(foundGame);
        }
        catch (RoundMismatchException e)
        {
            Log.Error(e.ToString());
            return null;
        }
    }

    //api route EndMatch
    public async Task<bool> EndMatchAsync(Guid gameId, Player matchWinner)
    {
        Log.Information($"End Match Async {gameId}");
        var foundGame = _games.FirstOrDefault(g => g.Id == gameId);
        if (foundGame == null) return false;
        if (!VoteHandler(gameId, matchWinner)) return false;
        if (!await PostMatchShutdown(foundGame.Id)) return false;
        return true;
    }

    //Handle votes for match winner
    private bool VoteHandler(Guid gameId, Player matchWinner)
    {
        Log.Information($"VoteHandlerAsync {gameId}");
        try
        {
            var foundGame = _games.FirstOrDefault(g => g.Id == gameId)
                            ?? throw new GameNotFoundException("VoteHandlerAsync");
            var foundPlayer = foundGame.currentPlayers.FirstOrDefault(p => p.Id == matchWinner.Id)
                              ?? throw new PlayerNotFoundException("VoteHandlerAsync");
            CastVote(foundGame, foundPlayer);
            return true;
        }
        catch (Exception e) when (e is GameNotFoundException || e is PlayerNotFoundException)
        {
            Log.Error(e.ToString());
            return false;
        }
    }


    //Individual Vote Casting Logic
    private void CastVote(Game foundGame, Player MatchWinner)
    {
        var currentVotes = foundGame.GetVotes();
        /* only if two votes are received should the MatchWinner's score increment */
        foundGame.SetVotes((Votes)((int)currentVotes + 1));

        switch (currentVotes)
        {
            case Votes.ZERO:
                break;
            case Votes.ONE:
                break;
            case Votes.TWO:
                MatchWinner.CurrentScore++;
                break;
        }
    }

    

    //called by SaveGameAsync or EndGameAsync
    private async Task<bool> PostMatchShutdown(Guid gameId)
    {
        Log.Information($"PostMatchShutdown {gameId}");

        //create userService context
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserManager>();
            try
            {
                var foundGame = _games.Find(g => g.Id == gameId);

                if (foundGame is null)
                {
                    throw new GameNotFoundException("PostMatchShutdown");
                }

                

                foreach (Player player in foundGame.currentPlayers)
                {
                    var currentUser = await userRepository.GetUserByIdAsync(player.UserId);

                    switch (IsRealUser(currentUser, player.UserId))
                    {
                        case UserType.RealUser:
                            if (foundGame.currentRound == player.CurrentRound)
                            {
                                UpdateUser(currentUser, player);
                                await userRepository.UpdateUserAsync(currentUser);
                            }
                            else
                            {
                                Log.Warning("Warning: UpdateUserScore called, but some users aren't on the same round as their game");
                            }
                            break;
                        case UserType.ByeUser:
                            Log.Information("Bye User processed. No score update needed.");
                            break;
                        case UserType.NullUser:
                            throw new UserNotFoundException("PostMatchShutdown");
                        default:
                            throw new InvalidObjectStateException("PostMatchShutdown");
                    }


                }
            }
            catch (UserNotFoundException e)
            {
                Log.Error($"{e}");
            }
            catch (GameNotFoundException e)
            {
                Log.Error($"{e}");
            }
            catch (InvalidObjectStateException e)
            {
                Log.Error($"{e}");
            }
        }
        return true;
    }

    //validate if user is null object pattern (bye user)
    //or "real" ApplicationUser

    enum UserType
    {
        RealUser,
        ByeUser,

        NullUser
    }
private UserType IsRealUser(ApplicationUser currentUser, string userId) =>
        userId == AppConstants.ByeUserId ? UserType.ByeUser :
        currentUser != null ? UserType.RealUser :
        UserType.NullUser;

    //Update User Values Post Match 
    private ApplicationUser UpdateUser(ApplicationUser currentUser, Player player)
    {
        switch (IsRealUser(currentUser, currentUser.Id))
        {
            case UserType.RealUser:
                currentUser.AllTimeMatches += player.CurrentRound;
                var currentWins = Math.Abs(player.CurrentScore - player.CurrentRound);
                currentUser.AllTimeWins += currentWins;
                break;
            case UserType.ByeUser:
                Log.Information("Bye User processed. No score update needed.");
                break;
            case UserType.NullUser:
                throw new UserNotFoundException("UpdateUser");
        }
        return currentUser;
    }

}