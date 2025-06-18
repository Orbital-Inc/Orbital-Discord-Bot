using Discord.Interactions;

namespace MainBot.Buttons;

public class CloseTicketButton : InteractionModuleBase<ShardedInteractionContext>
{
    [ComponentInteraction("close-ticket-button")]
    public Task CloseTicket() => Context.Guild.GetTextChannel(Context.Interaction.Channel.Id).DeleteAsync();
}
