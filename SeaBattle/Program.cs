using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.ResponseCompression;
using SeaBattle;
using SeaBattle.Hubs;
using Serilog;

Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.File("logs/seabattle.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options => options.DetailedErrors = true)
    .AddHubOptions(options =>
     {
         options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
         options.HandshakeTimeout = TimeSpan.FromSeconds(30);
     });
builder.Services.AddResponseCompression(o =>
{
#pragma warning disable CA1861 // Avoid constant arrays as arguments
    o.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
#pragma warning restore CA1861 // Avoid constant arrays as arguments
});
builder.Services.AddSignalR().AddJsonProtocol();
builder.Services.AddScoped<ICircuitBroker, CircuitBroker>();
builder.Services.AddScoped<CircuitHandler>(x => CircuitHandlerService.GetCircuitHandlerService(x.GetService<ICircuitBroker>()!));

var app = builder.Build();

app.UseResponseCompression();
app.MapHub<GameHub>("gamehub");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
