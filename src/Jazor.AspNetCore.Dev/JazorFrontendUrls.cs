using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

/// <summary>Builds browser URLs from the same options used by Vite, proxying, and release artifact hosting.</summary>
public static class JazorFrontendUrls
{
    /// <summary>Returns the configured Vite browser-client URL.</summary>
    public static string GetDevelopmentClient(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var options = context.RequestServices.GetRequiredService<IOptions<JazorFrontendOptions>>().Value;
        return options.BuildPublicUrl(Jazor.Common.JazorArtifactDefaults.DevelopmentClientRelativePath);
    }

    /// <summary>Returns the configured development entry or release bundle URL for the current host environment.</summary>
    public static string GetBrowserEntry(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var options = context.RequestServices.GetRequiredService<IOptions<JazorFrontendOptions>>().Value;
        var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();
        return options.BuildPublicUrl(environment.IsDevelopment()
            ? options.DevelopmentEntryRelativePath
            : options.ReleaseEntryRelativePath);
    }

    /// <summary>Returns the Release entry's stylesheets in dependency order; Development styles are loaded by Vite.</summary>
    /// <remarks>Reads the standard build's manifest.json beside the Release entry, once per host. Missing or invalid build metadata is propagated, not replaced by a directory scan.</remarks>
    public static IReadOnlyList<string> GetStylesheets(HttpContext context)
        => context.RequestServices.GetRequiredService<JazorFrontendAssets>().Stylesheets;
}
