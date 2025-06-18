using Discord;
using Discord.WebSocket;

using MainBot.Database;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;

namespace MainBot.Events;
internal class MenuEventHandler(DiscordShardedClient discord, DatabaseContext database)
{
    private readonly DiscordShardedClient _client = discord;
    private readonly DatabaseContext _database = database;

    public void Initialize()
    {
        _client.SelectMenuExecuted += ComponentCommandExecuted;
    }

    private async Task ComponentCommandExecuted(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "guild-channel-settings":
                await ExecuteGuildChannelSettingsCommand(component);
                break;
            case "AddDailyNukeChannel-settings":
                await ExecuteNukeChannelSettings(component, true);
                break;
            case "RemoveDailyNukeChannel-settings":
                await ExecuteNukeChannelSettings(component, false);
                break;
            default:
                await component.Channel.SendEmbedAsync("Error", component.Data.Value, "This is not a valid option.", deleteTimer: 10);
                break;
        }
    }

    private async Task ExecuteGuildChannelSettingsCommand(SocketMessageComponent component)
    {
        await component.DeferAsync(true);
        var menuId = component.Data.Values;
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Select an channel")
            .WithCustomId($"{menuId.FirstOrDefault()}-settings")
            .WithMinValues(1)
            .WithMaxValues(1)
            .WithType(ComponentType.ChannelSelect)
            .WithChannelTypes(ChannelType.Text);

        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder);
        var reply = await component.ModifyOriginalResponseAsync(x =>
        {
            x.Components = builder.Build();
            x.Content = "Guild Channel Settings";
        });
        _ = Task.Factory.StartNew(async () =>
        {
            await Task.Delay(TimeSpan.FromMinutes(1));
            await component.DeleteOriginalResponseAsync();
        });
    }

    private async Task ExecuteNukeChannelSettings(SocketMessageComponent component, bool addNukeChannel)
    {

        Database.Models.Guild? guildEntry = await _database.Guilds.FirstOrDefaultAsync(x => x.id == component.GuildId);
        if (guildEntry is null)
        {
            //_ = await component.Message.ReplyWithEmbedAsync("Error Occurred", "This requires the guild to be backed up.", deleteTimer: 60, invisible: true);
            return;
        }
    }
}
