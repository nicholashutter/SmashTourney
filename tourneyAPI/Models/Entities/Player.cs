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
    public Guid CurrentOpponent { get; set; }

    [Required]
    public string CurrentCharacter { get; set; } = "";

    [Required]
    public DateTime CurrentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid CurrentGameID { get; set; }
    public bool HasVoted { get; set; } = false;
    public Guid MatchWinner { get; set; }
}