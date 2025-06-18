using Discord;
using Discord.Interactions;

using MainBot.Database;
using MainBot.Utilities.Attributes;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;

namespace MainBot.Commands.SlashCommands.UserCommands;

[RequireModerator]
public class ManageRoleCommand(DatabaseContext database) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly DatabaseContext _database = database;

    [SlashCommand("role", "Add/remove a role from a user.")]
    public async Task ExecuteCommand(IUser user, IRole role)
    {
        Database.Models.Guild? guildEntry = await _database.Guilds.FirstOrDefaultAsync(x => x.id == Context.Guild.Id);
        if (DiscordExtensions.IsCommandExecutorPermsHigher(Context.User, user, guildEntry) is false)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Please check your permissions then try again.", deleteTimer: 60, invisible: true);
            return;
        }
        //role above user?
        int highestRole = 0;
        foreach (Discord.WebSocket.SocketRole? meRole in Context.Guild.CurrentUser.Roles)
        {
            if (meRole.Position > highestRole)
            {
                highestRole = meRole.Position;
            }
        }
        if (highestRole <= role.Position)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Please check your permissions then try again.", deleteTimer: 60, invisible: true);
            return;
        }
        Discord.Rest.RestGuildUser? guildUser = await Context.Client.Rest.GetGuildUserAsync(Context.Guild.Id, user.Id);
        if (guildUser is null)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "The user mentioned cannot be found, please try again", deleteTimer: 60, invisible: true);
            return;
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
        if (guildUser.RoleIds.Contains(role.Id))
        {
            //remove da role
            await guildUser.RemoveRoleAsync(role.Id);
            _ = await Context.ReplyWithEmbedAsync("Role Management", $"Successfully removed {role.Mention} role from {user.Mention}", deleteTimer: 120);
            if (logChannel is not null)
            {
                _ = await logChannel.SendEmbedAsync("Removed User Role", $"User: {user.Username} - {user.Mention}\nRole: {role.Mention}\nTaken By: {Context.Interaction.User.Mention}", $"{user.Id}", user.GetAvatarUrl());
            }

            return;
        }
        //add da role
        await guildUser.AddRoleAsync(role);
        _ = await Context.ReplyWithEmbedAsync("Role Management", $"Successfully added {role.Mention} role to {user.Mention}", deleteTimer: 120);
        if (logChannel is not null)
        {
            _ = await logChannel.SendEmbedAsync("Gave User Role", $"User: {user.Username} - {user.Mention}\nRole: {role.Mention}\nGiven By: {Context.Interaction.User.Mention}", $"{user.Id}", user.GetAvatarUrl());
        }
    }
}
