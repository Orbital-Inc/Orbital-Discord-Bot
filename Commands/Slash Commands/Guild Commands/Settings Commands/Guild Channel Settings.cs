using Discord;
using Discord.Interactions;

using MainBot.Database;
using MainBot.Utilities.Attributes;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;

using System.Threading.Channels;

namespace MainBot.Commands.SlashCommands.GuildCommands.SettingsCommands;

[RequireAdministrator]
public class GuildChannelSettingsCommand(DatabaseContext database) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly DatabaseContext _database = database;

    public enum guildChannelOption
    {
        //AddDailyNukeChannel,
        //RemoveDailyNukeChannel,
        //SetUserLogChannel,
        //RemoveUserLogChannel,
        //SetMessageLogChannel,
        //RemoveMessageLogChannel,
        //SetSystemLogChannel,
        //RemoveSystemLogChannel,
        //SetCommandLogChannel,
        //RemoveCommandLogChannel,
        //SetTicketCategory,
        //RemoveTicketCategory
        add_daily_nuke_channel,
        remove_daily_nuke_channel,
        set_user_log_channel,
        remove_user_log_channel,
        set_message_log_channel,
        remove_message_log_channel,
        set_system_log_channel,
        remove_system_log_channel,
        set_command_log_channel,
        remove_command_log_channel,
        set_ticket_category,
        remove_ticket_category
    }

    [SlashCommand("guild-channel-settings", "Guild settings that involve setting a channel.")]
    //public async Task ExecuteCommand()
    public async Task ExecuteCommand(guildChannelOption channelOption, IChannel? channel = null)
    {
        var categoryChannel = channel as ICategoryChannel;
        var textChannel = channel as ITextChannel;
        if (categoryChannel is null && textChannel is null)
        {
            throw new ArgumentNullException(nameof(channel), "This channel is not a valid to perform action on channel.");
        }
        //await DeferAsync(true);
        Database.Models.Guild? guildEntry = await _database.Guilds.FirstOrDefaultAsync(x => x.id == Context.Guild.Id);
        if (guildEntry is null)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "This requires the guild to be backed up.", deleteTimer: 60, invisible: true);
            return;
        }

        //var menuBuilder = new SelectMenuBuilder()
        //    .WithPlaceholder("Select an option")
        //    .WithCustomId("guild-channel-settings")
        //    .WithMinValues(1)
        //    .WithMaxValues(1)
        //    .AddOption(guildChannelOption.AddDailyNukeChannel.FormatEnum(), guildChannelOption.AddDailyNukeChannel.ToString(), "Designate a channel for daily automated content removal.")
        //    .AddOption(guildChannelOption.RemoveDailyNukeChannel.FormatEnum(), guildChannelOption.RemoveDailyNukeChannel.ToString(), "Remove the channel from daily automated content removal.")
        //    .AddOption(guildChannelOption.SetUserLogChannel.FormatEnum(), guildChannelOption.SetUserLogChannel.ToString(), "Specify a channel to log all user activity.")
        //    .AddOption(guildChannelOption.RemoveUserLogChannel.FormatEnum(), guildChannelOption.RemoveUserLogChannel.ToString(), "Stop logging user activity in the specified channel.")
        //    .AddOption(guildChannelOption.SetMessageLogChannel.FormatEnum(), guildChannelOption.SetMessageLogChannel.ToString(), "Assign a channel to log all server messages.")
        //    .AddOption(guildChannelOption.RemoveMessageLogChannel.FormatEnum(), guildChannelOption.RemoveMessageLogChannel.ToString(), "Stop logging messages in the specified channel.")
        //    .AddOption(guildChannelOption.SetSystemLogChannel.FormatEnum(), guildChannelOption.SetSystemLogChannel.ToString(), "Designate a channel for system-related event logging.")
        //    .AddOption(guildChannelOption.RemoveSystemLogChannel.FormatEnum(), guildChannelOption.RemoveSystemLogChannel.ToString(), "Stop logging system events in the specified channel.")
        //    .AddOption(guildChannelOption.SetCommandLogChannel.FormatEnum(), guildChannelOption.SetCommandLogChannel.ToString(), "Set a channel to log all command executions.")
        //    .AddOption(guildChannelOption.RemoveCommandLogChannel.FormatEnum(), guildChannelOption.RemoveCommandLogChannel.ToString(), "Stop logging command executions in the channel.")
        //    .AddOption(guildChannelOption.SetTicketCategory.FormatEnum(), guildChannelOption.SetTicketCategory.ToString(), "Establish a category for organizing support tickets.")
        //    .AddOption(guildChannelOption.RemoveTicketCategory.FormatEnum(), guildChannelOption.RemoveTicketCategory.ToString(), "Remove the category for support ticket management.");

        //var builder = new ComponentBuilder()
        //    .WithSelectMenu(menuBuilder);
        //var reply = await Context.Interaction.ModifyOriginalResponseAsync(x =>
        //{
        //    x.Components = builder.Build();
        //    x.Content = "Guild Channel Settings";
        //});
        //_ = Task.Factory.StartNew(async () =>
        //{
        //    await Task.Delay(TimeSpan.FromMinutes(1));
        //    await Context.Interaction.DeleteOriginalResponseAsync();
        //});

        #region old
        switch (channelOption)
        {
            case guildChannelOption.remove_command_log_channel:
                guildEntry.guildSettings.commandLogChannelId = null;
                await _database.ApplyChangesAsync(guildEntry);
                break;
            case guildChannelOption.remove_message_log_channel:
                guildEntry.guildSettings.messageLogChannelId = null;
                await _database.ApplyChangesAsync(guildEntry);
                break;
            case guildChannelOption.remove_ticket_category:
                guildEntry.guildSettings.ticketCategoryId = null;
                await _database.ApplyChangesAsync(guildEntry);
                break;
            case guildChannelOption.remove_user_log_channel:
                guildEntry.guildSettings.userLogChannelId = null;
                await _database.ApplyChangesAsync(guildEntry);
                break;
            case guildChannelOption.remove_system_log_channel:
                guildEntry.guildSettings.systemLogChannelId = null;
                await _database.ApplyChangesAsync(guildEntry);
                break;
            case guildChannelOption.set_message_log_channel:
                if (textChannel is null)
                {
                    throw new ArgumentNullException(nameof(textChannel), "This channel is not a text channel.");
                }

                guildEntry.guildSettings.messageLogChannelId = textChannel.Id;
                await _database.ApplyChangesAsync(guildEntry);
                break;
            case guildChannelOption.set_user_log_channel:
                if (textChannel is null)
                {
                    throw new ArgumentNullException(nameof(textChannel), "This channel is not a text channel.");
                }

                guildEntry.guildSettings.userLogChannelId = textChannel.Id;
                await _database.ApplyChangesAsync(guildEntry);
                break;
            case guildChannelOption.add_daily_nuke_channel:
                if (textChannel is null)
                {
                    throw new ArgumentNullException(nameof(textChannel), "This channel is not a text channel.");
                }

                await AddChannelToNukeListCommand(textChannel, _database, Context);
                return;
            case guildChannelOption.remove_daily_nuke_channel:
                if (textChannel is null)
                {
                    throw new ArgumentNullException(nameof(textChannel), "This channel is not a text channel.");
                }

                await RemoveChannelFromNukeListCommand(textChannel, _database, Context);
                return;
            case guildChannelOption.set_system_log_channel:
                if (textChannel is null)
                {
                    throw new ArgumentNullException(nameof(textChannel), "This channel is not a text channel.");
                }

                guildEntry.guildSettings.systemLogChannelId = textChannel.Id;
                await _database.ApplyChangesAsync(guildEntry);
                break;
            case guildChannelOption.set_command_log_channel:
                if (textChannel is null)
                {
                    throw new ArgumentNullException(nameof(textChannel), "This channel is not a text channel.");
                }

                guildEntry.guildSettings.commandLogChannelId = textChannel.Id;
                await _database.ApplyChangesAsync(guildEntry);
                break;
            case guildChannelOption.set_ticket_category:
                if (categoryChannel is null)
                {
                    throw new ArgumentNullException(nameof(categoryChannel), "This channel is not a category channel.");
                }
                guildEntry.guildSettings.ticketCategoryId = categoryChannel.Id;
                await _database.ApplyChangesAsync(guildEntry);
                _ = await Context.ReplyWithEmbedAsync("Guild Channel Settings", $"Successfully set the category to: {categoryChannel.Name}", deleteTimer: 60, invisible: true);
                return;
            default:
                _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Invalid option selected.", deleteTimer: 60, invisible: true);
                return;
        }
        #endregion
        if (textChannel is not null)
            _ = await Context.ReplyWithEmbedAsync("Guild Channel Settings", $"Successfully set the channel to: {textChannel.Mention}", deleteTimer: 60, invisible: true);
    }

    private static async Task AddChannelToNukeListCommand(IChannel channel, DatabaseContext database, ShardedInteractionContext context)
    {
        if (channel is not ITextChannel textChannel)
        {
            throw new ArgumentNullException(nameof(textChannel), "Cannot nuke channel, this channel is not a text channel.");
        }

        Database.Models.DiscordChannel? nukeChannel = await database.NukeChannels.FirstOrDefaultAsync(x => x.id == textChannel.Id);
        if (nukeChannel is not null)
        {
            _ = await context.ReplyWithEmbedAsync("Daily Nuke Channels", $"Failed to add {textChannel.Mention} to the list of daily nuke channels, because it is already added.", deleteTimer: 60, invisible: true);
            return;
        }
        _ = await database.NukeChannels.AddAsync(new Database.Models.DiscordChannel
        {
            id = textChannel.Id,
            name = textChannel.Name,
            guildId = textChannel.GuildId,
        });
        await database.ApplyChangesAsync();
        _ = await context.ReplyWithEmbedAsync("Daily Nuke Channels", $"Successfully added {textChannel.Mention} to the list of daily nuke channels.", deleteTimer: 60, invisible: true);
    }

    private static async Task RemoveChannelFromNukeListCommand(IChannel channel, DatabaseContext database, ShardedInteractionContext context)
    {
        if (channel is not ITextChannel textChannel)
        {
            throw new ArgumentNullException(nameof(textChannel), "Cannot nuke channel, this channel is not a text channel.");
        }

        Database.Models.DiscordChannel? nukeChannel = await database.NukeChannels.FirstOrDefaultAsync(x => x.id == textChannel.Id);
        if (nukeChannel is null)
        {
            _ = await context.ReplyWithEmbedAsync("Daily Nuke Channels", $"{textChannel.Mention} channel is not in the list of daily nuke channels, try adding it.", deleteTimer: 60, invisible: true);
            return;
        }
        database.Remove(nukeChannel);
        await database.ApplyChangesAsync();
        _ = await context.ReplyWithEmbedAsync("Daily Nuke Channels", $"Successfully removed {textChannel.Mention} from the list of daily nuke channels.", deleteTimer: 60, invisible: true);
    }
}
