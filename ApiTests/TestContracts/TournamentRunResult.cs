namespace ApiTests.TestContracts;

// Represents completion metrics for one end-to-end tournament run.
public sealed class TournamentRunResult
{
    public Guid GameId { get; set; }
    public int ReportedMatches { get; set; }
}
