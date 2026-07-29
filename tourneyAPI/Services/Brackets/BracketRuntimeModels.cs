namespace Services.Brackets;

using Enums;

// Stores in-memory runtime state used to build and progress tournament brackets.
internal sealed class BracketPlayerRuntime
{
    public Guid PlayerId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int Seed { get; set; }
    public int Losses { get; set; }
    public bool Eliminated { get; set; }
}

// Stores in-memory runtime state used to build and progress tournament brackets.
internal sealed class BracketMatchRuntime
{
    public Guid MatchId { get; set; } = Guid.NewGuid();
    public BracketLane Lane { get; set; }
    public int Round { get; set; }
    public int MatchNumber { get; set; }
    public Guid? PlayerOneId { get; set; }
    public Guid? PlayerTwoId { get; set; }
    public Guid? WinnerId { get; set; }
    public BracketMatchStatus Status { get; set; } = BracketMatchStatus.PENDING;
    public Guid? NextMatchForWinner { get; set; }
    public Guid? NextMatchForLoser { get; set; }

    // Identifies which participant slot of the next match this result feeds.
    // The bracket is built as a fixed tree before play starts, so an advancing
    // player has a reserved position rather than whichever slot happens to be
    // free — that is what keeps a bracket's shape independent of the order
    // results are reported in.
    public int NextSlotForWinner { get; set; }
    public int NextSlotForLoser { get; set; }
}

// Stores in-memory runtime state used to build and progress tournament brackets.
internal sealed class BracketRuntimeState
{
    public Guid GameId { get; set; }
    public BracketMode Mode { get; set; }
    public bool GameStarted { get; set; }
    public bool IsGrandFinalResetRequired { get; set; }
    public Guid? WinnersChampionId { get; set; }
    public Guid? LosersChampionId { get; set; }

    public List<BracketPlayerRuntime> Players { get; set; } = new();
    public List<BracketMatchRuntime> Matches { get; set; } = new();
    public HashSet<Guid> ByePlayerIds { get; set; } = new();

    // Votes cast for a match that has not reached consensus yet, keyed by match
    // and then by voting user.
    //
    // This lived in a field on the service, which meant a restart between the
    // first player tapping a winner and the second one doing so lost the first
    // vote silently. Keeping it here is what makes it part of the same
    // write-through as the bracket itself: one serialize, one row, one story
    // about what the tournament currently is.
    public Dictionary<Guid, Dictionary<string, Guid>> PendingVotes { get; set; } = new();

    public int WinnersMatchCounter { get; set; }
    public int LosersMatchCounter { get; set; }
    public int FinalsMatchCounter { get; set; }
}
