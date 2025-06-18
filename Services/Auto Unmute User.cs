using Discord.WebSocket;

using MainBot.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MainBot.Services;

public class AutoUnmuteUserService(DiscordShardedClient client, IConfiguration configuration) : BackgroundService
{
    private readonly DiscordShardedClient _client = client;
    private readonly IConfiguration _configuration = configuration;

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _ = Task.Factory.StartNew(async () => await AutoUnmuteUsersAsync(cancellationToken), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        return Task.CompletedTask;
    }

    private async Task AutoUnmuteUsersAsync(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested is false)
        {
            try
            {
                var connectionString = _configuration;
                await using var database = new DatabaseContext(connectionString);
                if (await database.MutedUsers.AnyAsync(cancellationToken: cancellationToken))
                {
                    List<Database.Models.MuteUser>? mutedUsers = await database.MutedUsers.ToListAsync(cancellationToken: cancellationToken);
                    foreach (Database.Models.MuteUser? user in mutedUsers)
                    {
                        if (user.muteExpiryDate <= DateTime.UtcNow)
                        {
                            SocketGuild? guild = _client.GetGuild(user.guildId);
                            if (guild is not null)
                            {
                                SocketGuildUser? userSocket = guild.GetUser(user.id);
                                if (userSocket is not null)
                                {
                                    await userSocket.RemoveRoleAsync(user.muteRoleId);
                                    database.Remove(user);
                                    await database.ApplyChangesAsync();
                                }
                            }
                        }
                    };
                }
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (Exception ex)
            {
                await using var database = new DatabaseContext(_configuration);
                await ex.LogErrorAsync(database);
            }
        }
    }
}
