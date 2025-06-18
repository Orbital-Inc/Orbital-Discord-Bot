using Discord;
using Discord.Interactions;

using MainBot.Utilities.Extensions;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

namespace MainBot.Commands.SlashCommands.APICommands;

public class Geolocation : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly HttpClient _http;

    private readonly IConfiguration _configuration;

    internal Geolocation(HttpClient http, IConfiguration configuration)
    {
        _configuration = configuration;
        _http = http;
    }

    [SlashCommand("geolocate", "Retrieves the geographic location & network details of the specified host.")]
    public async Task GeoLocate(string host)
    {
        _ = await Context.ReplyWithEmbedAsync("Geolocate Host", $"Attempting to geolocate {host}, please wait...");

        if (Uri.CheckHostName(host) is not (UriHostNameType.IPv4 or UriHostNameType.IPv6 or UriHostNameType.Dns))
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", "The specified hostname/IPv4 address is not valid, please try again.", deleteTimer: 60, invisible: true);
            return;
        }

        //adding header for request
        //_http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", _configuration.GetSection("General")["APIToken"]);
        _http.DefaultRequestHeaders.Authorization = null;
        HttpResponseMessage? result = await _http.GetAsync($"https://api.orbitalsolutions.ca/v1/network/geolookup/{host}");
        Models.APIModels.GeolocationModel.CombinedGeoLocationData? Information = null;
        //deserializing request response if successful
        if (result.IsSuccessStatusCode)
        {
            Information = JsonConvert.DeserializeObject<Models.APIModels.GeolocationModel.CombinedGeoLocationData>(await result.Content.ReadAsStringAsync());
        }

        if (Information is null)
        {
            _ = await Context.ReplyWithEmbedAsync("Error Occurred", $"An error occurred when attempting to geolocate the specified host, please try again.\nResponse status: {result.StatusCode}", deleteTimer: 60, invisible: true);
            return;
        }
        List<EmbedFieldBuilder> Fields = [];

        var data = Information.OrbitalGeoData;
        var acc = data?.Accuracy;

        #region Security

        string ExtraInfo = string.Empty;
        AppendIfTrue(ExtraInfo, data?.CloudProvider, "Cloud Provider", acc?.CloudProviderAccuracy);
        AppendIfTrue(ExtraInfo, data?.Abuser, "Abuser", acc?.AbuserAccuracy);
        AppendIfTrue(ExtraInfo, data?.Tor, "Tor", acc?.TorAccuracy);
        AppendIfTrue(ExtraInfo, data?.Attacker, "Attacker", acc?.AttackerAccuracy);
        AppendIfTrue(ExtraInfo, data?.Proxy, "Proxy", acc?.ProxyAccuracy);
        AppendIfTrue(ExtraInfo, data?.Relay, "Relay", acc?.RelayAccuracy);
        AppendIfTrue(ExtraInfo, data?.Anonymous, "Anonymous", acc?.AnonymousAccuracy);
        AppendIfTrue(ExtraInfo, data?.Bogon, "Bogon", acc?.BogonAccuracy);
        AppendIfTrue(ExtraInfo, data?.TorExit, "Tor Exit", acc?.TorExitAccuracy);
        AppendIfTrue(ExtraInfo, data?.Threat, "Threat", acc?.ThreatAccuracy);
        AppendIfTrue(ExtraInfo, data?.IcloudRelay, "iCloud Relay", acc?.IcloudRelayAccuracy);
        AppendIfTrue(ExtraInfo, data?.Datacenter, "Datacenter", acc?.DatacenterAccuracy);

        #endregion Security

        Fields.Add(new EmbedFieldBuilder
        {
            Name = "Network",
            Value =
                $"{(string.IsNullOrWhiteSpace(data?.IPAddress) ? "" : $"`IP Address`: {data.IPAddress}\n")}" +
                $"{(string.IsNullOrWhiteSpace(data?.Hostname) ? "" : $"`Hostname`: {data.Hostname}{(acc is null ? "" : $" `{acc.HostnameAccuracy}%`")}\n")}" +
                $"{(string.IsNullOrWhiteSpace(data?.Route) ? "" : $"`Route`: {data.Route}{(acc is null ? "" : $" `{acc.RouteAccuracy}%`")}\n")}" +
                $"{(string.IsNullOrWhiteSpace(data?.Type) ? "" : $"`Type`: {data.Type}{(acc is null ? "" : $" `{acc.TypeAccuracy}%`")}\n")}" +
                ExtraInfo
        });

        Fields.Add(new EmbedFieldBuilder
        {
            Name = "Provider",
            Value =
                $"{(string.IsNullOrWhiteSpace(data?.Domain) ? "" : $"`Domain`: [{data.Domain}](http://{data.Domain}){(acc is null ? "" : $" `{acc.DomainAccuracy}%`")}\n")}" +
                $"{(string.IsNullOrWhiteSpace(data?.Organization) ? "" : $"`Organization`: {data.Organization}{(acc is null ? "" : $" `{acc.OrganizationAccuracy}%`")}\n")}" +
                $"{(string.IsNullOrWhiteSpace(data?.ISP) ? "" : $"`ISP`: {data.ISP}{(acc is null ? "" : $" `{acc.ISPAccuracy}%`")}\n")}" +
                $"{(string.IsNullOrWhiteSpace(data?.ASName) ? "" : $"`AS Name`: {data.ASName}{(acc is null ? "" : $" `{acc.ASNameAccuracy}%`")}\n")}" +
                $"{(data?.ASNumber is null ? "" : $"`AS Number`: {data.ASNumber}{(acc is null ? "" : $" `{acc.ASNumberAccuracy}%`")}\n")}"
        });

        Fields.Add(new EmbedFieldBuilder
        {
            Name = "Location",
            Value =
                $"{(string.IsNullOrWhiteSpace(data?.Country) ? "" : $"`Country`: {data.Country}{(acc is null ? "" : $" `{acc.CountryAccuracy}%`")}\n")}" +
                $"{(string.IsNullOrWhiteSpace(data?.Region) ? "" : $"`Region`: {data.Region}{(acc is null ? "" : $" `{acc.RegionAccuracy}%`")}\n")}" +
                $"{(string.IsNullOrWhiteSpace(data?.District) ? "" : $"`District`: {data.District}{(acc is null ? "" : $" `{acc.DistrictAccuracy}%`")}\n")}" +
                $"{(string.IsNullOrWhiteSpace(data?.City) ? "" : $"`City`: {data.City}{(acc is null ? "" : $" `{acc.CityAccuracy}%`")}\n")}"
        });

        _ = await Context.ReplyWithEmbedAsync(
            $"Geolocate Complete For: {host}",
            "",
            $"https://orbitalsolutions.ca/network/tools/geolocation?ip={data?.IPAddress}",
            data?.Flag,
            embeds: Fields);
    }

    void AppendIfTrue(string message, bool? condition, string label, double? accuracy)
    {
        if (condition == true)
            message += $"`{label}`: True{(accuracy is null ? "" : $" `{accuracy}%`")}\n";
    }
}