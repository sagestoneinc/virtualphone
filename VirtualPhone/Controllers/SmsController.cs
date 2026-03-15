using Microsoft.AspNetCore.Mvc;
using Twilio.TwiML;
using VirtualPhone.Services;

namespace VirtualPhone.Controllers;

/// <summary>
/// Handles incoming SMS messages from Twilio and forwards them to Telegram.
/// Configure your Twilio phone number's messaging webhook to POST to /sms/incoming.
/// </summary>
[ApiController]
[Route("sms")]
public class SmsController : ControllerBase
{
    private readonly TelegramService _telegramService;
    private readonly ILogger<SmsController> _logger;

    public SmsController(TelegramService telegramService, ILogger<SmsController> logger)
    {
        _telegramService = telegramService;
        _logger = logger;
    }

    /// <summary>
    /// Twilio webhook endpoint for incoming SMS messages.
    /// Forwards the message body and sender number to the configured Telegram chat.
    /// </summary>
    [HttpPost("incoming")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<ContentResult> Incoming([FromForm] string? Body, [FromForm] string? From)
    {
        if (!string.IsNullOrWhiteSpace(Body) && !string.IsNullOrWhiteSpace(From))
        {
            var text = $"📱 SMS from {From}:\n{Body}";
            _logger.LogInformation("Received SMS from {From}. Forwarding to Telegram.", From);
            await _telegramService.SendMessageAsync(text);
        }

        // Respond with an empty TwiML document so Twilio does not retry.
        var twiml = new MessagingResponse();
        return Content(twiml.ToString(), "text/xml");
    }
}
