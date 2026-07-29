using System.ComponentModel.DataAnnotations;
using System;
using Enums;

namespace Entities;

// Represents a tournament game and its persisted progression state.
public class Game
{
    public Guid Id { get; set; }
    public BracketMode BracketMode { get; set; } = BracketMode.SINGLE_ELIMINATION;
    public string? BracketStateJson { get; set; }

    // Identity of whoever created the game.
    //
    // Host-ness used to live only in the browser's sessionStorage, so a host who
    // lost their tab came back as an ordinary player and nobody could start the
    // tournament. Storing it with the game means the server can answer the
    // question instead of trusting the client to remember it.
    public string? HostUserId { get; set; }
    public List<Player> currentPlayers { get; set; } = new List<Player>();

    [Required]
    public DateTime currentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public int currentRound { get; set; } = 0;

    public int currentMatch { get; set; } = 0;

    private Votes _currentVotes = Votes.ZERO;

    private readonly object _votesLock = new object();

    private readonly object _currentRoundLock = new object();

    // Returns the current game vote state.
    public Votes GetVotes()
    {
        return _currentVotes;
    }

    // Updates the current game vote state.
    public void SetVotes(Votes votes)
    {
        lock (_votesLock)
        {
            _currentVotes = votes;
        }
    }

    // Returns the current round number.
    public int GetCurrentRound()
    {
        return currentRound;
    }

    // Updates the current round number.
    public void SetCurrentRound(int newRound)
    {
        lock (_currentRoundLock)
        {
            currentRound = newRound;
        }
    }


    public int byes { get; set; } = 0;
}
