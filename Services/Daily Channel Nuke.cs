using Castle.Core.Configuration;

using Discord;
using Discord.WebSocket;

using MainBot.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MainBot.Services;

public class DailyChannelNukeService(DiscordShardedClient client, Microsoft.Extensions.Configuration.IConfiguration configuration) : BackgroundService
{
    private readonly DiscordShardedClient _client = client;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration = configuration;

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _ = Task.Factory.StartNew(async () => await AutoNukeChannels(cancellationToken), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        return Task.CompletedTask;
    }

    private async Task AutoNukeChannels(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested is false)
        {
            try
            {
                //check date
                DateTime today = DateTime.Now;
                DateTime midnight = DateTime.Today.AddDays(1);
                TimeSpan waitTime = midnight - today;
                Console.WriteLine(waitTime);
                await Task.Delay((int)Math.Round(waitTime.TotalMilliseconds, 0), cancellationToken);
                //start real work
                var connectionString = _configuration;
                await using var database = new DatabaseContext(connectionString);
                List<Database.Models.DiscordChannel>? freshList = await database.NukeChannels.ToListAsync(cancellationToken: cancellationToken);
                foreach (Database.Models.DiscordChannel? channel in freshList)
                {
                    SocketGuild? guild = _client.GetGuild(channel.guildId);
                    if (guild is null)
                    {
                        database.Remove(channel);
                        continue;
                    }
                    SocketTextChannel? socketChannel = guild.GetTextChannel(channel.id);
                    if (socketChannel is not null)
                    {
                        await NukeChannelAsync(socketChannel, database);
                    }
                }
                await database.ApplyChangesAsync();
                await database.DisposeAsync();
            }
            catch (Exception ex)
            {
                await using var database = new DatabaseContext(_configuration);
                await ex.LogErrorAsync(database);
            }
        }
    }

    internal static async Task NukeChannelAsync(IChannel channel, DatabaseContext? database = null, Microsoft.Extensions.Configuration.IConfiguration? configuration = null)
    {
        //check if channel is even a text channel
        if (channel is not ITextChannel textChannel)
        {
            throw new ArgumentNullException(nameof(channel), "Cannot nuke channel, this channel is not a text channel.");
        }
        //create new text channel with same exact settings
        ITextChannel? newTextChannel = await textChannel.Guild.CreateTextChannelAsync(textChannel.Name, x =>
        {
            x.CategoryId = textChannel.CategoryId;
            x.IsNsfw = textChannel.IsNsfw;
            x.Name = textChannel.Name;
            x.PermissionOverwrites = new Optional<IEnumerable<Overwrite>>(textChannel.PermissionOverwrites);
            x.Position = textChannel.Position;
            x.SlowModeInterval = textChannel.SlowModeInterval;
            if (string.IsNullOrEmpty(textChannel.Topic) is false)
            {
                x.Topic = textChannel.Topic;
            }
        });
        //sync permissions if in a category
        ICategoryChannel? categoryChannel = await textChannel.GetCategoryAsync();
        if (categoryChannel is not null)
        {
            if (textChannel.PermissionOverwrites == categoryChannel.PermissionOverwrites)
            {
                await textChannel.SyncPermissionsAsync();
            }
        }
        //delete old channel
        await textChannel.DeleteAsync();
        //post image to new channel
        switch (new Random().Next(1, 6))
        {
            case 1:
                _ = await newTextChannel.SendMessageAsync("https://orbitalsolutions.ca/assets/img/cat-nuke.gif");
                break;
            case 2:
                _ = await newTextChannel.SendMessageAsync("https://orbitalsolutions.ca/assets/img/chicken-nuke.gif");
                break;
            case 3:
                _ = await newTextChannel.SendMessageAsync("https://orbitalsolutions.ca/assets/img/world-nuke.gif");
                break;
            case 4:
                _ = await newTextChannel.SendMessageAsync("https://orbitalsolutions.ca/assets/img/noot-noot-nuke.gif");
                break;
            case 5:
                _ = await newTextChannel.SendMessageAsync("https://orbitalsolutions.ca/assets/img/nuke-press-button.gif");
                break;
            case 6:
                _ = await newTextChannel.SendMessageAsync("https://orbitalsolutions.ca/assets/img/pepe-glasses-watching-nuke.gif");
                break;
        }
        bool nullDB = false;
        if (database is null)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration), "Cannot nuke channel, database and configuration are both null.");
            }

            database = new DatabaseContext(configuration);
            nullDB = true;
        }
        //add channel back to daily nuke channels if exists
        Database.Models.DiscordChannel? nukeChannel = await database.NukeChannels.FirstOrDefaultAsync(x => x.id == textChannel.Id);
        if (nukeChannel is not null)
        {
            nukeChannel.id = newTextChannel.Id;
            await database.ApplyChangesAsync(nukeChannel);
        }
        if (nullDB)
        {
            await database.DisposeAsync();
        }
    }
}