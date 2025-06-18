using Discord.Interactions;

using MainBot.Database;
using MainBot.Database.Models;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;

namespace MainBot.Commands.SlashCommands.GuildCommands;

[Utilities.Attributes.RequireOwner]
public class BackupCommand(DatabaseContext database) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly DatabaseContext _database = database;

    [SlashCommand("backup", "Backup this entire server, includes: roles, channels, *users*, & permissions.")]
    public Task BackupDiscordServerSlashCommand() => BackupServerAsync();

    private async Task BackupServerAsync()
    {
        await Context.Interaction.DeferAsync();
        Guild? guildEntry = await _database.Guilds.FirstOrDefaultAsync(x => x.id == Context.Guild.Id);
        if (guildEntry is null)
        {
            guildEntry = new Guild
            {
                id = Context.Guild.Id,
                name = Context.Guild.Name,
            };
            await _database.AddAsync(guildEntry);
            await _database.ApplyChangesAsync();
        }
        else
        {
            guildEntry.name = Context.Guild.Name;
            await _database.ApplyChangesAsync(guildEntry);
        }
        _ = await Context.ReplyWithEmbedAsync("Server Backup", $"Successfully completed backing up the server.", deleteTimer: 60);
    }
}
