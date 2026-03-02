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

        try
        {
            _ = Guid.Parse(validatePlayer.UserId);
        }
        catch (FormatException)
        {
            throw new PlayerValidationException($"{TAG}: UserId is not a valid GUID.");
        }

        if (validatePlayer.CurrentGameID == Guid.Empty)
        {
            throw new PlayerValidationException($"{TAG}: CurrentGameID cannot be an empty GUID.");
        }
    }
}