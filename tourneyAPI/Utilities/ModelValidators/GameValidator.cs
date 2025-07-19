using CustomExceptions;
using Entities;
using System.Diagnostics.CodeAnalysis;

namespace Validators;

[ExcludeFromCodeCoverage]
public class GameValidator()
{
    public static void Validate(Game validateGame, string TAG)
    {
        if (validateGame.currentPlayers is null || validateGame.currentRound < 0 || validateGame.currentMatch < 0)
        {
            throw new GameValidationException(TAG);
        }
    }
}