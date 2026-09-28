using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;

namespace BindingsShowcase.Host;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = JazorWebApplication.CreateBuilder(args);
        builder.AddJazorFrontend(options =>
        {
            options.PathBase = builder.Configuration["Showcase:PathBase"] ?? string.Empty;
            options.Vite.ServerOrigin = new Uri(
                builder.Configuration["Showcase:JavaScriptServer"] ?? JazorViteServerOptions.DefaultServerOrigin.AbsoluteUri);
            var configuredRoot = builder.Configuration["Showcase:JazorRoot"];
            if (!string.IsNullOrWhiteSpace(configuredRoot))
                options.ProjectRootPath = configuredRoot;
        });

        var app = builder.Build();
        app.UseJazorPathBase();
        app.UseJazorFrontend();
        app.UseJazorSpaFallback(ShowcaseHostShell.WriteAsync);
        app.Run();
    }
}
