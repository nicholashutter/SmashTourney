namespace Routers;

public static class HelloWorld
{

    public static void Map(WebApplication app)
    {
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("HelloWorld");

        logger.LogInformation("Receieved request at {Timestamp}", DateTime.UtcNow);

        app.MapGet("/", async context =>
        {
            await context.Response.WriteAsJsonAsync(new { Message = "Hello World" });
        });
    }
}
