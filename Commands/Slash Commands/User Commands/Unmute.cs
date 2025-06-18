using Discord;
using Discord.Interactions;

using MainBot.Database;
using MainBot.Utilities.Attributes;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;

namespace MainBot.Commands.SlashCommands.UserCommands;

[RequireModerator]
public class UnmuteCommand(DatabaseContext database) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly DatabaseContext _database = database;

    [SlashCommand("unmute", "Unmute a user.")]
    public async Task UnmuteUserCommand(IUser user)
    {
        Database.Models.Guild? guildEntry = await _database.Guilds.FirstOrDefaultAsync(x => x.id == Context.Guild.Id);
        if (guildEntry is null)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "This requires the guild to be backed up.", deleteTimer: 60, invisible: true);
            return;
        }
        Database.Models.MuteUser? mutedUserEntry = await _database.MutedUsers.FirstOrDefaultAsync(x => x.id == user.Id && x.guildId == Context.Guild.Id);
        if (mutedUserEntry is null)
        {
            _ = await Context.ReplyWithEmbedAsync("Unmute User", $"User is not muted.", deleteTimer: 60, invisible: true);
            return;
        }
        if (guildEntry.guildSettings.muteRoleId is null)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Role doesn't exist.", deleteTimer: 60, invisible: true);
            return;
        }
        if (DiscordExtensions.IsCommandExecutorPermsHigher(Context.User, user, guildEntry) is false)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Please check your permissions then try again.", deleteTimer: 60, invisible: true);
            return;
        }
        Discord.WebSocket.SocketRole? role = Context.Guild.GetRole((ulong)guildEntry.guildSettings.muteRoleId);
        if (role is null)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Role doesn't exist.", deleteTimer: 60, invisible: true);
            return;
        }
        _database.Remove(mutedUserEntry);
        await _database.ApplyChangesAsync();
        //remove mute role on user
        await Context.Guild.GetUser(user.Id).RemoveRoleAsync(role);
        _ = await Context.ReplyWithEmbedAsync("Unmute", $"Successfully unmuted {user.Mention}");
        if (guildEntry.guildSettings.userLogChannelId is null)
        {
            return;
        }

        var logChannel = Context.Guild.GetChannel((ulong)guildEntry.guildSettings.userLogChannelId);
        if (logChannel is not null)
        {
            _ = await logChannel.SendEmbedAsync("Unmuted User", $"User: {user.Username} - {user.Mention}\nUnmuted By: {Context.Interaction.User.Mention}", $"{user.Id}", user.GetAvatarUrl());
        }
    }
}
