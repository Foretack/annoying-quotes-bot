using TwitchLib.Client;
using TwitchLib.Client.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace annoying_quotes_bot;

public static class Program
{
    private static TwitchClient Client { get; set; } = new TwitchClient();
    private static Random R = new Random();
    static async Task Main(string[] args)
    {
        Client.Initialize(new ConnectionCredentials(Config.BotUsername, Config.AccessToken), Config.Channel);
        Client.Connect();
        Client.OnConnected += (s, e) => Console.WriteLine($"Connected as {e.BotUsername}");
        Client.OnJoinedChannel += (s, e) => Console.WriteLine($"Joined {e.Channel}");
        Client.OnRitualNewChatter += Client_OnRitualNewChatter;
        await DoStuff();
        Start();

        Console.Read();
    }

    private static async void Client_OnRitualNewChatter(object? sender, TwitchLib.Client.Events.OnRitualNewChatterArgs e)
    {
        HttpClient c = new HttpClient();
        c.Timeout = TimeSpan.FromSeconds(2);

        Stream apiResponse = await c.GetStreamAsync($"http://api.alquran.cloud/ayah/{R.Next(6237)}/editions/en.pickthall");
        QuranVerse v = (await JsonSerializer.DeserializeAsync<QuranVerse>(apiResponse))!;

        Console.WriteLine($"new chatter {e.RitualNewChatter.DisplayName} - {v.Data[0].Text}");
        string verseLine = $"@{e.RitualNewChatter.DisplayName}, {v.Data[0].Surah.EnglishName} {{{v.Data[0].Surah.Number}}} -- {v.Data[0].Text}";
        Client.SendMessage(Config.Channel, verseLine.Length >= 490 ?  verseLine[..475] : verseLine);
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


public class Datum
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("surah")]
    public Surah Surah { get; set; }
}


public class QuranVerse
{
    [JsonPropertyName("data")]
    public Datum[] Data { get; set; }
}

public class Surah
{
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("englishName")]
    public string EnglishName { get; set; }
}


