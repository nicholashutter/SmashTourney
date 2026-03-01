namespace Routers;

using Entities;
using Services;
using System;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Helpers;
using System.Security.Claims;

/*

This class was written prior to integrating my User model into efcore as a IdentityUser
This class may be removed at some point

*/
public static class UserRouter
{
    private sealed class LoginRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public static void Map(WebApplication app)
    {
        var UserRoutes = app.MapGroup("/users");

        UserRoutes.MapPost("/login", async (
            LoginRequest loginRequest,
            UserManager<ApplicationUser> identityUserManager,
            SignInManager<ApplicationUser> signInManager) =>
        {
            Log.Information("Request Type: Post \n URL: '/users/login' \n Time: {Timestamp}", DateTime.UtcNow);

            if (string.IsNullOrWhiteSpace(loginRequest.UserName) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return Results.BadRequest("Username and password are required.");
            }

            var foundUser = await identityUserManager.FindByNameAsync(loginRequest.UserName)
                ?? await identityUserManager.FindByEmailAsync(loginRequest.UserName);

            if (foundUser is null)
            {
                return Results.Unauthorized();
            }

            var signInResult = await signInManager.PasswordSignInAsync(
                foundUser,
                loginRequest.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new { Message = "Login successful" });
        });

        UserRoutes.MapGet("/demo-credentials", (IHostEnvironment environment) =>
        {
            if (!environment.IsDevelopment())
            {
                return Results.NotFound();
            }

            return Results.Ok(new
            {
                UserName = AppConstants.DemoUserName,
                Password = AppConstants.DemoUserPassword
            });
        });

        UserRoutes.MapGet("/session", (ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userName = user.Identity?.Name ?? string.Empty;

            return Results.Ok(new
            {
                IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
                UserId = userId,
                UserName = userName
            });
        }).RequireAuthorization();

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

        UserRoutes.MapDelete("/{Id}", async (HttpContext context, string Id, IUserManager userRepository) =>
        {
            Log.Information("Request Type: Delete \n URL: '/Users \n Time:{Timestamp}", DateTime.UtcNow);

            var result = await userRepository.DeleteUserAsync(Id);

                if (result.Succeeded)
                {
                    return Results.StatusCode(200);
                }
                else
                {
                    return Results.StatusCode(500);
                }


        }).RequireAuthorization();



        UserRoutes.MapPost("/logout", async (HttpContext context, IGameService gameService) =>
        {
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);

            Log.Information("Request Type: Post \n URL: '/users/logout' \n Time: {Timestamp}", DateTime.UtcNow);

            bool success = gameService.EndUserSession(context.User);

            if (!success)
            {
                return Results.Problem("Internal Server Error");
            }

            return Results.Ok();
        }).RequireAuthorization();
    }
}
