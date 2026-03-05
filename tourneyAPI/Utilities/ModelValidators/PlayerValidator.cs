using CustomExceptions;
using Entities;
using System.Diagnostics.CodeAnalysis;
using Enums;
using System;

namespace Validators;

[ExcludeFromCodeCoverage]
// Validates domain models before persistence or gameplay processing.
public class PlayerValidator()
{
    // Applies business-rule validation and throws a domain exception when input is invalid.
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