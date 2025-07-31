namespace Services;

/* GameService implements game operation logic */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;
using Helpers;
using CustomExceptions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.Extensions.Configuration.UserSecrets;

public class GameService : IGameService
{
    private readonly IServiceProvider _serviceProvider;

    private readonly IServiceScopeFactory _scopeFactory;

    //list that holds all currently played games in memory
    private List<Game> _games;

    //list that represents users authenticated but not in a specific game
    public List<ApplicationUser> _userSessions;

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

        Log.Information($"Info: CreateGame");

        Game game = new Game
        {
            Id = Guid.NewGuid()
        };
        await InsertGameAsync(game);
        _games.Add(game);
        Log.Information($"Info: New Game with gameId {game.Id}");
        return game.Id;
    }


    private async Task InsertGameAsync(Game currentGame)
    {
        Log.Information("Info: Insert Game Async");

        using var scope = _serviceProvider.CreateScope();
        var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        _db.Games.Add(currentGame);
        await _db.SaveChangesAsync();
    }

    //api route /SaveGame
    //SaveGame will persist current game state to the database
    public async Task UpdateGameAsync(Guid gameId)
    {
        Log.Information("Info: Update Game Async");

        using var scope = _serviceProvider.CreateScope();
        var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var foundGame = _games.Find(g => g.Id == gameId);
        if (foundGame is null)
        {
            throw new GameNotFoundException("UpdateGameAsync");
        }

        var dbGame = await _db.Games.FindAsync(gameId);
        if (dbGame is null)
        {
            throw new GameNotFoundException($"Game with ID {gameId} does not exist.");
        }


        _db.Entry(dbGame).CurrentValues.SetValues(foundGame);
        await _db.SaveChangesAsync();
    }

    //api route /ResumeGame
    public async Task<bool> LoadGameAsync(Guid gameId)
    {
        Log.Information($"Info: Create New Game Async");

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
    }

    //api route /GetAllGames
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

    //api route /GetGameById
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

    //Api route /AddUserToLobby
    //will only let authenticated users further inside the application if the gameID presented is valid
    public bool CreateUserSession(ApplicationUser addUser)
    {
        Log.Information($"Info: Adding User {addUser.UserName} to _userSessions.");

        try
        {
            if (addUser is null)
            {
                Log.Error($"Unable to add User to game: addUser is null");

                throw new InvalidArgumentException("CreateUserSession ");
            }
            else
            {
                if (_games.Count is 0)
                {
                    throw new Exception();
                }
                else
                {
                    _userSessions.Add(addUser);
                    Log.Information($"User {addUser.UserName} Added To _userSessions.");
                    return true;
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

    }

    //api route /AllPlayersIn
    //List<Players> comes from the front end
    public bool AddPlayersToGame(List<Player> players, Guid gameId)
    {
        Log.Information($"Info: Loading Users and Creating Their Corresponding Players");

        try
        {
            var foundGame = _games.Find(g => g.Id == gameId);

            if (foundGame is null)
            {
                throw new GameNotFoundException("AddPlayersToGameAsync");
            }

            /* 
            Each ApplicationUser with a session where Player.UserId 
            equals User.UserId add player to game using gameId 
            provided by front end
            */

            foreach (ApplicationUser user in _userSessions)
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
    }


    //api route StartGame
    public async Task<bool> StartGameAsync(Guid existingGameId)

    {

        Log.Information($"Info: End Game {existingGameId}");

        try
        {
            var foundGame = await GetGameByIdAsync(existingGameId);

            bool success = false;
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
            success = GenerateBracket(existingGameId);
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
    }

    //called by StartGame
    private bool GenerateBracket(Guid gameId)
    {
        Log.Information($"Info: Generate Bracket {gameId}");

        try
        {
            var foundGame = _games.Find(g => g.Id == gameId);

            if (foundGame is null)
            {
                throw new GameNotFoundException("GenerateBracketAsync");
            }

            CalculateByes(foundGame);

            InsertByes(foundGame);

            ShuffleBracket(foundGame);
        }
        catch (GameNotFoundException e)
        {
            Log.Error($"{e}");
            return false;
        }
        return true;
    }

    //calculate how many null objects to insert into foundGame based on currentPlayers
    private void CalculateByes(Game foundGame)
    {
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
        Log.Information($"Info: End Game {endGameId}");

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
    }
    //api route //StartRound
    public List<Player>? StartRound(Guid gameId)
    {
        return StartMatch(gameId);
    }

    //api route /EndRound
    public bool EndRound(Guid gameId)
    {

        Log.Information($"Info: EndRoundAsync {gameId}");

        try
        {
            var foundGame = _games.Find(g => g.Id == gameId);
            if (foundGame is null)
            {
                throw new GameNotFoundException("EndRoundAsync");
            }
            foundGame.currentRound++;
            return true;

        }
        catch (GameNotFoundException e)
        {
            Log.Error($"{e}");
            return false;
        }
    }
    //api route StartMatch
    //list of players should be unpacked by route and returned in HTTP response
    public List<Player>? StartMatch(Guid gameId)
    {
        Log.Information($"Info: StartMatch {gameId}");

        try
        {

            var foundGame = _games.Find(g => g.Id == gameId);

            if (foundGame is null)
            {
                throw new GameNotFoundException("StartMatch");
            }

            var currentPlayers = PreMatchSetup(foundGame);

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
    }

    private List<Player> PreMatchSetup(Game foundGame)
    {
        //load two new players
        List<Player> currentPlayers = new List<Player>();

        /* 
            two players loaded into return array based on round counter
            so that two players are loaded every match and should stay in sync with bracket
        */
        var currentPlayerOne = foundGame.currentPlayers[foundGame.currentMatch];
        var currentPlayerTwo = foundGame.currentPlayers[foundGame.currentMatch++];

        if (currentPlayerOne.CurrentRound != foundGame.currentRound)
        {
            throw new RoundMismatchException("StartMatch");
        }
        else if (currentPlayerTwo.CurrentRound != foundGame.currentRound)
        {
            throw new RoundMismatchException("StartMatch");
        }

        currentPlayers.Add(currentPlayerOne);
        currentPlayers.Add(currentPlayerTwo);

        /*players themselves also track their round so that round stays syncronized
        all players who have played should have a higher round than currentRound */
        currentPlayerOne.CurrentRound = currentPlayerOne.CurrentRound++;
        currentPlayerTwo.CurrentRound = currentPlayerTwo.CurrentRound++;

        foundGame.currentMatch++;
        return currentPlayers;
    }
    //api route EndMatch
    public async Task<bool> EndMatchAsync(Guid gameId, Player matchWinner)
    {

        Log.Information($"Info: End Match Async {gameId}");

        try
        {


            var foundGame = _games.Find(g => g.Id == gameId);
            if (foundGame is null)
            {
                throw new GameNotFoundException("EndRoundAsync");
            }


            var finalVoteSuccess = VoteHandler(gameId, matchWinner);

            if (!finalVoteSuccess)
            {
                throw new InvalidFunctionResponseException("EndMatchAsync");
            }


            var success = await PostMatchShutdown(foundGame.Id);

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
    }

    //Handle votes for match winner
    private bool VoteHandler(Guid gameId, Player MatchWinner)

    {

        Log.Information($"Info: VoteHandlerAsync {gameId}");

        try
        {

            //get game context and current players 
            var foundGame = _games.Find(g => g.Id == gameId);

            if (foundGame is null)
            {
                throw new GameNotFoundException("VoteHandlerAsync");
            }

            MatchWinner = foundGame.currentPlayers.Find(p => p.Id == MatchWinner.Id);

            if (MatchWinner is null)
            {
                throw new PlayerNotFoundException("VoteHandlerAsync");
            }

            MatchWinner = foundGame.currentPlayers.Find(p => p.Id == MatchWinner.Id);
            if (MatchWinner is null)
            {
                throw new PlayerNotFoundException("VoteHandlerAsync");
            }

            CastVote(foundGame, MatchWinner);
        }
        catch (PlayerNotFoundException e)
        {
            Log.Error($"{e}");
            return false;
        }
        return true;
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

    //Calculate highest score in game
    private Game CalculateHighestScore(Game currentGame)
    {
        Player highestScore = new Player();
        foreach (Player player in currentGame.currentPlayers)
        {
            //bubble sort to determine who has highest score
            if (player.CurrentScore > highestScore.CurrentScore)
            {
                highestScore = player;
            }
        }
        return currentGame;
    }

    //called by SaveGameAsync or EndGameAsync
    private async Task<bool> PostMatchShutdown(Guid gameId)
    {
        Log.Information($"Info: UpdateUserScore {gameId}");

        //create userService context
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserManager>();
            try
            {
                var foundGame = _games.Find(g => g.Id == gameId);

                if (foundGame is null)
                {
                    throw new GameNotFoundException("UpdateUserScore");
                }

                foundGame = CalculateHighestScore(foundGame);

                foreach (Player player in foundGame.currentPlayers)
                {
                    var currentUser = await userRepository.GetUserByIdAsync(player.UserId);
                    //"real" user is an actual user and not a user created to allow players to skip rounds 

                    //bye users always lose there is no need to process their score
                    bool isUserReal = IsRealUser(currentUser, player.UserId);

                    if (isUserReal)
                    {
                        //set new timestamps for relevant timestamp fields
                        //increment and decrement totals wins / losses / games played
                        if (foundGame.currentRound == player.CurrentRound)
                        {
                            UpdateUser(currentUser, player);
                            await userRepository.UpdateUserAsync(currentUser);
                        }
                        else
                        {
                            Log.Warning("Warning: UpdateUserScore called, but some users aren't on the same round as their game");
                        }

                    }


                }
            }
            catch (GameNotFoundException e)
            {
                Log.Error($"{e}");
            }
        }
        return true;
    }

    //validate if user is null object pattern (bye user)
    //or "real" ApplicationUser
    private bool IsRealUser(ApplicationUser currentUser, string userId)
    {
        if (currentUser is null && userId.Equals(AppConstants.ByeUserId))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    //Update User Values Post Match 
    private ApplicationUser UpdateUser(ApplicationUser currentUser, Player player)
    {
        currentUser.AllTimeMatches = player.CurrentRound + currentUser.AllTimeMatches;
        var currentWins = Math.Abs(player.CurrentScore - player.CurrentRound);
        currentUser.AllTimeWins = currentWins + currentUser.AllTimeWins;
        var currentLosses = Math.Abs(player.CurrentRound - currentWins);

        if (currentLosses + currentWins != player.CurrentRound)
        {
            throw new InvalidObjectStateException("UpdateUserScore");
        }

        return currentUser;
    }

}