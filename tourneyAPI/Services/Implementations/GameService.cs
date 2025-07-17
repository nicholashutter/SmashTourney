namespace Services;

/* GameService implements game operation logic */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;
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

    //string representing a dummy user entity to allow
    //a player entity to skip a round (bye)
    const string ByeUserId = "00000000-0000-0000-0000-000000000000";

    //GameService has a singleton lifetime and is created on application start
    public GameService(IServiceScopeFactory scopeFactory, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _scopeFactory = scopeFactory;
        _games = new List<Game>();
        _userSessions = new List<ApplicationUser>();
    }

    //api route /NewGame 
    public Guid CreateGame()
    {

        Log.Information($"Info: CreateGame");

        Game game = new Game
        {
            Id = Guid.NewGuid()
        };
        _games.Add(game);
        Log.Information($"Info: New Game with gameId {game.Id}");
        return game.Id;
    }

    //api route /SaveGame
    //SaveGame will persist current game state to the database
    public async Task<bool> SaveGameAsync(Guid gameId)
    {
        Log.Information($"Info: Create New Game Async");

        using (var scope = _serviceProvider.CreateScope())
        {
            //get database context
            var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            //check for _games in memory first
            var foundGame = _games.Find(g => g.Id == gameId);

            //if not in memory (_games) check the db
            if (foundGame is null)
            {
                foundGame = await _db.Games.FindAsync(gameId);
                //if not in memory or the db throw exception
                if (foundGame is null)
                {
                    //TILT should not get here
                    throw new GameNotFoundException("SaveGameAsync");
                }
            }
            else
            {
                //check if in memory and db (if already exists)
                var dbGame = await _db.Games.FindAsync(gameId);

                //if not exists, create
                if (dbGame is null)
                {
                    _db.Games.Add(foundGame);
                }
                //if exists, update
                else
                {
                    _db.Games.Update(foundGame);
                }

                //save the operation in efcore 
                await _db.SaveChangesAsync();
            }
            return true;
        }
    }
    //LoadGameAsync will check against the db and attempt to restore
    //game state from games persisted in db
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
    //players list should come from the httprequest from the front end
    public bool AddPlayersToGame(List<Player> players, Guid gameId)
    {
        Log.Information($"Info: Loading Users and Creating Their Corresponding Players");

        try
        {
            //only adds users already in lobby to game if their UserId from the back end and 
            //userId from the player object submitted by the front end agree
            var foundGame = _games.Find(g => g.Id == gameId);

            if (foundGame is null)
            {
                throw new GameNotFoundException("AddPlayersToGameAsync");
            }

            //check each User entity if in _userSessions
            //check each Player in Players (from front end)
            //where Player.UserId matches a user with a session
            //add player to gameId also provided by front end
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
    //called by StartGame
    public bool GenerateBracket(Guid gameId)
    {
        Log.Information($"Info: Generate Bracket {gameId}");

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
                foundGame.currentPlayers.Add(new Player
                {
                    Id = Guid.NewGuid(), 
                    UserId = ByeUserId
                });
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
    }

    //api route StartGame
    public async Task<bool> StartGameAsync(Guid existingGameId)

    {

        Log.Information($"Info: End Game {existingGameId}");

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

            //load two new players
            List<Player> currentPlayers = new List<Player>();


            //two players loaded into return array based on round counter
            //so that two players are loaded every round and should stay in sync with bracket
            var currentPlayerOne = foundGame.currentPlayers[foundGame.currentMatch];
            var currentPlayerTwo = foundGame.currentPlayers[foundGame.currentMatch++];

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
    }
    //api route EndMatch
    public async Task<bool> EndMatchAsync(Guid gameId, Player matchWinner, Player matchLoser)
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


            var success = await UpdateUserScoreAsync(foundGame.Id);

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
    //called by endRoundAsync
    //may become private
    public bool VoteHandler(Guid gameId, Player MatchWinner)

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
                    MatchWinner.CurrentScore++;
                    break;
            }
        }
        catch (PlayerNotFoundException e)
        {
            Log.Error($"{e}");
            return false;
        }
        return true;
    }

    //called by SaveGameAsync or EndGameAsync
    public async Task<bool> UpdateUserScoreAsync(Guid gameId)
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

                //foundGame is GetGameByIdAsync()
                //if foundGame is not null

                Player highestScore = new Player();

                foreach (Player player in foundGame.currentPlayers)
                {
                    //iterate over players, get their userIds
                    //iterate over list<userIds> use current player score values
                    var currentUser = await userRepository.GetUserByIdAsync(player.UserId);

                    //bubble sort to determine who has highest score
                    if (player.CurrentScore > highestScore.CurrentScore)
                    {
                        highestScore = player;
                    }

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
    }

}