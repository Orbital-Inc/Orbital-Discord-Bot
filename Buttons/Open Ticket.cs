using Discord;
using Discord.Interactions;
using Discord.Rest;

using MainBot.Database;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;

namespace MainBot.Buttons;

public class OpenTicketButton(DatabaseContext database) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly DatabaseContext _database = database;

    [ComponentInteraction("open-ticket-button")]
    public async Task OpenTicket()
    {
        Discord.WebSocket.SocketGuildChannel? channel = Context.Guild.Channels.FirstOrDefault(x => x.Name.Contains($"ticket-{Context.Interaction.User.Username}", StringComparison.OrdinalIgnoreCase));
        if (channel is not null)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Please close your open ticket, before opening a new one.", deleteTimer: 60, invisible: true);
            return;
        }
        Database.Models.Guild? guild = await _database.Guilds.FirstOrDefaultAsync(x => x.id == Context.Guild.Id);

        RestTextChannel? ticketChannel = ticketChannel = await Context.Guild.CreateTextChannelAsync($"ticket-{Context.Interaction.User.Username}", x =>
        {
            x.CategoryId = guild?.guildSettings.ticketCategoryId;
            x.Topic = $"Ticket for {Context.Interaction.User.Username}";
            x.PermissionOverwrites = new List<Overwrite>()
            {
                new (Context.Guild.EveryoneRole.Id, PermissionTarget.Role, Utilities.Miscallenous.EveryoneTicketPermsChannel()),
                new (Context.User.Id, PermissionTarget.User, Utilities.Miscallenous.TicketPermsChannel()),
            };
        });
        //await ticketChannel.ModifyAsync(x => x.CategoryId = guild?.guildSettings.ticketCategoryId);
        //await ticketChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, Utilities.Miscallenous.EveryoneTicketPermsChannel());
        //await ticketChannel.AddPermissionOverwriteAsync(Context.User, Utilities.Miscallenous.TicketPermsChannel());
        if (guild is not null)
        {
            if (guild.guildSettings.moderatorRoleId is not null)
            {
                await ticketChannel.AddPermissionOverwriteAsync(Context.Guild.GetRole((ulong)guild.guildSettings.moderatorRoleId), Utilities.Miscallenous.TicketPermsChannel());
            }

            if (guild.guildSettings.administratorRoleId is not null)
            {
                await ticketChannel.AddPermissionOverwriteAsync(Context.Guild.GetRole((ulong)guild.guildSettings.administratorRoleId), Utilities.Miscallenous.TicketPermsChannel());
            }
        }
        _ = SendTicketMessage(ticketChannel);
        _ = await Context.ReplyWithEmbedAsync("Ticket", $"Successfully opened ticket {ticketChannel.Mention}.", deleteTimer: 60, invisible: true);
    }

    private async Task SendTicketMessage(RestTextChannel channel)
    {
        MessageComponent? msg = new ComponentBuilder()
        {
            ActionRows = new List<ActionRowBuilder>()
                {
                    new ()
                    {
                        Components = new List<IMessageComponent>
                        {
                            new ButtonBuilder()
                            {
                                CustomId = "close-ticket-button",
                                Style = ButtonStyle.Danger,
                                Label = "Close Ticket",
                            }.Build(),
                        }
                    }
                }
        }.Build();
        Embed? embed = new EmbedBuilder()
        {
            Title = $"Ticket",
            Color = Utilities.Miscallenous.RandomDiscordColour(),
            Author = new EmbedAuthorBuilder
            {
                Url = "https://orbitalsolutions.ca",
                Name = "Orbital, Inc.",
                IconUrl = Context.Guild.IconUrl
            },
            Footer = new EmbedFooterBuilder
            {
                Text = Context.Guild.Name,
                IconUrl = Context.Guild.IconUrl
            },
            Description = "Click to close the ticket.",
        }.WithCurrentTimestamp().Build();
        var ticket = await channel.SendMessageAsync(embed: embed, components: msg);
        _ = ticket.PinAsync();
    }
}