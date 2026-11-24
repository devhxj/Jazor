using Jazor.AspNetCore.Dev;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    // Hosted services start in registration order. Wait for Jolt before the frontend
    // service runs its shared readiness check and configures the existing proxy.
    builder.Services.AddHostedService<JoltDevSession>();
}
builder.AddJazorFrontend(options =>
{
    options.ProjectRootPath = builder.Environment.IsDevelopment() ? "frontend" : "frontend/dist";
    options.PathBase = "/portal";
    options.RequestPath = "/frontend";
    options.DevelopmentEntryRelativePath = "index.html";
    options.ReleaseEntryRelativePath = "index.html";
    options.Vite.ServerOrigin = new Uri("http://127.0.0.1:15658");
    options.Vite.LaunchServer = false;
});

var app = builder.Build();
app.UseJazorPathBase();
app.UseJazorFrontend();
app.MapGet("/", () => Results.Redirect("/portal/frontend/index.html"));
await app.RunAsync();
