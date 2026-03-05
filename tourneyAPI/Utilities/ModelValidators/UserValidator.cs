using CustomExceptions;
using Entities;
using System.Diagnostics.CodeAnalysis;

namespace Validators;

[ExcludeFromCodeCoverage]
// Validates domain models before persistence or gameplay processing.
public class UserValidator()
{
    // Applies business-rule validation and throws a domain exception when input is invalid.
    public static void Validate(ApplicationUser validateUser, string TAG)
    {
        if (validateUser.UserName is null || validateUser.Email is null)
        {
            throw new UserValidationException(TAG);
        }

    }
}