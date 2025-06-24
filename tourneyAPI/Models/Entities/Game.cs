using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Net.Http.Headers;
using System;

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
    public List<Player> currentPlayers { get; set; } = new List<Player>();

    [Required]
    public DateTime currentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public int currentRound { get; set; } = 0;

    public int currentMatch { get; set; } = 0;

    public Votes _votes = Votes.ZERO;

    private readonly object lockObject = new object();

    public Votes GetVotes()
    {
        return _votes;
    }

    public void SetVotes(Votes votes)
    {
        lock (lockObject)
        {
            _votes = votes;
        }
    }



    public int byes { get; set; } = 0;
}
