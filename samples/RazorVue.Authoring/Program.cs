using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;

namespace RazorVue.Authoring;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = JazorWebApplication.CreateBuilder(args);
        builder.AddJazorFrontend(options =>
        {
            options.PathBase = builder.Configuration["Authoring:PathBase"] ?? string.Empty;
            options.Vite.ServerOrigin = new Uri(
                builder.Configuration["Authoring:JavaScriptServer"] ?? JazorViteServerOptions.DefaultServerOrigin.AbsoluteUri);
            var configuredRoot = builder.Configuration["Authoring:JazorRoot"];
            if (!string.IsNullOrWhiteSpace(configuredRoot))
                options.ProjectRootPath = configuredRoot;
        });

        var app = builder.Build();
        app.UseJazorPathBase();
        app.UseJazorFrontend();
        app.UseJazorSpaFallback(AuthoringHostShell.WriteAsync);
        app.Run();
    }
}
