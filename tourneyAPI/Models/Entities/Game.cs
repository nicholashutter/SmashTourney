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

    // When this game last did something worth keeping it alive for.
    //
    // Creation time alone cannot decide what to clean up: a tournament of
    // sixty-four people legitimately runs for hours, while a lobby somebody
    // opened and walked away from is dead within minutes of being made. Both
    // look identical by created date. This moves whenever players join or the
    // bracket advances, so age here means genuine silence rather than duration.
    public DateTime LastActivityUtc { get; set; } = DateTime.UtcNow;

    // When the bracket finished, or null while it is still running.
    //
    // A finished tournament has a shorter useful life than an abandoned one —
    // people want to look up who won for a while, and then never again — so it
    // is swept on its own, shorter clock rather than waiting out the silence
    // window that catches abandoned lobbies.
    public DateTime? CompletedUtc { get; set; }

    [Required]
    public int currentRound { get; set; } = 0;

    public int currentMatch { get; set; } = 0;

    public int byes { get; set; } = 0;
}
