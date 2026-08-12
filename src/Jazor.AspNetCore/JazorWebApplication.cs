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
        // A published Jazor host may have no authored wwwroot. Its generated jazor/
        // graph is still enough to identify the publish directory as the content root.
        if (Directory.Exists(Path.Combine(appBaseDirectory, "wwwroot")) ||
            Directory.Exists(Path.Combine(appBaseDirectory, "jazor")))
            return appBaseDirectory;

        return Path.GetDirectoryName(sourceFilePath)
            ?? throw new InvalidOperationException("Cannot determine Jazor web application content root.");
    }
}
