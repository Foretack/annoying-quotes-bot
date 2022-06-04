﻿using TwitchLib.Client;
using TwitchLib.Client.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace annoying_quotes_bot;

public static class Program
{
    private static TwitchClient Client { get; set; } = new TwitchClient();
    static async Task Main(string[] args)
    {
        Client.Initialize(new ConnectionCredentials(Config.BotUsername, Config.AccessToken), Config.Channel);
        Client.Connect();
        Client.OnConnected += (s, e) => Console.WriteLine($"Connected as {e.BotUsername}");
        Client.OnJoinedChannel += (s, e) => Console.WriteLine($"Joined {e.Channel}");
        await DoStuff();
        Start();

        Console.Read();
    }

    public static void Start()
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        timer.Interval = Config.MessageFrequencyInSeconds * 1000;
        timer.Start();
        timer.Elapsed += Timer_Elapsed;
    }

    private static async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        await DoStuff();
    }

    private static async Task DoStuff()
    {
        HttpClient c = new HttpClient();
        Random r = new Random();
        string tmiResponse = await c.GetStringAsync($"https://tmi.twitch.tv/group/user/{Config.Channel}/chatters");
        List<string> chatters = JsonSerializer.Deserialize<TMI>(tmiResponse)!.chatters.viewers.Where(x => x.ToLower() != Config.BotUsername.ToLower()).ToList();
        string quoteApiResponse = await c.GetStringAsync($"https://zenquotes.io/api/random/");
        string quote = JsonSerializer.Deserialize<Quote[]>(quoteApiResponse)!.FirstOrDefault()!.Text;
        Client.SendMessage(Config.Channel, $"@{chatters[r.Next(chatters.Count)]} {quote}");
    }
}

public class Quote
{
    [JsonPropertyName("q")]
    public string Text { get; set; }
    [JsonPropertyName("a")]
    public string Author { get; set; }
}

public class TMI
{
    public Chatters chatters { get; set; }
}
public class Chatters
{
    public List<string> viewers { get; set; }
}


