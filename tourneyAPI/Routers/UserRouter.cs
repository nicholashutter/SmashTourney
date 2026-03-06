namespace Routers;

using Entities;
using Services;
using System;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Helpers;
using System.Security.Claims;

// Maps user authentication and account management endpoints.
public static class UserRouter
{
    // Represents a username/password login payload.
    private sealed class LoginRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Registers all user API endpoints.
    public static void Map(WebApplication app)
    {
        var userRoutes = app.MapGroup("/users");

        userRoutes.MapPost("/login", async (
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

        userRoutes.MapGet("/demo-credentials", (IHostEnvironment environment) =>
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

        userRoutes.MapGet("/session", (ClaimsPrincipal user) =>
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

        userRoutes.MapPost("/logout", async (HttpContext context, IGameService gameService) =>
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
