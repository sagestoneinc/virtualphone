using Microsoft.AspNetCore.HttpOverrides;
using VirtualPhone.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<TwilioService>();
builder.Services.AddSingleton<TelegramService>();

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownProxies.Clear();
forwardedHeadersOptions.KnownIPNetworks.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
