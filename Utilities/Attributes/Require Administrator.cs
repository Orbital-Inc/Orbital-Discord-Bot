using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using MainBot.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MainBot.Utilities.Attributes;

public class RequireAdministratorAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        switch (context.Client.TokenType)
        {
            case TokenType.Bot:
                IDMChannel? privateChannel = await context.Client.GetDMChannelAsync(context.Channel.Id).ConfigureAwait(false);
                if (privateChannel is not null)
                {
                    return PreconditionResult.FromError(ErrorMessage ?? "Command must be executed in a guild.");
                }

                IApplication? application = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
                if (context.User.Id == application.Owner.Id)
                {
                    return PreconditionResult.FromSuccess();
                }

                DiscordShardedClient? client = services.GetService<DiscordShardedClient>();
                if (client is not null)
                {
                    Discord.Rest.RestGuildUser? user = await client.Rest.GetGuildUserAsync(context.Guild.Id, context.User.Id).ConfigureAwait(false);
                    if (user.GuildPermissions.Administrator || context.Guild.OwnerId == user.Id)
                    {
                        return PreconditionResult.FromSuccess();
                    }

                    await using var database = services.GetService<DatabaseContext>();

                    Database.Models.Guild? guild = await database.Guilds.FirstOrDefaultAsync(x => x.id == context.Guild.Id).ConfigureAwait(false);
                    if (guild is not null)
                    {
                        if (guild.guildSettings.administratorRoleId is not null)
                        {
                            HashSet<ulong>? roles = await user.RoleIds.ToAsyncEnumerable().ToHashSetAsync();
                            if (roles.Contains((ulong)guild.guildSettings.administratorRoleId))
                            {
                                return PreconditionResult.FromSuccess();
                            }
                        }
                    }
                }
                return PreconditionResult.FromError(ErrorMessage ?? "Command can only be executed by an administrator.");
            default:
                return PreconditionResult.FromError($"{nameof(RequireAdministratorAttribute)} is not supported by this {nameof(TokenType)}.");
        }
    }
}
