using Discord;
using Discord.Interactions;
using Discord.Rest;

using MainBot.Database;
using MainBot.Utilities.Attributes;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;

namespace MainBot.Commands.SlashCommands.UserCommands;

[RequireModerator]
public class UnbanCommand(DatabaseContext database) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly DatabaseContext _database = database;

    [SlashCommand("unban", "Unban a user from the guild.")]
    public async Task ExecuteCommand(IUser? user = null, ulong? userId = null, string? username = null)
    {
        if (user is null && string.IsNullOrWhiteSpace(username) && userId is null)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Please specify a user and try again.", deleteTimer: 60, invisible: true);
            return;
        }
        RestUser? bannedUser = null;
        if (user is not null)
        {
            var possibleBannedUser = await Context.Guild.GetBanAsync(user);
            if (possibleBannedUser is null)
            {
                return;
            }
            bannedUser = possibleBannedUser.User;
        }
        else
        {
            var banList = await Context.Guild.GetBansAsync().FlattenAsync();
            if (banList is null) { return; }
            foreach (var ban in banList)
            {
                if (userId is not null)
                {
                    if (ban.User.Id == userId)
                    {
                        bannedUser = ban.User;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(username) is false)
                {
                    if (ban.User.Username == username)
                    {
                        bannedUser = ban.User;
                        break;
                    }
                }
            }
        }
        if (bannedUser is null)
        {

            return;
        }
        await Context.Guild.RemoveBanAsync(bannedUser);
        _ = await Context.ReplyWithEmbedAsync("Unbanned", $"Unbeamed {bannedUser.Mention} <a:es_bigeyes:1034240759525801994>", deleteTimer: 240);
        Database.Models.Guild? guildEntry = await _database.Guilds.FirstOrDefaultAsync(x => x.id == Context.Guild.Id);
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
            _ = await logChannel.SendEmbedAsync("Unbanned User", $"User: {bannedUser.Username} - {bannedUser.Mention}\nUnbanned By: {Context.Interaction.User.Mention}", $"{bannedUser.Id}", bannedUser.GetAvatarUrl());
        }
    }
}
