# VirtualPhone

An ASP.NET Core Web API that bridges **Telegram** and **Twilio** SMS. Messages you send from your Telegram account are forwarded as SMS via your Twilio number, and replies to that number are forwarded back to your Telegram chat.

## How it works

```
Telegram user ──► Telegram Bot webhook ──► TelegramController ──► Twilio SMS (outbound)
                                                                         │
Twilio number ◄── SMS reply ◄──────────────────────────────────────────┘
     │
     └──► Twilio webhook ──► SmsController ──► Telegram chat (forwarded message)
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A [Twilio account](https://www.twilio.com/) with a phone number that supports SMS
- A [Telegram bot](https://core.telegram.org/bots#creating-a-new-bot) created via [@BotFather](https://t.me/BotFather)
- A publicly reachable HTTPS URL for webhooks (e.g. [ngrok](https://ngrok.com/) for local development)

## Configuration

Copy the placeholder values in `VirtualPhone/appsettings.json` and fill in your credentials:

| Key | Description |
|---|---|
| `Twilio:AccountSid` | Your Twilio Account SID (from the [Twilio Console](https://console.twilio.com/)) |
| `Twilio:AuthToken` | Your Twilio Auth Token |
| `Twilio:FromNumber` | Your Twilio phone number in E.164 format (e.g. `+15551234567`) |
| `Twilio:ToNumber` | The phone number that Telegram messages are forwarded to as SMS |
| `Telegram:BotToken` | Bot token provided by @BotFather |
| `Telegram:ChatId` | Numeric ID of the Telegram chat that receives forwarded SMS messages |

> **Security:** Never commit real credentials. Use environment variables or [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) in development and environment variables / a secrets manager in production.

### Finding your Telegram Chat ID

1. Start a conversation with your bot.
2. Send it any message.
3. Call `https://api.telegram.org/bot<YOUR_BOT_TOKEN>/getUpdates` in a browser.
4. Look for `"chat":{"id":...}` in the response.

## Running locally

```bash
cd VirtualPhone
dotnet run
```

Use [ngrok](https://ngrok.com/) (or any tunneling tool) to expose the local server:

```bash
ngrok http 5000
```

## Registering the webhooks

### Twilio

1. Go to the [Twilio Console](https://console.twilio.com/) → Phone Numbers → your number.
2. Under **Messaging Configuration**, set the webhook URL for incoming messages to:
   ```
   https://<your-host>/sms/incoming
   ```
   Method: **HTTP POST**

### Telegram

Register your bot's webhook with a single API call (replace placeholders):

```bash
curl -X POST "https://api.telegram.org/bot<YOUR_BOT_TOKEN>/setWebhook" \
     -d "url=https://<your-host>/telegram/webhook"
```

## Building

```bash
dotnet build VirtualPhone/VirtualPhone.csproj
```

## Project structure

```
VirtualPhone/
├── Controllers/
│   ├── SmsController.cs        # Twilio webhook → forward SMS to Telegram
│   └── TelegramController.cs   # Telegram webhook → send SMS via Twilio
├── Services/
│   ├── TwilioService.cs        # Twilio REST API wrapper
│   └── TelegramService.cs      # Telegram Bot API wrapper
├── Program.cs
└── appsettings.json
```
