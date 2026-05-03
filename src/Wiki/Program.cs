var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/health", () => Results.Ok("ok"));
app.MapFallbackToFile("index.html");

app.Run();
