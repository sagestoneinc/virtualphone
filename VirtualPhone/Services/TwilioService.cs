using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace VirtualPhone.Services;

/// <summary>
/// Sends outbound SMS messages via the Twilio REST API.
/// </summary>
public class TwilioService
{
    private readonly string _fromNumber;

    public TwilioService(IConfiguration configuration)
    {
        var accountSid = configuration["Twilio:AccountSid"]
            ?? throw new InvalidOperationException("Twilio:AccountSid is not configured.");
        var authToken = configuration["Twilio:AuthToken"]
            ?? throw new InvalidOperationException("Twilio:AuthToken is not configured.");
        _fromNumber = configuration["Twilio:FromNumber"]
            ?? throw new InvalidOperationException("Twilio:FromNumber is not configured.");

        TwilioClient.Init(accountSid, authToken);
    }

    /// <summary>
    /// Sends an SMS from the configured Twilio number to <paramref name="toNumber"/>.
    /// </summary>
    public async Task SendSmsAsync(string toNumber, string body)
    {
        await MessageResource.CreateAsync(
            body: body,
            from: new PhoneNumber(_fromNumber),
            to: new PhoneNumber(toNumber)
        );
    }
}
