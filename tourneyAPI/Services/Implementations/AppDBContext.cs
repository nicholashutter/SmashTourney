namespace Services;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Entities;

public class AppDBContext : DbContext
{
    public DbSet<ApplicationUser> Users { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Character> Characters { get; set; } = null!;

    public string DbPath { get; }

    public AppDBContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "tourneyDB.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }

}

