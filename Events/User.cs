using Discord;
using Discord.Rest;
using Discord.WebSocket;

using MainBot.Database;
using MainBot.Utilities;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MainBot.Events;

public class UserEventHandler
{
    private readonly DiscordShardedClient _client;
    private readonly IConfiguration _configuration;

    public UserEventHandler(DiscordShardedClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
        _client.UserJoined += UserJoinedGuild;
        _client.UserLeft += UserLeftGuild;
        _client.GuildMemberUpdated += UserUpdated;
        _client.UserBanned += UserGotBannedFromGuild;
        _client.UserUnbanned += UserUnbannedFromGuild;
    }

    private Task UserUnbannedFromGuild(SocketUser socketUser, SocketGuild socketGuild) => AuditLogHistory(_client, socketUser, socketGuild, ActionType.Unban);

    private Task UserGotBannedFromGuild(SocketUser socketUser, SocketGuild socketGuild) => AuditLogHistory(_client, socketUser, socketGuild, ActionType.Ban);

    private async Task UserUpdated(Cacheable<SocketGuildUser, ulong> arg1, SocketGuildUser GuildUserAfter)
    {
        SocketGuildUser? GuildUserBefore = await arg1.GetOrDownloadAsync();
        if (GuildUserBefore is not null && GuildUserAfter is not null)
        {
            if (string.IsNullOrWhiteSpace(GuildUserAfter.Nickname) is false)
            {
                if (GuildUserAfter.Nickname != GuildUserBefore.Nickname)
                {
                    await ChangeUsersName(GuildUserAfter, GuildUserAfter.Nickname);
                    return;
                }
            }
            if (GuildUserBefore.Username != GuildUserAfter.Username)
            {
                await ChangeUsersName(GuildUserAfter, GuildUserAfter.Username);
                return;
            }
        }
    }

    private async Task UserLeftGuild(SocketGuild socketGuild, SocketUser socketUser)
    {
        try
        {
            //get audit log
            var connectionString = _configuration;

            var auditLogs = await socketGuild.GetAuditLogsAsync(1).FlattenAsync();
            var log = auditLogs.FirstOrDefault();
            if (log is not null)
            {
                switch (log.Action)
                {
                    case ActionType.Ban:
                    case ActionType.Kick:
                        await AuditLogHistory(_client, socketUser, socketGuild, log.Action);
                        return;
                }
            }

            await using var database = new DatabaseContext(connectionString);
            Database.Models.Guild? guildEntry = await database.Guilds.FirstOrDefaultAsync(x => x.id == socketGuild.Id);
            if (guildEntry is null)
            {
                return;
            }

            if (guildEntry.guildSettings.userLogChannelId is null)
            {
                return;
            }

            var channel = _client.GetChannel((ulong)guildEntry.guildSettings.userLogChannelId) as SocketGuildChannel;
            if (channel is not null)
            {
                _ = await channel.SendEmbedAsync("User Left", $"User: {socketUser.Username} - {socketUser.Mention}", $"{socketUser.Id}", socketUser.GetAvatarUrl());
            }
        }
        catch (Exception e)
        {
            await using var database = new DatabaseContext(_configuration);
            await e.LogErrorAsync(database);
        }
    }

    private async Task UserJoinedGuild(SocketGuildUser arg)
    {
        try
        {
            _ = Task.Run(async () => await AutoKick(arg));
            //anti raid features
            await Task.WhenAll(SendUserJoinEmbed(arg), ChangeUsersName(arg, arg.Username), PersistentMute(arg));
        }
        catch (Exception e)
        {
            await using var database = new DatabaseContext(_configuration);
            await e.LogErrorAsync(database);
        }
    }

    private async Task AutoKick(SocketGuildUser user)
    {
        if (user.GuildPermissions.Administrator || user.IsBot || user.GuildPermissions.BanMembers || (user.PremiumSince is not null))
        {
            return;
        }

        var connectionString = _configuration;
        await using var database = new DatabaseContext(connectionString);
        Database.Models.Guild? guild = await database.Guilds.FirstOrDefaultAsync(x => x.id == user.Guild.Id);
        if (guild is null)
        {
            return;
        }

        if (guild.guildSettings.verifyRoleId is null or 0)
        {
            return;
        }

        if (user.Roles.FirstOrDefault(x => x.Id == guild.guildSettings.administratorRoleId) is not null)
        {
            return;
        }

        if (user.Roles.FirstOrDefault(x => x.Id == guild.guildSettings.moderatorRoleId) is not null)
        {
            return;
        }

        if (user.Roles.FirstOrDefault(x => x.Id == guild.guildSettings.verifyRoleId) is not null)
        {
            return;
        }

        await Task.Delay(TimeSpan.FromMinutes(10));
        SocketGuild? socketGuild = _client.GetGuild(user.Guild.Id);
        var newUser = socketGuild.GetUser(user.Id);
        if (newUser.Roles.FirstOrDefault(x => x.Id == guild.guildSettings.verifyRoleId) is not null)
        {
            return;
        }

        if (newUser.GuildPermissions.Administrator || newUser.IsBot || newUser.GuildPermissions.BanMembers || (newUser.PremiumSince is not null))
        {
            return;
        }

        await user.KickAsync();
    }

    private async Task PersistentMute(SocketGuildUser user)
    {
        await using var database = new DatabaseContext(_configuration);
        Database.Models.MuteUser? userEntry = await database.MutedUsers.FirstOrDefaultAsync(x => x.id == user.Id);
        if (userEntry is not null)
        {
            await user.AddRoleAsync(userEntry.muteRoleId);
        }
    }

    private async Task SendUserJoinEmbed(SocketGuildUser user)
    {
        var connectionString = _configuration;
        await using var database = new DatabaseContext(connectionString);
        Database.Models.Guild? guildEntry = await database.Guilds.FirstOrDefaultAsync(x => x.id == user.Guild.Id);
        if (guildEntry is null)
        {
            return;
        }

        if (guildEntry.guildSettings.userLogChannelId is null)
        {
            return;
        }

        var channel = _client.GetChannel((ulong)guildEntry.guildSettings.userLogChannelId) as SocketGuildChannel;
        if (channel is not null)
        {
            _ = await channel.SendEmbedAsync("User Joined", $"User: {user.Username} - {user.Mention}", $"{user.Id}", user.GetAvatarUrl());
        }
    }

    private async Task ChangeUsersName(SocketGuildUser user, string name)
    {
        try
        {
            if (name.ContainsSpecialCharacters())
            {
                string unCanceredName = name.RemoveSpecialCharacters();
                if (string.IsNullOrWhiteSpace(unCanceredName))
                {
                    string[] NewNicknames = ["Sunshine And Rainbows", "Hello World", "Just Another", "Billy", "Tyrone", "Bob", "My Nick Was Gay", "Boost For Nickname Change", "Me Over Here", "Tim", "Jimmy", "Quacha", "Freddy", "LoKo", "YeErT dErP dErPpY"];
                    unCanceredName = NewNicknames[new Random().Next(NewNicknames.Length)];
                }
                await user.ModifyAsync(x => x.Nickname = unCanceredName);

                Embed? RichEmbed = new EmbedBuilder()
                .WithTitle("Nickname Status")
                .WithAuthor("Orbital, Inc.", "https://orbitalsolutions.ca/assets/img/logo.png", "https://orbitalsolutions.ca")
                .WithDescription($"Hello {user.Username}, your nickname in our server has just been set to {unCanceredName} as your username/nickname violates our username/nickname guidelines.")
                .WithColor(Miscallenous.RandomDiscordColour())
                .WithCurrentTimestamp()
                .WithFooter("Enjoy your stay", "https://orbitalsolutions.ca/assets/img/logo.png")
                .Build();
                try { _ = await user.SendMessageAsync(embed: RichEmbed); } catch { }
            }
        }
        catch (Exception e)
        {
            await using var database = new DatabaseContext(_configuration);
            await e.LogErrorAsync(database);
        }
    }

    private async Task AuditLogHistory(DiscordShardedClient client, SocketUser socketUser, SocketGuild socketGuild, ActionType actionType)
    {
        try
        {
            string title = string.Empty;
            string action = string.Empty;
            SocketChannel? logChannel = null;
            IAuditLogEntry? auditLog = null;
            await using var database = new DatabaseContext(_configuration);
            Database.Models.Guild? guildEntry = await database.Guilds.FirstOrDefaultAsync(x => x.id == socketGuild.Id);
            if (guildEntry is null)
            {
                return;
            }
            //
            switch (actionType)
            {
                case ActionType.Unban:
                case ActionType.Kick:
                case ActionType.Ban:
                    if (guildEntry.guildSettings.userLogChannelId is null)
                    {
                        return;
                    }

                    break;
            }
            //
            var auditLogs = await socketGuild.GetAuditLogsAsync(1, actionType: actionType).FlattenAsync();
            if (auditLogs is null)
            {
                return;
            }

            auditLog = auditLogs.FirstOrDefault();
            if (auditLog is null)
            {
                return;
            }

            if (auditLog.User.Id == client.CurrentUser.Id)
            {
                return;
            }
            //
            switch (actionType)
            {
                case ActionType.Ban:
                    if (auditLog?.Data is not BanAuditLogData banLogData)
                    {
                        return;
                    }

                    if (banLogData.Target.Id != socketUser.Id)
                    {
                        return;
                    }

                    title = "User Banned";
                    action = "Banned By:";
                    break;
                case ActionType.Unban:
                    if (auditLog?.Data is not UnbanAuditLogData unbanLogData)
                    {
                        return;
                    }

                    if (unbanLogData.Target.Id != socketUser.Id)
                    {
                        return;
                    }

                    title = "User Unbanned";
                    action = "Unbanned By:";
                    break;
                case ActionType.Kick:
                    if (auditLog?.Data is not KickAuditLogData kickLogData)
                    {
                        return;
                    }

                    if (kickLogData.Target.Id != socketUser.Id)
                    {
                        return;
                    }

                    title = "User Kicked";
                    action = "Kicked By:";
                    break;
            }
            //
            switch (actionType)
            {
                case ActionType.Unban:
                case ActionType.Kick:
                case ActionType.Ban:
                    if (guildEntry.guildSettings.userLogChannelId is null)
                    {
                        return;
                    }
                    logChannel = client.GetChannel((ulong)guildEntry.guildSettings.userLogChannelId) as SocketGuildChannel;
                    if (logChannel is not null)
                    {
                        _ = await logChannel.SendEmbedAsync(title, $"User: {socketUser.Username} - {socketUser.Mention}\n{(auditLog is null ? "" : $"{action} {auditLog.User.Mention}{(auditLog.Reason is null ? "" : $"\nReason: {auditLog.Reason}")}")}", $"{socketUser.Id}", socketUser.GetAvatarUrl());
                    }

                    break;
            }
        }
        catch (Exception e)
        {
            await using var database = new DatabaseContext(_configuration);
            await e.LogErrorAsync(database);
        }
    }
}
