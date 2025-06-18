using Discord;
using Discord.Interactions;

using MainBot.Database;
using MainBot.Utilities.Attributes;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;

namespace MainBot.Commands.SlashCommands.UserCommands;

[RequireModerator]
public class BanCommand(DatabaseContext database) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly DatabaseContext _database = database;

    [SlashCommand("ban", "Ban a user from the guild.")]
    public async Task ExecuteCommand(IUser? user, ulong? userId, string? reason = null, int pruneDays = 7)
    {
        if (user is null && userId is null)
        {
            _ = Context.ReplyWithEmbedAsync("Error Occurred", "Please specify a user and try again.", deleteTimer: 60, invisible: true).ConfigureAwait(false);
            return;
        }

        Database.Models.Guild? guildEntry = await _database.Guilds.FirstOrDefaultAsync(x => x.id == Context.Guild.Id);
        if (user is not null)
        {
            if (DiscordExtensions.IsCommandExecutorPermsHigher(Context.User, user, guildEntry) is false)
            {
                _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Please check your permissions then try again.", deleteTimer: 60, invisible: true);
                return;
            }
            await Context.Guild.GetUser(user.Id).BanAsync(pruneDays, reason);
            _ = await Context.ReplyWithEmbedAsync("Ban", $"Beamed {user.Mention} lawl", deleteTimer: 240).ConfigureAwait(false);

        }
        if (user is null && userId is not null)
        {
            await Context.Guild.AddBanAsync((ulong)userId, pruneDays, reason);
            _ = await Context.ReplyWithEmbedAsync("Ban", $"Beamed {userId} lawl", deleteTimer: 240).ConfigureAwait(false);
        }
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
            if (user is null)
            {
                _ = await logChannel.SendEmbedAsync("Banned User", $"User: {userId} - <@{userId}>\nReason: {(string.IsNullOrWhiteSpace(reason) ? "N/A" : reason)}\nBanned By: {Context.Interaction.User.Mention}", $"{userId}").ConfigureAwait(false);
                return;
            }
            _ = await logChannel.SendEmbedAsync("Banned User", $"User: {user.Username} - {user.Mention}\nReason: {(string.IsNullOrWhiteSpace(reason) ? "N/A" : reason)}\nBanned By: {Context.Interaction.User.Mention}", $"{user.Id}", user.GetAvatarUrl()).ConfigureAwait(false);
        }
    }
}
