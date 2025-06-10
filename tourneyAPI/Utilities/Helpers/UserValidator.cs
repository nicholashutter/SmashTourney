using CustomExceptions;
using Entities;

namespace Validators;

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