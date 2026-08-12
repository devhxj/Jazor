using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;

namespace Jazor.AspNetCore;

/// <summary>Creates Jazor web hosts with a source- and publish-layout-aware content root.</summary>
public static class JazorWebApplication
{
    /// <summary>Creates a builder whose content root works for source and publish layouts.</summary>
    public static WebApplicationBuilder CreateBuilder(
        string[] args,
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentNullException.ThrowIfNull(args);

        return WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = ResolveContentRootPath(AppContext.BaseDirectory, sourceFilePath)
        });
    }

    internal static string ResolveContentRootPath(string appBaseDirectory, string sourceFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appBaseDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFilePath);

        appBaseDirectory = Path.GetFullPath(appBaseDirectory);
        // A publish directory owns its generated jazor/ graph and must win even when the
        // original source tree is still present on the machine. An empty copied jazor/
        // directory is not a ready artifact graph.
        if (HasReadyArtifactGraph(appBaseDirectory))
            return appBaseDirectory;

        var sourceDirectory = Path.GetDirectoryName(sourceFilePath)
            ?? throw new InvalidOperationException("Cannot determine Jazor web application content root.");

        // Debug output copies wwwroot into bin/, but JazorDir intentionally remains beside
        // the project. Prefer that source artifact graph over the copied web root so the
        // host, reload service, and SPA shell all observe the same generated modules.
        if (HasReadyArtifactGraph(sourceDirectory))
            return sourceDirectory;

        // Non-Jazor web hosts can still use a conventional copied wwwroot output.
        return Directory.Exists(Path.Combine(appBaseDirectory, "wwwroot"))
            ? appBaseDirectory
            : sourceDirectory;
    }

    private static bool HasReadyArtifactGraph(string rootPath)
    {
        var artifactRoot = Path.Combine(rootPath, "jazor");
        return File.Exists(Path.Combine(artifactRoot, "jazor-manifest.json")) ||
               File.Exists(Path.Combine(artifactRoot, "bundle.js"));
    }
}
