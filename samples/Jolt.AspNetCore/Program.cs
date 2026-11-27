using Jazor.AspNetCore.Dev;

var builder = WebApplication.CreateBuilder(args);
builder.AddJazorSPA(options =>
{
    options.PathBase = "/portal";
    options.RequestPath = "/frontend";
    options.Vite.ServerOrigin = new Uri("http://127.0.0.1:15658");
});

var app = builder.Build();
app.UseJazorPathBase();
app.UseJazorSPA();
app.MapGet("/", () => Results.Redirect("/portal/frontend/index.html"));
await app.RunAsync();
