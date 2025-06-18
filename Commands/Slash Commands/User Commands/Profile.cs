using Discord;
using Discord.Interactions;

using MainBot.Database;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;

namespace MainBot.Commands.SlashCommands.UserCommands;

public class ProfileCommand(DatabaseContext database) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly DatabaseContext _database = database;

    [SlashCommand("profile", "Display details about your account.")]
    public async Task ViewProfile(IUser? user = null)
    {
        Discord.WebSocket.SocketGuildUser? userInfo = user is null ? Context.Guild.GetUser(Context.User.Id) : Context.Guild.GetUser(user.Id);
        string userRoles = "Roles: ";
        foreach (Discord.WebSocket.SocketRole? role in userInfo.Roles)
        {
            userRoles += $"{role.Mention},";
        }

        if (userRoles.Last() is ',')
        {
            userRoles = userRoles[0..^1];
        }

        _ = await Context.ReplyWithEmbedAsync($"{userInfo.Username}'s Information", $"{userInfo.Mention} | {userInfo.Id}\n" +
            $"Creation Date: <t:{userInfo.CreatedAt.ToUnixTimeSeconds()}>\n" +
            $"Join Date: {(userInfo.JoinedAt is null ? "N/A" : $"<t:{userInfo.JoinedAt.Value.ToUnixTimeSeconds()}>")}\n" +
            $"Boost Date: {(userInfo.PremiumSince is null ? "N/A" : $"<t:{userInfo.PremiumSince.Value.ToUnixTimeSeconds()}>")}\n" +
            userRoles + "\n" +
            $"Mute Status: {_database.MutedUsers.FirstOrDefaultAsync(x => x.id == userInfo.Id && x.guildId == Context.Guild.Id) is null}", thumbnailUrl: userInfo.GetAvatarUrl(), imageUrl: userInfo.GetGuildAvatarUrl(), deleteTimer: 180);
    }
}
