using CustomExceptions;
using Entities;

namespace Validators;

public class UserValidator()
{
    public static void Validate(User validateUser, string TAG)
    {
        if (validateUser.Username is null || validateUser.Email is null)
        {
            throw new UserValidationException(TAG);
        }

    }
}