using Discord;
using Discord.Interactions;

using MainBot.Database;
using MainBot.Utilities.Attributes;
using MainBot.Utilities.Extensions;

using Microsoft.EntityFrameworkCore;

namespace MainBot.Commands.SlashCommands.GuildCommands.SettingsCommands;

[RequireAdministrator]
public class GuildEmbedSettingsCommand(DatabaseContext database) : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly DatabaseContext _database = database;

    public enum guildEmbedOption
    {
        send_verify_embed,
        send_ticket_embed,
        send_rules_embed,
        send_announcement,
        send_rule_ticket_embed,
        send_aio_embed
    }

    [SlashCommand("guild-embed-settings", "Guild settings that involve sending an embed.")]
    public async Task ExecuteCommand(guildEmbedOption embedOption, IChannel channel, string? description = null)
    {
        if (channel is not ITextChannel textChannel)
        {
            throw new ArgumentNullException(nameof(textChannel), "This channel is not a text channel.");
        }

        Database.Models.Guild? guildEntry = await _database.Guilds.FirstOrDefaultAsync(x => x.id == Context.Guild.Id);
        if (guildEntry is null)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "This requires the guild to be backed up.", deleteTimer: 60, invisible: true);
            return;
        }
        switch (embedOption)
        {
            case guildEmbedOption.send_verify_embed:
                await SendVerifyMessage(textChannel, description);
                break;
            case guildEmbedOption.send_ticket_embed:
                await SendTicketMessage(textChannel, "Ticket", "Click to open a ticket with the staff.", "Open a ticket");
                break;
            case guildEmbedOption.send_rules_embed:
                await SendRulesMessage(textChannel);
                break;
            case guildEmbedOption.send_announcement:
                if (description is null)
                {
                    _ = await Context.ReplyWithEmbedAsync("Error Occurred", "This requires a message to be sent with the embed.", deleteTimer: 60, invisible: true);
                    return;
                }
                await SendAnnouncementMessage(textChannel, description);
                break;
            case guildEmbedOption.send_rule_ticket_embed:
                await SendRulesMessage(textChannel, true);
                break;
            case guildEmbedOption.send_aio_embed:
                await SendRulesMessage(textChannel, true, true);
                break;
            default:
                _ = await Context.ReplyWithEmbedAsync("Error Occurred", "Invalid option selected.", deleteTimer: 60, invisible: true);
                return;
        }
        _ = await Context.ReplyWithEmbedAsync("Guild Embed Settings", $"Successfully sent the embed to: {textChannel.Mention}", deleteTimer: 60, invisible: true);
    }

    private async Task SendVerifyMessage(ITextChannel channel, string? description = "Click to verify.")
    {
        MessageComponent? msg = new ComponentBuilder()
        {
            ActionRows = new List<ActionRowBuilder>()
                {
                    new()
                    {
                        Components = new List<IMessageComponent>
                        {
                            new ButtonBuilder()
                            {
                                CustomId = "verify-button",
                                Style = ButtonStyle.Primary,
                                Label = "Verify",
                            }.Build(),
                        }
                    }
                }
        }.Build();
        Embed? embed = new EmbedBuilder()
        {
            Title = "Verification",
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
            Description = description is null ? "Click to verify." : description,
        }.Build();
        _ = await channel.SendMessageAsync(embed: embed, components: msg);
    }

    private async Task SendTicketMessage(ITextChannel channel, string title, string description, string buttonLabel)
    {
        MessageComponent? msg = new ComponentBuilder()
        {
            ActionRows = new List<ActionRowBuilder>()
            {
                new()
                {
                    Components = new List<IMessageComponent>
                    {
                        new ButtonBuilder()
                        {
                            CustomId = "open-ticket-button",
                            Style = ButtonStyle.Primary,
                            Label = buttonLabel,
                        }.Build(),
                    }
                }
            }
        }.Build();
        Embed? embed = new EmbedBuilder()
        {
            Title = title,
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
            Description = description,
        }.Build();
        var sentMsg = await channel.SendMessageAsync(embed: embed, components: msg);
        await sentMsg.PinAsync();
    }

    private async Task SendRulesMessage(ITextChannel channel, bool ticketButton = false, bool hiddenRoleButton = false)
    {
        MessageComponent? msg = hiddenRoleButton ? new ComponentBuilder()
        {
            ActionRows = new List<ActionRowBuilder>()
            {
                new()
                {
                    Components = new List<IMessageComponent>
                    {
                        new ButtonBuilder()
                        {
                            CustomId = "open-ticket-button",
                            Style = ButtonStyle.Primary,
                            Label = "Open ticket",
                        }.Build(),
                    }
                },
                new()
                {
                    Components = new List<IMessageComponent>
                    {
                        new ButtonBuilder()
                        {
                            CustomId = "custom-role-button",
                            Style = ButtonStyle.Secondary,
                            Label = "Unlock Private Access"
                        }.Build(),
                    }
                }
            }
        }.Build()
        : new ComponentBuilder()
        {
            ActionRows = new List<ActionRowBuilder>()
            {
                new()
                {
                    Components = new List<IMessageComponent>
                    {
                        new ButtonBuilder()
                        {
                            CustomId = "open-ticket-button",
                            Style = ButtonStyle.Primary,
                            Label = "Open ticket",
                        }.Build(),
                    }
                }
            }
        }.Build();
        Embed? embed = new EmbedBuilder()
        {
            Title = $"{channel.Guild.Name} Rules",
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
            Description =
    $"{Context.Guild.Name} Server Rules & Guidelines 🚀\n\n" +
    "1. **Adhere to Discord's Policies**\n" +
    "   Always follow the [Discord Terms of Service](https://discord.com/terms) and [Community Guidelines](https://discord.com/guidelines).\n\n" +
    "2. **Respect Privacy**\n" +
    "   Do not share anyone's real-life location, phone number, or any private information that could compromise someone's privacy.\n\n" +
    "3. **No Illegal Activity**\n" +
    "   Discussions or sharing of illegal activities (e.g., unethical hacking, DDoS attacks, botnets, web stressors, doxing, swatting) are strictly prohibited.\n\n" +
    "4. **Zero Tolerance for Threats**\n" +
    "   Do not threaten or talk about harming any members or staff in any capacity.\n\n" +
    "5. **No Advertising or Spamming**\n" +
    "   Advertising is not permitted anywhere, including direct messages. This rule helps maintain a clean environment free of spam and abuse. **Note:** This server does not promote or encourage spam.\n\n" +
    "6. **Avoid Spamming & Disruption**\n" +
    "   Refrain from spamming, flooding, or engaging in any activities that could hinder the usability or experience of the server.\n\n" +
    "7. **No Exploitation or Abuse**\n" +
    "   Do not abuse or exploit any server features or bot-based systems.\n\n" +
    "8. **Stay on Topic**\n" +
    "   Stick to each channel’s topic, and make sure to post content in the correct channels.\n\n" +
    "9. **Respect Staff & Members**\n" +
    "   Respect all members of the server, including owners and staff. Remember that we reserve the right to issue warnings, mutes, kicks, or bans if rules are broken. We also reserve the right to act on any behavior that may be disruptive, even if not explicitly listed in the rules.\n\n" +
    $"By participating in the server and sending messages, you agree to all the rules mentioned above. Let's keep {Context.Guild.Name} a welcoming and fun place for everyone! 🌌"

        }.WithCurrentTimestamp().Build();
        _ = ticketButton ? await channel.SendMessageAsync(embed: embed, components: msg) : await channel.SendMessageAsync(embed: embed);
    }

    private async Task SendAnnouncementMessage(ITextChannel channel, string description)
    {
        Embed? embed = new EmbedBuilder()
        {
            Title = $"Server Announcement",
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
            Description = description
        }.WithCurrentTimestamp().Build();
        _ = await channel.SendMessageAsync(Context.Guild.EveryoneRole.Mention, embed: embed);
    }
}
