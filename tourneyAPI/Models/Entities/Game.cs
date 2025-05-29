using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Net.Http.Headers;

namespace Entities;

public enum Votes
{
    ZERO,
    ONE,
    TWO
}

public class Game
{
    [Required]
    public Guid Id { get; set; }
    public List<Player> CurrentPlayers { get; set; } = new List<Player>();

    [Required]
    public DateTime currentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public int currentRound { get; set; } = 0;
    public Votes votes { get; set; } = Votes.ZERO;

    public int byes { get; set; } = 0;
}
