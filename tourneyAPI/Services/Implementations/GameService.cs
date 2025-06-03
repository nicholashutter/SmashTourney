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
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Serilog;

public class GameService : IGameService
{

    //internal reference to dbContextFactory which allows
    //for local access to db inside of singleton class
    private readonly IDbContextFactory<AppDBContext> _dbContextFactory;

    private readonly IServiceScopeFactory _scopeFactory;

    //list that holds all currently played games in memory
    private List<Game> _games;

    //list that represents users authenticated but not in a specific game
    public List<User> _lobby;


    //GameService has a singleton lifetime and is created on application start
    public GameService(IDbContextFactory<AppDBContext> dbContextFactory, IServiceScopeFactory scopeFactory)
    {

        _dbContextFactory = dbContextFactory;
        _scopeFactory = scopeFactory;
        _games = new List<Game>();
        _lobby = new List<User>();
    }

    //api route /NewGame 
    public Task<Guid> CreateGame()
    {

        Log.Information($"Info: CreateGame");

        Task<Guid> result = Task.Run(() =>
        {
            Game game = new Game();
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
    public async Task<Game?> GetGameByIdAsync(Guid Id)
    {
        Log.Information($"Info: Get Game By Id {Id}");

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
                Log.Information($"Error: Game not found or otherwise null\n {e}");
                return null;
            }
        }


    }


    //api route /GetAllGames (debug)
    public async Task<IEnumerable<Game>?> GetAllGamesAsync()
    {
        Log.Information($"Info: Get All Games");

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
                Log.Warning($"All Games Returns Zero, Did You Just Reset The DB? \n {e}");
                return null;
            }
        }
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
                if (foundGame is null)
                {
                    foundGame = await LoadGameAsync(existingGameId);
                    if (foundGame is null)
                    {
                        foundGame = _games.Find(g => g.Id == existingGameId);
                        if (foundGame is null)
                        {
                            throw new GameNotFoundException("StartGameAsync");
                        }
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

    //api route //StartRound
    public Task<List<Player>?> StartRound(Guid gameId)
    {
        return StartMatch(gameId);
    }

    //api route StartMatch
    //list of players should be unpacked by route and returned in HTTP response
    public Task<List<Player>?> StartMatch(Guid gameId)
    {
        Log.Information($"Info: StartMatch {gameId}");

        Task<List<Player>?> result = Task.Run(() =>
        {
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
                var currentPlayerTwo = foundGame.currentPlayers[foundGame.currentMatch + 1];

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

        });
        return result;
    }

    //api route EndMatch
    public Task<bool> EndMatchAsync(Guid gameId, Player matchWinner, Player matchLoser)
    {

        Log.Information($"Info: End Match Async {gameId}");

        Task<bool> result = Task.Run(async () =>
        {
            try
            {
                var foundGame = _games.Find(g => g.Id == gameId);
                if (foundGame is null)
                {
                    throw new GameNotFoundException("EndRoundAsync");
                }
                var success = await VoteHandlerAsync(gameId, matchWinner, matchLoser);

                if (!success)
                {
                    throw new InvalidFunctionResponseException("EndMatchAsync");
                }

                success = await UpdateUserScore(foundGame.Id);

                if (!success)
                {
                    throw new InvalidFunctionResponseException("EndMatchAsync");
                }

                success = await EndGameAsync(foundGame.Id);

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
        });



        return result;
    }

    //api route /EndRound
    public Task<bool> EndRoundAsync(Guid gameId, Player RoundWinner, Player RoundLoser)
    {

        Log.Information($"Info: EndRoundAsync {gameId}");

        Task<bool> result = Task.Run(() =>
        {
            try
            {
                var foundGame = _games.Find(g => g.Id == gameId);
                if (foundGame is null)
                {
                    throw new GameNotFoundException("EndRoundAsync");
                }
                //this should be the only method that iterates this property
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


    //LoadGameAsync will check against the db and attempt to restore
    //game state from games persisted in db
    public Task<Game?> LoadGameAsync(Guid gameId)
    {
        Log.Information($"Info: Create New Game Async");

        Task<Game?> result = Task.Run(async () =>
        {
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
                    Log.Warning($"Error: foundGame is null. Unable to load game not found \n {e}");
                    return null;
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
                    Log.Error($"Error: {e}");
                    return false;
                }
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
                else if (foundGame.currentPlayers.Count < 5)
                {
                    foundGame.byes = Math.Abs(foundGame.currentPlayers.Count - 4);
                }
                else if (foundGame.currentPlayers.Count < 9)
                {
                    foundGame.byes = Math.Abs(foundGame.currentPlayers.Count - 8);
                }
                else if (foundGame.currentPlayers.Count < 17)
                {
                    foundGame.byes = Math.Abs(foundGame.currentPlayers.Count - 16);
                }
                else if (foundGame.currentPlayers.Count < 33)
                {
                    foundGame.byes = Math.Abs(foundGame.currentPlayers.Count - 32);
                }
                else if (foundGame.currentPlayers.Count < 65)
                {
                    foundGame.byes = Math.Abs(foundGame.currentPlayers.Count - 64);
                }
                else if (foundGame.currentPlayers.Count < 129)
                {
                    foundGame.byes = Math.Abs(foundGame.currentPlayers.Count - 128);
                }
                else if (foundGame.currentPlayers.Count < 257)
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

                foreach (User user in _lobby)
                {
                    foreach (Player player in players)
                    {
                        if (user.Id == player.UserId)
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

    //Api route /AddUserToLobby
    //will only let authenticated users further inside the application if the gameID presented is valid
    public Task<bool> AddUserToLobby(User addUser, Guid gameId)
    {
        Log.Information($"Info: Adding User {addUser.Username} to game lobby.");

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
                            Log.Information($"User {addUser.Username} Added To Game {gameId} lobby");
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

    //called by SaveGameAsync or EndGameAsync
    public Task<bool> UpdateUserScore(Guid gameId)
    {
        Log.Information($"Info: UpdateUserScore {gameId}");

        Task<bool> result = Task.Run(async () =>
        {
            //create userService context
            await using (var scope = _scopeFactory.CreateAsyncScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<UserRepository>();

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



    //called by endRoundAsync
    //may become private
    public Task<bool> VoteHandlerAsync(Guid gameId, Player roundWinner, Player roundLoser)
    {

        Log.Information($"Info: VoteHandlerAsync {gameId}");

        Task<bool> result = Task.Run(() =>
        {
            try
            {

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

                var currentVotes = foundGame.votes;
                //only once two votes are received should the round move forward
                //use the submitted players Id to increment the game VOTE enum 
                //set the players individual properties as winner and loser 
                currentVotes = (Votes)((int)currentVotes + 1);

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