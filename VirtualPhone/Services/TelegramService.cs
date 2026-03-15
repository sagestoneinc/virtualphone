using Telegram.Bot;

namespace VirtualPhone.Services;

/// <summary>
/// Forwards messages to a Telegram chat via the Telegram Bot API.
/// </summary>
public class TelegramService
{
    private readonly ITelegramBotClient _botClient;
    private readonly long _chatId;

    public TelegramService(IConfiguration configuration)
    {
        var botToken = configuration["Telegram:BotToken"]
            ?? throw new InvalidOperationException("Telegram:BotToken is not configured.");
        _chatId = configuration.GetValue<long>("Telegram:ChatId");
        _botClient = new TelegramBotClient(botToken);
    }

    /// <summary>
    /// Sends <paramref name="text"/> to the configured Telegram chat.
    /// </summary>
    public async Task SendMessageAsync(string text)
    {
        await _botClient.SendMessage(_chatId, text);
    }

    /// <summary>
    /// The Telegram Chat ID to which incoming SMS messages are forwarded.
    /// </summary>
    public long ChatId => _chatId;
}
