using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;

namespace RazorVue.Authoring;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = JazorWebApplication.CreateBuilder(args);
        builder.Services.AddJazorReload();

        var app = builder.Build();
        var pathBase = builder.Configuration["Authoring:PathBase"];
        if (!string.IsNullOrWhiteSpace(pathBase))
        {
            if (!pathBase.StartsWith('/', StringComparison.Ordinal))
                throw new InvalidOperationException("Authoring:PathBase must start with '/'.");

            app.UsePathBase(pathBase.EndsWith('/', StringComparison.Ordinal) && pathBase.Length > 1
                ? pathBase[..^1]
                : pathBase);
        }

        app.UseJazorHost(options =>
        {
            var configuredRoot = builder.Configuration["Authoring:JazorRoot"];
            if (!string.IsNullOrWhiteSpace(configuredRoot))
                options.Assets.ConfigureArtifacts = artifact => artifact.RootPath = ResolveArtifactRoot(app.Environment.ContentRootPath, configuredRoot);
        });
        app.UseJazorReload();
        app.UseJazorSpaFallback(AuthoringHostShell.WriteAsync);
        app.Run();
    }

    private static string ResolveArtifactRoot(string contentRoot, string configuredRoot)
        => Path.GetFullPath(Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(contentRoot, configuredRoot));
}
