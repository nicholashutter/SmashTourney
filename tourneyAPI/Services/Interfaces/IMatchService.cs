namespace Services;

using Entities;

public interface IMatchService
{
    //Gameservice will need to pass itself to matchService and track an instance of matchService

    //api route StartMatch
    //list of players should be unpacked by route and returned in HTTP response
    Task<List<Player>?> StartMatch(Guid gameId);

    //api route EndMatch
    Task<bool> EndMatchAsync(Guid gameId, Player matchWinner, Player matchLoser);

    /*----------------------------------------------------END Match SERVICE---------------------------------------------------- */

}