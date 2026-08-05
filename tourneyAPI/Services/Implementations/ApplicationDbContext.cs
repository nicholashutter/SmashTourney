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

    // Configures the model beyond what convention infers correctly.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Game.currentPlayers is a carrier, not a navigation.
        //
        // Every place that fills it queries Players by CurrentGameID and assigns
        // the result; nothing ever loads it through EF. But convention saw a
        // collection of Players and invented a second relationship for it, and
        // because CurrentGameID does not match the name it was looking for, it
        // added a shadow GameId column alongside. The table ended up with two
        // columns modelling one thing, only one of which was ever written.
        //
        // Ignoring the property removes the phantom column and changes no
        // behaviour, because the property was never mapped storage in the first
        // place — it is how a game hands its roster to a response.
        modelBuilder.Entity<Game>()
            .Ignore(game => game.currentPlayers);
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

