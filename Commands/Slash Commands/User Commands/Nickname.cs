using Discord;
using Discord.Interactions;

using MainBot.Database;
using MainBot.Utilities.Attributes;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;

namespace MainBot.Commands.SlashCommands.UserCommands;

[RequireModerator]
public class NicknameCommand(DatabaseContext database) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly DatabaseContext _database = database;

    [SlashCommand("nickname", "Change a user's nickname or reset it.")]
    public async Task ExecuteCommand(IUser user, string? nickname = null)
    {
        Database.Models.Guild? guildEntry = await _database.Guilds.FirstOrDefaultAsync(x => x.id == Context.Guild.Id);
        if (DiscordExtensions.IsCommandExecutorPermsHigher(Context.User, user, guildEntry) is false)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Please check your permissions then try again.", deleteTimer: 60, invisible: true);
            return;
        }
        if (string.IsNullOrWhiteSpace(nickname))
        {
            await Context.Guild.GetUser(user.Id).ModifyAsync(x => x.Nickname = null);
            _ = await Context.ReplyWithEmbedAsync("Nickname", $"Successfully removed {user.Mention}'s nickname.", deleteTimer: 120);
            return;
        }
        await Context.Guild.GetUser(user.Id).ModifyAsync(x => x.Nickname = nickname);
        _ = await Context.ReplyWithEmbedAsync("Nickname", $"Successfully set {user.Mention}'s nickname to {nickname}.", deleteTimer: 120);
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
            _ = await logChannel.SendEmbedAsync("Changed User Nickname", $"User: {user.Username} - {user.Mention}\nChanged By: {Context.Interaction.User.Mention}", $"{user.Id}", user.GetAvatarUrl());
        }
    }
}
