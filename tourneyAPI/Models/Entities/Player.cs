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
    public CharacterName CurrentCharacter { get; set; } = CharacterName.NONE;

    [Required]
    public Guid CurrentGameID { get; set; }
}