using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WebHaven.Telegrambot;


internal class Program
{
    private static readonly HttpClientHandler _httpClientHandler = new()
    {
        // for development
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    private static readonly HttpClient _httpClient = new(_httpClientHandler);
    private static readonly TelegramBotClient _bot = new("");
    public static void Main(string[] args)
    {
        

        using var cts = new CancellationTokenSource();

        _bot.StartReceiving(
            HandleUpdate,
            HandleError,
            null,
            cts.Token
        );

        Console.WriteLine("Start listening for updates. Press enter to stop");
        Console.ReadLine();

        cts.Cancel();
    }

    // Each time a user interacts with the bot, this method is called
    static async Task HandleUpdate(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            // A message was received
            case UpdateType.Message:
                await HandleMessage(update.Message!);
                break;
            default:
                await _bot.SendTextMessageAsync(update.Message!.From!.Id, "Unrecognized command");
                break;
        }
    }

    static async Task HandleError(ITelegramBotClient _, Exception exception, CancellationToken cancellationToken)
    {
        await Console.Error.WriteLineAsync(exception.Message);
    }

    static async Task HandleMessage(Message msg)
    {
        User? user = msg.From;
        var text = msg.Text ?? string.Empty;

        if (user is null)
            return;

        if (text.StartsWith('/'))
            await HandleCommand(user.Id, text);
        else
            await _bot.SendTextMessageAsync(user.Id, "Unrecognized command");
    }
    static async Task HandleCommand(long userId, string command)
    {
        switch (command)
        {
            case "/getblogs":
                var result = await GetBlogs();
                if (result is null)
                    await _bot.SendTextMessageAsync(userId, "Internal server error. Please try again later");
                else
                    await SendBlogsMessages(userId, result);
                break;

            default:
                await _bot.SendTextMessageAsync(userId, "Unrecognized command");
                break;
        }
    }

    static async Task SendBlogsMessages(long userId, IDictionary<string, PostSummary[]> posts)
    {
        foreach (var uri in posts.Keys)
        {
            await _bot.SendTextMessageAsync(userId, uri);
            foreach (var post in posts[uri])
                await _bot.SendTextMessageAsync(userId, post.ToString());
        }
    }

    static async Task<IDictionary<string, PostSummary[]>?> GetBlogs()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://localhost:7246/api/blogs");
            var stream = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<IDictionary<string, PostSummary[]>>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return result!;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }
}