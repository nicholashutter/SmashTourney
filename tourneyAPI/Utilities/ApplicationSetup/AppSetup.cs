namespace Helpers;

using Serilog;
using Microsoft.AspNetCore.Authentication.Cookies;
using Services;
using Serilog;
using CustomExceptions;

public class AppSetup
{
    public static void SetupLogging()
    {
        //setup logger system using serlog library
        //typical implementation for writing to file and console
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            //WriteTo.File provides a path inside the logs folder filename is today's date
            .WriteTo.File($"logs/{DateOnly.FromDateTime(DateTime.Now).ToString("MMMM dd yyyy")}")
            .CreateLogger();
    }

    public static async Task HandleUserSession(CookieSignedInContext context, string username)
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