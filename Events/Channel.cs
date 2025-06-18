using Discord;
using Discord.WebSocket;

using MainBot.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MainBot.Events;

internal class ChannelEventHandler
{
    private readonly DiscordShardedClient _client;
    private readonly IConfiguration _configuration;

    public ChannelEventHandler(DiscordShardedClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
        _client.ChannelCreated += ChannelCreated;
    }

    private async Task ChannelCreated(SocketChannel arg)
    {
        var connectionString = _configuration;
        await using var database = new DatabaseContext(connectionString);
        var text = arg as ITextChannel;
        if (text is not null)
        {
            Database.Models.Guild? guildEntry = await database.Guilds.FirstOrDefaultAsync(x => x.id == text.GuildId);
            if (guildEntry is null)
            {
                return;
            }

            if (guildEntry.guildSettings.muteRoleId is null)
            {
                return;
            }

            ICategoryChannel? category = await text.GetCategoryAsync();
            if (category is not null)
            {
                if (text.PermissionOverwrites == category.PermissionOverwrites)
                {
                    return;
                }
            }
            IRole? role = text.Guild.GetRole((ulong)guildEntry.guildSettings.muteRoleId);
            if (role is not null)
            {
                OverwritePermissions? perms = text.GetPermissionOverwrite(role);
                if (perms is null)
                {
                    await text.AddPermissionOverwriteAsync(role, Utilities.Miscallenous.MutePermsChannel());
                }
            }
            return;
        }
        var voice = arg as IVoiceChannel;
        if (voice is not null)
        {
            Database.Models.Guild? guildEntry = await database.Guilds.FirstOrDefaultAsync(x => x.id == voice.GuildId);
            if (guildEntry is null)
            {
                return;
            }

            if (guildEntry.guildSettings.muteRoleId is null)
            {
                return;
            }

            ICategoryChannel? category = await voice.GetCategoryAsync();
            if (category is not null)
            {
                if (voice.PermissionOverwrites == category.PermissionOverwrites)
                {
                    return;
                }
            }
            IRole? role = voice.Guild.GetRole((ulong)guildEntry.guildSettings.muteRoleId);
            if (role is not null)
            {
                await voice.AddPermissionOverwriteAsync(role, Utilities.Miscallenous.MutePermsChannel());
            }

            return;
        }
        var catgeorySocket = arg as ICategoryChannel;
        if (catgeorySocket is not null)
        {
            Database.Models.Guild? guildEntry = await database.Guilds.FirstOrDefaultAsync(x => x.id == catgeorySocket.GuildId);
            if (guildEntry is null)
            {
                return;
            }

            if (guildEntry.guildSettings.muteRoleId is null)
            {
                return;
            }

            IRole? role = catgeorySocket.Guild.GetRole((ulong)guildEntry.guildSettings.muteRoleId);
            if (role is not null)
            {
                await catgeorySocket.AddPermissionOverwriteAsync(role, Utilities.Miscallenous.MutePermsChannel());
            }

            return;
        }
    }
}
