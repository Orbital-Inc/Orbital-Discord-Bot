using Discord.WebSocket;

using MainBot.Database;
using MainBot.Database.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MainBot.Services;

public class CustomService
{
    private readonly DiscordShardedClient _client;
    private readonly RainbowRoleService _roleService;
    private readonly IConfiguration _configuration;
    public CustomService(DiscordShardedClient client, RainbowRoleService rainbowRoleService, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
        _roleService = rainbowRoleService;
        _client.ShardReady += ShardReady;
    }

    private async Task ShardReady(DiscordSocketClient arg)
    {
        var rainbowRole = _roleService;
        await _client.SetStatusAsync(Discord.UserStatus.Idle);
        await _client.SetGameAsync("orbitalsolutions.ca", null, Discord.ActivityType.Watching);
        if (rainbowRole._rainbowRoleGuilds.IsEmpty)
        {
            var connectionString = _configuration;
            await using var database = new DatabaseContext(connectionString);
            List<Guild>? guilds = await database.Guilds.ToListAsync();
            await Task.WhenAll(RainbowShit(guilds));
        }
    }

    private Task RainbowShit(List<Guild> guilds)
    {
        var rainbowService = _roleService;
        foreach (Guild? guild in guilds)
        {
            if (guild.guildSettings.rainbowRoleId is not null)
            {
                rainbowService._rainbowRoleGuilds.Add(new Models.RainbowRoleModel
                {
                    roleId = (ulong)guild.guildSettings.rainbowRoleId,
                    guildId = guild.id,
                    uglyColours = guild.guildSettings.uglyColours
                });
            }
        }
        return Task.CompletedTask;
    }
}
