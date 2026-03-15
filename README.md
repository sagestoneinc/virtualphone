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

## GitHub Actions & GitHub Pages

Three workflows are included in `.github/workflows/`:

| File | Purpose |
|---|---|
| `ci.yml` | Builds the project automatically on every push and pull request to `main` |
| `pages.yml` | Deploys the static documentation site in `docs/` to **GitHub Pages** on every push to `main` |
| `cloudflare.yml` | Builds a Docker image, pushes it to GitHub Container Registry, and deploys the API to a server via SSH, exposed through **Cloudflare Tunnel** |

### Enabling GitHub Pages

> **Note:** GitHub Pages only serves *static* files, so it cannot run the ASP.NET Core API itself.
> The Pages deployment publishes the documentation site at `docs/index.html`.

1. Go to your repository on GitHub → **Settings** → **Pages**.
2. Under **Build and deployment**, select **Source: GitHub Actions**.
3. Push (or re-push) to `main` – the `pages.yml` workflow will deploy the docs automatically.

The live documentation will be available at:
```
https://<your-github-username>.github.io/virtualphone/
```

### Deploying the API to Cloudflare

The `cloudflare.yml` workflow containerises the API with Docker, pushes the image to the **GitHub Container Registry** (`ghcr.io`), and then SSH-deploys it to a server that exposes it through a [Cloudflare Tunnel](https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/).

#### Server prerequisites

1. Install Docker on your server.
2. Install and configure `cloudflared`:
   ```bash
   # Authenticate and create a tunnel
   cloudflared tunnel login
   cloudflared tunnel create virtualphone
   # Route your domain to the tunnel
   cloudflared tunnel route dns virtualphone <your-domain>
   # Run the tunnel (points to the container port)
   cloudflared tunnel --url http://localhost:8080 run virtualphone
   ```

#### GitHub repository secrets

Add the following secrets in **Settings → Secrets and variables → Actions**:

| Secret | Description |
|---|---|
| `SSH_HOST` | Hostname or IP address of your deployment server |
| `SSH_USER` | SSH username on the deployment server |
| `SSH_KEY` | Private SSH key that can authenticate to the server |
| `GHCR_TOKEN` | GitHub Personal Access Token (classic) with `read:packages` scope, used by the server to pull the image from GHCR |
| `TWILIO_ACCOUNT_SID` | Your Twilio Account SID |
| `TWILIO_AUTH_TOKEN` | Your Twilio Auth Token |
| `TWILIO_FROM_NUMBER` | Your Twilio phone number in E.164 format |
| `TWILIO_TO_NUMBER` | The phone number SMS replies are forwarded to |
| `TELEGRAM_BOT_TOKEN` | Bot token provided by @BotFather |
| `TELEGRAM_CHAT_ID` | Numeric ID of the Telegram chat |

Optionally, set the `DEPLOY_URL` **variable** (not secret) to the public URL of your deployment so the workflow environment shows a clickable link.

#### How it works

1. On every push to `main` (or via manual trigger), the workflow builds a Docker image from the `Dockerfile` at the repository root and pushes it to `ghcr.io/<owner>/virtualphone:latest`.
2. The `deploy` job SSHes into your server and runs `docker pull` followed by `docker run` to start the latest image on port `8080`.
3. Cloudflare Tunnel (running on the server) forwards external HTTPS traffic to `localhost:8080`, so your webhooks are always reachable at your configured domain without opening any firewall ports.

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
