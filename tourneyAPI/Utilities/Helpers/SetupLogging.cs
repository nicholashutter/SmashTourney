using Serilog;

namespace Helpers;

public class SetupLogging
{
    public static void Setup()
    {
        //setup logger system using serlog library
        //typical implementation for writing to file and console
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            //WriteTo.File provides a path inside the logs folder filename is today's date
            .WriteTo.File($"logs/{DateOnly.FromDateTime(DateTime.Now).ToString("MMMM dd yyyy")}")
            .CreateLogger();
    }
}