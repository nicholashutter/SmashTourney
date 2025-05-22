namespace Services;

using Entities;
public interface IPlayerService
{
    Task<Player> CreateAsync(Guid userId, string displayName);
    Task<Player> GetByIdAsync(Guid id);
    Task<IEnumerable<Player>> GetAllAsync();
    Task<Player> UpdateAsync(
    Guid id,
    string? displayName = null,
    int? currentScore = null,
    int? currentRound = null,
    string? currentOpponent = null,
    string? currentCharacter = null,
    Guid? currentGameSessionID = null,
    bool? hasVoted = null,
    Guid? roundVote = null);
    Task<bool> DeleteAsync(Guid id);

}