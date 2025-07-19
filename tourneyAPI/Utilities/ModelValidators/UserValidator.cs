using CustomExceptions;
using Entities;
using System.Diagnostics.CodeAnalysis;

namespace Validators;

[ExcludeFromCodeCoverage]
public class UserValidator()
{
    public static void Validate(ApplicationUser validateUser, string TAG)
    {
        if (validateUser.UserName is null || validateUser.Email is null)
        {
            throw new UserValidationException(TAG);
        }

    }
}