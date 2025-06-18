using Discord;
using Discord.Interactions;

using MainBot.Database;
using MainBot.Utilities.Attributes;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;

namespace MainBot.Commands.SlashCommands.UserCommands;

[RequireModerator]
public class RemovePermissions(DatabaseContext database) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly DatabaseContext _database = database;

    [SlashCommand("remove-permissions", "Remove view permissions to a user in a specific channel.")]
    public async Task ExecuteCommand(IUser user, IChannel channel)
    {
        Database.Models.Guild? guildEntry = await _database.Guilds.FirstOrDefaultAsync(x => x.id == Context.Guild.Id);
        if (DiscordExtensions.IsCommandExecutorPermsHigher(Context.User, user, guildEntry) is false)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Please check your permissions then try again.", deleteTimer: 60, invisible: true);
            return;
        }
        var guildChannel = Context.Guild.GetChannel(channel.Id);
        await guildChannel.RemovePermissionOverwriteAsync(user);
        _ = await Context.ReplyWithEmbedAsync("View Permissions", $"Removed view permissions for {user.Mention} for <#{guildChannel.Id}>", deleteTimer: 240);
        if (guildEntry is null)
        {
            return;
        }

        if (guildEntry.guildSettings.userLogChannelId is null)
        {
            return;
        }

        var logChannel = Context.Guild.GetChannel((ulong)guildEntry.guildSettings.userLogChannelId);
        if (logChannel is not null)
        {
            _ = await logChannel.SendEmbedAsync("Removed View Permissions", $"User: {user.Username} - {user.Mention}\nRemoved By: {Context.Interaction.User.Mention}", $"{user.Id}", user.GetAvatarUrl());
        }
    }
}