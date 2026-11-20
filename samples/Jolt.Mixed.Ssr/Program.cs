using Jazor.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddJoltSsr(Path.Combine(AppContext.BaseDirectory, "frontend", "dist"));

var app = builder.Build();
app.UseJoltSsr((http, _) =>
{
    var name = http.Request.Query["name"].FirstOrDefault() ?? "Guest";
    var props = new { failPrefetch = http.Request.Query["fail"] == "1" };
    return Task.FromResult(new JoltSsrRequest(
        Props: props,
        Providers: [new JazorSsrProvider("workspace:request-name", name)]));
});
app.Run();
