namespace Services.Brackets;

using Enums;

internal sealed class BracketPlayerRuntime
{
    public Guid PlayerId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int Seed { get; set; }
    public int Losses { get; set; }
    public bool Eliminated { get; set; }
}

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
}

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

    public Dictionary<int, List<Guid>> WinnersPools { get; } = new();
    public Dictionary<int, List<Guid>> LosersPools { get; } = new();

    public int WinnersMatchCounter { get; set; }
    public int LosersMatchCounter { get; set; }
    public int FinalsMatchCounter { get; set; }
}
