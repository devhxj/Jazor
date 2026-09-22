using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;

namespace BindingsShowcase.Host;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = JazorWebApplication.CreateBuilder(args);
        builder.Services.AddJazorViteProxy(options => options.ServerOrigin = new Uri(builder.Configuration["Showcase:JavaScriptServer"] ?? "http://127.0.0.1:5173"));

        var app = builder.Build();
        var pathBase = builder.Configuration["Showcase:PathBase"];
        if (!string.IsNullOrWhiteSpace(pathBase))
        {
            if (!pathBase.StartsWith('/', StringComparison.Ordinal))
                throw new InvalidOperationException("Showcase:PathBase must start with '/'.");

            app.UsePathBase(pathBase.EndsWith('/', StringComparison.Ordinal) && pathBase.Length > 1
                ? pathBase[..^1]
                : pathBase);
        }

        app.UseJazorHost(options =>
        {
            var configuredRoot = builder.Configuration["Showcase:JazorRoot"];
            if (!string.IsNullOrWhiteSpace(configuredRoot))
                options.Assets.ConfigureArtifacts = artifact => artifact.RootPath = ResolveArtifactRoot(app.Environment.ContentRootPath, configuredRoot);
        });
        app.UseJazorViteProxy();
        app.UseJazorSpaFallback(ShowcaseHostShell.WriteAsync);
        app.Run();
    }

    private static string ResolveArtifactRoot(string contentRoot, string configuredRoot)
        => Path.GetFullPath(Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(contentRoot, configuredRoot));
}
