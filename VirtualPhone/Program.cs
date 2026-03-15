using VirtualPhone.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<TwilioService>();
builder.Services.AddSingleton<TelegramService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
