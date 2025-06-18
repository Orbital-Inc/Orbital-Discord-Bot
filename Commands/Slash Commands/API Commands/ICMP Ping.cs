using Discord;
using Discord.Interactions;

using MainBot.Utilities.Extensions;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

namespace MainBot.Commands.SlashCommands.APICommands;

public class ICMPPing : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly HttpClient _http;

    private readonly IConfiguration _configuration;

    internal ICMPPing(HttpClient http, IConfiguration configuration)
    {
        _configuration = configuration;
        _http = http;
    }

    [SlashCommand("ping-icmp", "Sends an ICMP packet to a specified host in hopes for a reponse.")]
    public async Task PingHost(string host)
    {
        _ = await Context.ReplyWithEmbedAsync("ICMP Ping", $"Attempting to ICMP ping {host}, please wait...");

        if (Uri.CheckHostName(host) is not (UriHostNameType.IPv4 or UriHostNameType.IPv6 or UriHostNameType.Dns))
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "The specified hostname/IPv4 address is not valid, please try again.", deleteTimer: 60, invisible: true);
            return;
        }

        //add header
        //_http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", _configuration.GetSection("General")["APIToken"]);
        _http.DefaultRequestHeaders.Authorization = null;
        //response
        HttpResponseMessage? result = await _http.GetAsync($"https://api.orbitalsolutions.ca/v1/network/networkping/{host}/icmp");
        Models.APIModels.ICMPPingModel? PingResults = null;
        if (result.IsSuccessStatusCode)
        {
            PingResults = JsonConvert.DeserializeObject<Models.APIModels.ICMPPingModel>(await result.Content.ReadAsStringAsync());
        }
        if (PingResults is null)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", $"An error occurred while attempting to ping, please try again.\nResponse Status: {result.StatusCode}", deleteTimer: 60, invisible: true);
            return;
        }
        string embedValue = string.Empty;

        if (PingResults.results is not null)
        {
            await PingResults.results.ToAsyncEnumerable().ForEachAsync(value =>
            {
                embedValue += $"[{PingResults.host}](https://check-host.net/check-ping?host={PingResults.host}) {(value.recievedResponse ? $"replied back in `{value.responseTime}`ms" : "failed to reply back")}\n";
            });
        }

        if (PingResults.averageResponseTime is not null)
        {
            embedValue += $"Average: `{PingResults.averageResponseTime}`ms Maximum: `{PingResults.maximumResponseTime}`ms Minimum: `{PingResults.minimumResponseTime}`ms";
        }

        List<EmbedFieldBuilder> Fields = new()
        {
            new EmbedFieldBuilder
            {
                Name = "ICMP Ping Results",
                Value = embedValue
            }
        };
        _ = await Context.ReplyWithEmbedAsync($"ICMP Ping Complete For: {PingResults.host}", string.Empty, $"https://orbitalsolutions.ca/geolocation?ip={PingResults.host}", string.Empty, string.Empty, Fields);
    }
}