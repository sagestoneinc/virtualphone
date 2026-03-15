using Microsoft.AspNetCore.HttpOverrides;
using VirtualPhone.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<TwilioService>();
builder.Services.AddSingleton<TelegramService>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
