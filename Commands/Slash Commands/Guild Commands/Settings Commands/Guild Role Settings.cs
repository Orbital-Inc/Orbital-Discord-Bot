using Discord;
using Discord.Interactions;

using MainBot.Database;
using MainBot.Services;
using MainBot.Utilities.Attributes;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;

namespace MainBot.Commands.SlashCommands.GuildCommands.SettingsCommands;

[RequireAdministrator]
public class GuildRoleSettingsCommand(RainbowRoleService rainbowRole, DatabaseContext database) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly RainbowRoleService _roleService = rainbowRole;
    private readonly DatabaseContext _database = database;
    public enum guildRoleOption
    {
        set_mute_role,
        set_verify_role,
        set_rainbow_role,
        set_hidden_role,
        set_administrator_role,
        set_moderator_role
    }

    [SlashCommand("guild-role-settings", "Guild settings that involve setting a role.")]
    public async Task ExecuteCommand(guildRoleOption roleOption, IRole role)
    {
        var rainbowRole = _roleService;
        Database.Models.Guild? guildEntry = await _database.Guilds.FirstOrDefaultAsync(x => x.id == Context.Guild.Id);
        if (guildEntry is null)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "This requires the guild to be backed up.", deleteTimer: 60, invisible: true);
            return;
        }
        switch (roleOption)
        {
            case guildRoleOption.set_mute_role:
                guildEntry.guildSettings.muteRoleId = role.Id;
                break;
            case guildRoleOption.set_verify_role:
                guildEntry.guildSettings.verifyRoleId = role.Id;
                break;
            case guildRoleOption.set_rainbow_role:
                Discord.Rest.RestApplication? application1 = await Context.Client.GetApplicationInfoAsync();
                Console.WriteLine(application1.Owner.Id);
                if (application1.Owner.Id == Context.User.Id)
                {
                    guildEntry.guildSettings.rainbowRoleId = role.Id;
                    rainbowRole._rainbowRoleGuilds.Add(new Models.RainbowRoleModel
                    {
                        roleId = role.Id,
                        guildId = Context.Guild.Id,
                    });
                    break;
                }
                _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Please check your permissions then try again.", deleteTimer: 60, invisible: true);
                return;
            case guildRoleOption.set_moderator_role:
                guildEntry.guildSettings.moderatorRoleId = role.Id;
                break;
            case guildRoleOption.set_administrator_role:
                Discord.Rest.RestApplication? application2 = await Context.Client.GetApplicationInfoAsync();
                if (Context.Guild.OwnerId == Context.User.Id || Context.User.Id == application2.Owner.Id)
                {
                    guildEntry.guildSettings.administratorRoleId = role.Id;
                    break;
                }
                _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Please check your permissions then try again.", deleteTimer: 60, invisible: true);
                return;
            case guildRoleOption.set_hidden_role:
                guildEntry.guildSettings.hiddenRoleId = role.Id;
                break;
            default:
                _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Invalid option selected.", deleteTimer: 60, invisible: true);
                return;
        }
        await _database.ApplyChangesAsync(guildEntry);
        if (roleOption == guildRoleOption.set_mute_role)
        {
            await Context.Interaction.DeferAsync();
            await Context.Guild.UpdateGuildChannelsForMute(guildEntry);
        }
        _ = await Context.ReplyWithEmbedAsync("Guild Role Settings", $"Successfully set the role to: {role.Mention}", deleteTimer: 60, invisible: true);
    }
}
