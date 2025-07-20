namespace Services;

using Microsoft.EntityFrameworkCore;
using System;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public override DbSet<ApplicationUser> Users { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Character> Characters { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {

    }
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

