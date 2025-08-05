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
        var UserRoutes = app.MapGroup("/users");

        UserRoutes.MapGet("/GetAllUsers", async (HttpContext context, IUserManager userManager) =>
        {

            Log.Information("Request Type: Get \n URL: '/GetAllUsers' \n Time: {Timestamp}", DateTime.UtcNow);

            var users = await userManager.GetAllUsersAsync();

            if (users is not null)
            {
                return Results.Ok(users);
            }
            else
            {
                return Results.StatusCode(500);
            }

        }).RequireAuthorization();

        UserRoutes.MapGet("/GetById{Id}", async (HttpContext context, IUserManager userManager, string Id) =>
        {

            Log.Information("Request Type: Get \n URL: '/GetAllUsers' \n Time: {Timestamp}", DateTime.UtcNow);

            var foundUser = await userManager.GetUserByIdAsync(Id);

            if (foundUser is not null)
            {
                return Results.Ok(foundUser);
            }
            else
            {
                return Results.StatusCode(500);
            }

        }).RequireAuthorization();

        UserRoutes.MapGet("/GetByUserName{UserName}", async (HttpContext context, IUserManager userManager, string UserName) =>
        {

            Log.Information("Request Type: Get \n URL: '/GetAllUsers' \n Time: {Timestamp}", DateTime.UtcNow);

            var foundUser = await userManager.GetUserByIdAsync(UserName);

            if (foundUser is not null)
            {
                return Results.Ok(foundUser);
            }
            else
            {
                return Results.StatusCode(500);
            }

        }).RequireAuthorization();

        UserRoutes.MapPut("/UpdateUser", async (HttpContext context, IUserManager userManager, ApplicationUser user) =>
      {

          Log.Information("Request Type: Get \n URL: '/GetAllUsers' \n Time: {Timestamp}", DateTime.UtcNow);

          var results = await userManager.UpdateUserAsync(user);

          if (results.Succeeded)
          {
              return Results.Ok();
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
