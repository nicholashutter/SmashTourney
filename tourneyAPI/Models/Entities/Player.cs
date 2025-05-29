using System.ComponentModel.DataAnnotations;

namespace Entities;

public class Player
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string DisplayName { get; set; } = "";
    public int CurrentScore { get; set; } = 0;
    public int CurrentRound { get; set; } = 0;

    [Required]
    public string CurrentOpponent { get; set; } = "";

    [Required]
    public string CurrentCharacter { get; set; } = "";

    [Required]
    public DateTime CurrentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid CurrentGameID { get; set; }
    public bool HasVoted { get; set; } = false;
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