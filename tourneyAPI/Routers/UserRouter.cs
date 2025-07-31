namespace Routers;

using Entities;
using Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services;
using System;
using Serilog;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;

/*

This class was written prior to integrating my User model into efcore as a IdentityUser
This class may be removed at some point

*/
[ExcludeFromCodeCoverage]
public static class UserRouter
{

    public static void Map(WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AppLog");

        var UserRoutes = app.MapGroup("/users");

        UserRoutes.MapGet("/", async () =>
        {

            Log.Information("Request Type: Get \n URL: '/Users' \n Time: {Timestamp}", DateTime.UtcNow);

            using var scope = app.Services.CreateAsyncScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<UserManager>();

            var users = await userRepository.GetAllUsersAsync();

            if (users is not null)
            {
                return Results.Ok(users);
            }
            else
            {
                return Results.StatusCode(500);
            }

        }).RequireAuthorization();


        UserRoutes.MapDelete("/{Id}", async (HttpContext context, string Id) =>
        {
            Log.Information("Request Type: Delete \n URL: '/Users \n Time:{Timestamp}", DateTime.UtcNow);

            using (var scope = app.Services.CreateAsyncScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<UserManager>();

                var result = await userRepository.DeleteUserAsync(Id);

                if (result.Succeeded)
                {
                    return Results.StatusCode(200);
                }
                else
                {
                    return Results.StatusCode(500);
                }
            }


        }).RequireAuthorization();

        UserRoutes.MapPost("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);

            return Results.Ok();
        }).RequireAuthorization();
    }
}
