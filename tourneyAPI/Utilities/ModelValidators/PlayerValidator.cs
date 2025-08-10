using CustomExceptions;
using Entities;
using System.Diagnostics.CodeAnalysis;
using Enums;
using System;

namespace Validators;

[ExcludeFromCodeCoverage]
public class PlayerValidator()
{
    public static void Validate(Player validatePlayer, string TAG)
    {
        if (string.IsNullOrWhiteSpace(validatePlayer.DisplayName))
        {
            throw new PlayerValidationException($"{TAG}: DisplayName cannot be blank.");
        }

        if (string.IsNullOrWhiteSpace(validatePlayer.UserId))
        {
            throw new PlayerValidationException($"{TAG}: UserId cannot be blank.");
        }

        if (!Guid.TryParse(validatePlayer.UserId, out _))
        {
            throw new PlayerValidationException($"{TAG}: UserId is not a valid GUID.");
        }

        if (validatePlayer.CurrentOpponent == Guid.Empty)
        {
            throw new PlayerValidationException($"{TAG}: CurrentOpponent cannot be an empty GUID.");
        }

        if (validatePlayer.CurrentCharacter == CharacterName.NONE)
        {
            throw new PlayerValidationException($"{TAG}: CurrentCharacter cannot be NONE.");
        }

        if (validatePlayer.CurrentGameID == Guid.Empty)
        {
            throw new PlayerValidationException($"{TAG}: CurrentGameID cannot be an empty GUID.");
        }
    }
}