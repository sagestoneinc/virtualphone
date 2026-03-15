using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VirtualPhone.Services;

namespace VirtualPhone.Controllers;

/// <summary>
/// Handles incoming Telegram bot updates and forwards text messages as SMS via Twilio.
/// Register this URL as your Telegram bot's webhook:
///   POST https://api.telegram.org/bot{token}/setWebhook?url=https://your-host/telegram/webhook
/// </summary>
[ApiController]
[Route("telegram")]
public class TelegramController : ControllerBase
{
    private readonly TwilioService _twilioService;
    private readonly TelegramService _telegramService;
    private readonly ILogger<TelegramController> _logger;
    private readonly IConfiguration _configuration;

    public TelegramController(
        TwilioService twilioService,
        TelegramService telegramService,
        ILogger<TelegramController> logger,
        IConfiguration configuration)
    {
        _twilioService = twilioService;
        _telegramService = telegramService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Telegram webhook endpoint. Receives <see cref="Update"/> objects from the Telegram API.
    /// Text messages from the configured chat are forwarded as SMS to the configured Twilio number.
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] Update update)
    {
        if (update.Type != UpdateType.Message || update.Message?.Text is null)
        {
            return Ok();
        }

        var message = update.Message;

        // Only process messages from the configured Telegram chat.
        if (message.Chat.Id != _telegramService.ChatId)
        {
            _logger.LogWarning(
                "Received message from unexpected chat {ChatId}. Expected {ExpectedChatId}.",
                message.Chat.Id, _telegramService.ChatId);
            return Ok();
        }

        var toNumber = _configuration["Twilio:ToNumber"]
            ?? throw new InvalidOperationException("Twilio:ToNumber is not configured.");

        _logger.LogInformation(
            "Forwarding Telegram message from chat {ChatId} as SMS to {ToNumber}.",
            message.Chat.Id, toNumber);

        await _twilioService.SendSmsAsync(toNumber, message.Text);
        return Ok();
    }
}
