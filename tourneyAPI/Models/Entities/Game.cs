using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
    public DateTime CurrentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public int CurrentRound { get; set; } = 0;
    public Votes _votes { get; set; }

    public void AddPlayer(Player player)
    {
        CurrentPlayers.Add(player);
    }

    public void RemovePlayer(Guid playerId)
    {
        var player = CurrentPlayers.Find(p => p.Id == playerId);
        if (player != null)
            CurrentPlayers.Remove(player);
    }
}
