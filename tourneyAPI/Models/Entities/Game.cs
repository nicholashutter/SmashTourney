using System.ComponentModel.DataAnnotations;

namespace Entities;

public class Game
{
    public Guid Id { get; set; }
    public List<Player> CurrentPlayers { get; set; } = new List<Player>();
    public DateTime CurrentDate { get; set; } = DateTime.UtcNow;
    public int CurrentRound { get; set; } = 0;

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
