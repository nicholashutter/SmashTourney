using System.ComponentModel.DataAnnotations;
using Enums;

namespace Entities;

public class Player
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public required string UserId { get; set; } = "";

    [Required]
    public string DisplayName { get; set; } = "";
    public int CurrentScore { get; set; } = 0;
    public int CurrentRound { get; set; } = 0;

    [Required]
    public Guid CurrentOpponent { get; set; }

    [Required]
    public CharacterName CurrentCharacter { get; set; } = CharacterName.NONE;

    [Required]
    public DateTime CurrentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid CurrentGameID { get; set; }
    public bool HasVoted { get; set; } = false;
    public Guid MatchWinner { get; set; }
}