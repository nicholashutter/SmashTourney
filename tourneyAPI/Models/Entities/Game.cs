using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Net.Http.Headers;
using System;
using Enums;

namespace Entities;

public enum Votes
{
    ZERO,
    ONE,
    TWO
}

public class Game
{
    public Guid Id { get; set; }
    public List<Player> currentPlayers { get; set; } = new List<Player>();

    [Required]
    public DateTime currentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public int currentRound { get; set; } = 0;

    public int currentMatch { get; set; } = 0;

    public Votes _votes = Votes.ZERO;

    private readonly object lockVotes = new object();

    private readonly object lockCurrentRound = new object();

    public Votes GetVotes()
    {
        return _votes;
    }

    public void SetVotes(Votes votes)
    {
        lock (lockVotes)
        {
            _votes = votes;
        }
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public void SetCurrentRound(int newRound)
    {
        lock (lockCurrentRound)
        {
            currentRound = newRound;
        }
    }


    public int byes { get; set; } = 0;
}
