namespace Services; 

using Entities;

public interface IRoundService
{
    //Gameservice will need to pass itself to roundService and track an instance of roundService

    //api route //StartRound
    Task<List<Player>?> StartRound(Guid gameId);

    //api route /EndRound
    Task<bool> EndRoundAsync(Guid gameId, Player RoundWinner, Player RoundLoser);

    //called by endRoundAsync
    Task<bool> VoteHandlerAsync(Guid gameId, Player RoundWinner, Player RoundLoser);

    /*----------------------------------------------------END ROUND SERVICE---------------------------------------------------- */
}