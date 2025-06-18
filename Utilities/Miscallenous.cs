using Discord;

using Newtonsoft.Json;

namespace MainBot.Utilities;

internal class Miscallenous
{
    internal static Color RandomDiscordColour() => new(new Random().Next(0, 255), new Random().Next(0, 255), new Random().Next(0, 255));
    internal static async ValueTask<Color> RandomDiscordColourAsync(ICollection<uint>? uglyColours, string apiToken)
    {
        Color colour = RandomDiscordColour();
        if (uglyColours is null)
        {
            return colour;
        }

        for (int i = 0; i < 10; i++)
        {
            if (uglyColours.Contains(colour.RawValue) is false)
            {
                return colour;
            }

            colour = RandomDiscordColour();
            // Add AI to check if colour is ugly
            //var aiResponse = await GetAIResponseAsync(apiToken, $"Is {colour.R} {colour.G} {colour.B} ugly? I am basing \"pretty colours\" on vibrant and uplifting shades. \"Ugly colours\" are mainly poop-looking and similar colour shades. Please respond with a yes or no answer.");

            //if (aiResponse == "Yes")
            //{
            //    continue;
            //}
            //else
            //{
            //    return colour;
            //}
        }

        return colour;
    }
    internal static GuildPermissions MutePermsRole() => new(addReactions: false, sendMessages: false);
    internal static OverwritePermissions MutePermsChannel() => new(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, usePublicThreads: PermValue.Deny);
    internal static OverwritePermissions TicketPermsChannel() => new(viewChannel: PermValue.Allow);
    internal static OverwritePermissions EveryoneTicketPermsChannel() => new(viewChannel: PermValue.Deny);
    internal static async Task<string> GetAIResponseAsync(string token, string message, string model = "phi3", string endpoint = "http://ai-api.kennedyportal.org/api/generate", HttpClient? httpClient = null)
    {
        httpClient ??= new();
        httpClient.DefaultRequestHeaders.Add("Authorization", token);
        httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        var requestMessage = new 
        {
            model,
            prompt = message,
            stream = false,
            options = new
            {
                temperature = 0
            }
        };
        var response = await httpClient.PostAsync(endpoint + JsonConvert.SerializeObject(requestMessage), null);
        if (response.IsSuccessStatusCode is false)
        {
            return "Error";
        }
        var responseString = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
        return responseString["response"];
    }
}
