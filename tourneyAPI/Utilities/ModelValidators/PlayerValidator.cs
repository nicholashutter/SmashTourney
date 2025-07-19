using CustomExceptions;
using Entities;
using System.Diagnostics.CodeAnalysis;

namespace Validators;

[ExcludeFromCodeCoverage]
public class PlayerValidator()
{
    public static void Validate(Player validatePlayer, string TAG)
    {
        if (validatePlayer.CurrentCharacter is ""
        || validatePlayer.DisplayName is "")
        {
            throw new PlayerValidationException(TAG);
        }

    }
}