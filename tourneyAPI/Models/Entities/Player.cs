using System.ComponentModel.DataAnnotations;

namespace Entities;

public class Player
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = default!;
    public int CurrentScore { get; set; }
    public int CurrentRound { get; set; }
    public string CurrentOpponent { get; set; } = default!;
    public string CurrentCharacter { get; set; } = default!;
    public DateTime CurrentDate { get; set; }
    public Guid CurrentGameID { get; set; }
    public bool HasVoted { get; set; }
    public Guid RoundVote { get; set; }


    //TODO the below methods need to be extracted to a 
    //player manager service as models should be data structures only
    /*
    public void IncrementWins()
    {
        // Implementation logic here.
    }
    public void IncrementLosses()
    {
        // Implementation logic here.
    }
    public void IncrementMatches()
    {
        // Implementation logic here.
    }
    public void ResetRoundData()
    {
        // Implementation logic here.
    }
    public void CastVote(Guid opponentId)
    {
        RoundVote = opponentId;
        HasVoted = true;
    } */
}