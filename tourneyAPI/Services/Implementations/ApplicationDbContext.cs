namespace Services;

using Microsoft.EntityFrameworkCore;
using System;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

// Defines the EF Core database context for identity, games, players, and characters.
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public override DbSet<ApplicationUser> Users { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Character> Characters { get; set; } = null!;

    // Creates a new database context instance from configured options.
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {

    }

    // Builds the production SQLite connection string in local app data.
    public static string SetupProd()
    {
        var dbFileName = "tourneyDb.db";
        var applicationTitle = "tourneyAPI";

        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string dbFolder = Path.Combine(appDataPath, applicationTitle);
        Directory.CreateDirectory(dbFolder);
        string dbPath = $"DataSource={Path.Combine(dbFolder, dbFileName)}";
        return dbPath;
    }

    
}

