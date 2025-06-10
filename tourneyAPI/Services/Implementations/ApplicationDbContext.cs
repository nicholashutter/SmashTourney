namespace Services;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public override DbSet<ApplicationUser> Users { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Character> Characters { get; set; } = null!;

    public string DbPath { get; }

    //DbContextOptions is needed for the custom TUser example we working from
    //Currently DbContext options does nothing as OnConfiguring overrides it with its own optionsBuilder
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, string dbName) : base(options)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, dbName);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }

}

