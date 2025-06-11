using CustomExceptions;
using Entities;

namespace Validators;

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