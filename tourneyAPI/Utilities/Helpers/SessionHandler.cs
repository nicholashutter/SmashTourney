using Microsoft.AspNetCore.Authentication.Cookies;
using Services;
using Serilog;
using CustomExceptions;

public static class SessionHandler
{
    public static async Task HandleSession(CookieSignedInContext context, string username)
    {
        if (username is not null)
        {
            using (var scope = context.HttpContext.RequestServices.CreateAsyncScope())
            {
                var scopedServices = scope.ServiceProvider;

                var _gameService = scopedServices.GetRequiredService<IGameService>();
                var _userRepository = scopedServices.GetRequiredService<IUserManager>();

                var foundUser = await _userRepository.GetUserByUserNameAsync(username);

                if (foundUser is not null)
                {
                    _gameService.CreateUserSession(foundUser);
                }
                else
                {
                    throw new UserNotFoundException("HandleSession: TILT - should not get here");
                }
            }
        }
    }
}