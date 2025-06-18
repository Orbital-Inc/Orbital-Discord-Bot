using MainBot.Database.Models;
using MainBot.Database.Models.Logs;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MainBot.Database;

public class DatabaseContext(IConfiguration configuration) : DbContext
{
    private readonly IConfiguration _configuration = configuration;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PostgresConnectionString"), x => { }).UseLazyLoadingProxies();
    }
    //dbsets
    public DbSet<ErrorLog> Errors { get; set; }
    public DbSet<Guild> Guilds { get; set; }
    public DbSet<DiscordChannel> NukeChannels { get; set; }
    public DbSet<MuteUser> MutedUsers { get; set; }
}
