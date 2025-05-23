namespace Services;

using Microsoft.EntityFrameworkCore;
using DTO;
using Entities;

public class AppDBContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("");
    }

}

