using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using MainBot.Commands.SlashCommands.GuildCommands.SettingsCommands;
using MainBot.Database;
using MainBot.Events;
using MainBot.Loggers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MainBot.Services;

internal class StartupService
{
    private readonly DiscordShardedClient _client;

    private readonly IConfiguration _configuration;

    internal StartupService()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("secrets.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();

        _client = new DiscordShardedClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose,
            AlwaysDownloadUsers = true,
            GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
            UseSystemClock = false,
            MessageCacheSize = 250,
            UseInteractionSnowflakeDate = true,
            LogGatewayIntentWarnings = false,
            AlwaysDownloadDefaultStickers = false,
            AlwaysResolveStickers = false,
        });
    }

    internal async Task RunAsync()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider? provider = services.BuildServiceProvider();

        await provider.GetRequiredService<DatabaseContext>().Database.MigrateAsync();
        provider.GetRequiredService<DiscordLogger>();
        provider.GetRequiredService<CustomService>();
        provider.GetRequiredService<ChannelEventHandler>();
        provider.GetRequiredService<MessageEventHandler>();
        provider.GetRequiredService<UserEventHandler>();
        provider.GetRequiredService<MenuEventHandler>();
        await provider.GetRequiredService<InteractionEventHandler>().InitializeAsync();
        provider.GetRequiredService<MenuEventHandler>().Initialize();
        await provider.GetRequiredService<DailyChannelNukeService>().StartAsync(new CancellationToken());
        await provider.GetRequiredService<AutoUnmuteUserService>().StartAsync(new CancellationToken());
        await provider.GetRequiredService<RainbowRoleService>().StartAsync(new CancellationToken());
#if DEBUG
        await _client.LoginAsync(TokenType.Bot, _configuration.GetSection("General")["TestToken"]);
#else
            await _client.LoginAsync(TokenType.Bot, _configuration.GetSection("General")["Token"]);
#endif
        await _client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        _ = services.AddSingleton(_client)
            .AddSingleton(_configuration)
            .AddSingleton<DiscordLogger>()
            .AddSingleton<InteractionEventHandler>()
            .AddSingleton<MessageEventHandler>()
            .AddSingleton<UserEventHandler>()
            .AddSingleton<MenuEventHandler>()
            .AddSingleton<DailyChannelNukeService>()
            .AddSingleton<RainbowRoleService>()
            .AddSingleton<ChannelEventHandler>()
            .AddSingleton<AutoUnmuteUserService>()
            .AddSingleton<CustomService>()
            .AddSingleton<GuildRoleSettingsCommand>()
            .AddSingleton(new Random())
            .AddSingleton(new HttpClient())
            .AddDbContext<DatabaseContext>(options => options.UseNpgsql(_configuration.GetConnectionString("PostgresConnectionString")))
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordShardedClient>(), new InteractionServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Verbose
            }));
    }
}
