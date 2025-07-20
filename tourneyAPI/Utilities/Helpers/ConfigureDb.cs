namespace Helpers;

using Services; 
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Data.Sqlite;

public static class ConfigureDb
{
    public static void ConfigureDatabase(WebApplicationBuilder builder)
    {
        bool useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

        if (useInMemory)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            builder.Services.AddSingleton(connection);

            builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var conn = sp.GetRequiredService<SqliteConnection>();
                options.UseSqlite(conn);
            });
        }
        else
        {
            string dbPath = ConfigureDb.SetupProd();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(dbPath));
        }
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