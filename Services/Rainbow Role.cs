using System.Collections.Concurrent;

using Discord.WebSocket;

using MainBot.Database;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace MainBot.Services;

public class RainbowRoleService(DiscordShardedClient client, IConfiguration configuration) : BackgroundService
{
    private readonly DiscordShardedClient _client = client;
    internal ConcurrentBag<Models.RainbowRoleModel> _rainbowRoleGuilds = new();
    private readonly IConfiguration _configuration = configuration;

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Rainbow Task Started!");
        _ = Task.Factory.StartNew(async () => await RainbowRoleChanger(cancellationToken), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        return Task.CompletedTask;
    }

    private async Task RainbowRoleChanger(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested is false)
        {
            try
            {
                if (_rainbowRoleGuilds.Any() is false)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    continue;
                }
                foreach (Models.RainbowRoleModel? guild in _rainbowRoleGuilds)
                {
                    SocketGuild? guildSocket = _client.GetGuild(guild.guildId);
                    if (guildSocket is not null)
                    {
                        SocketRole? role = guildSocket.GetRole(guild.roleId);
                        if (role is not null)
                        {
                            await role.ModifyAsync(async (x) => x.Color = await Utilities.Miscallenous.RandomDiscordColourAsync(guild.uglyColours, _configuration.GetSection("General")["AI_Token"]));
                            Console.WriteLine("Changed Rainbow Colour");
                        }
                    }
                }
                int rand = new Random().Next(1, 20);
                await Task.Delay(TimeSpan.FromMinutes(rand), cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Message == "The server responded with error 50013: Missing Permissions")
                    return;

                await using var database = new DatabaseContext(_configuration);
                await ex.LogErrorAsync(database, "rainbow role service func");
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }
        }
    }
}
