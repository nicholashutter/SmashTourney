using CustomExceptions;
using Entities;
using System.Diagnostics.CodeAnalysis;

namespace Validators;

[ExcludeFromCodeCoverage]
// Validates domain models before persistence or gameplay processing.
public class GameValidator()
{
    // Applies business-rule validation and throws a domain exception when input is invalid.
    public static void Validate(Game validateGame, string TAG)
    {
        if (validateGame.currentPlayers is null || validateGame.currentRound < 0 || validateGame.currentMatch < 0)
        {
            throw new GameValidationException(TAG);
        }
    }
}