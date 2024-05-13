using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
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

    private static string? _blogUrl(string s) => s switch
    {
        "Andrew Lock" => "https://andrewlock.net",
        "David Pine" => "https://davidpine.net/blog",
        "Dev blogs" => "https://devblogs.microsoft.com/dotnet",
        "Enterprise Craftsmanship" => "https://enterprisecraftsmanship.com",
        "Steve Gordon" => "https://www.stevejgordon.co.uk",
        _ => null
    };

    readonly static InlineKeyboardMarkup inlineKeyboardMarkup = new(
    [
        [
            InlineKeyboardButton.WithCallbackData("Andrew Lock"),
            InlineKeyboardButton.WithCallbackData("David Pine"),
        ],
        [
            InlineKeyboardButton.WithCallbackData("Dev blogs"),
            InlineKeyboardButton.WithCallbackData("Enterprise Craftsmanship"),
        ],
        [InlineKeyboardButton.WithCallbackData("Steve Gordon")]
    ]);

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
        if (update.Message?.Date < DateTime.UtcNow.AddSeconds(-10))
            return;
        switch (update.Type)
        {
            // A message was received
            case UpdateType.Message:
                await HandleMessage(update.Message!);
                break;
            case UpdateType.CallbackQuery:
                await HandleButton(update.CallbackQuery!);
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
                await _bot.SendTextMessageAsync(userId, "Which blog you want to get?", replyMarkup: inlineKeyboardMarkup);
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

    static async Task<PostSummary[]?> GetBlog(string url)
    {
        _ = new Uri(url); // throw an exception because it should always be valid
        try
        {
            var response = await _httpClient.GetAsync($"https://localhost:7246/api/blogs?id={url}");
            var stream = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PostSummary[]>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return result!;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }
    static async Task HandleButton(CallbackQuery query)
    {
        var url = _blogUrl(query.Data!);
        if (url is null)
            return;

        var posts = await GetBlog(url);
        if (posts is null)
        {
            await _bot.SendTextMessageAsync(query.Message!.Chat.Id, "The server is down. Please try again later..");
            return;
        }

        foreach (var post in posts)
        {
            await _bot.SendTextMessageAsync(query.Message!.Chat.Id, post.ToString());
        }
    }
}