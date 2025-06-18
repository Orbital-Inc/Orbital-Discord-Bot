using Discord.Interactions;

using MainBot.Database;

using Microsoft.EntityFrameworkCore;

namespace MainBot.Buttons;
public class CustomRoleButton(DatabaseContext database) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly DatabaseContext _database = database;

    [ComponentInteraction("custom-role-button")]
    public async Task ExecuteAsync()
    {
        await Context.Interaction.DeferAsync(true);
        Discord.WebSocket.SocketGuildUser? user = Context.Guild.GetUser(Context.Interaction.User.Id);
        if (user is null)
        {
            return;
        }

        Database.Models.Guild? guildEntry = await _database.Guilds.FirstOrDefaultAsync(x => x.id == Context.Guild.Id);
        if (guildEntry is null)
        {
            return;
        }

        if (guildEntry.guildSettings.hiddenRoleId is null)
        {
            return;
        }

        await user.AddRoleAsync((ulong)guildEntry.guildSettings.hiddenRoleId);
    }
}
