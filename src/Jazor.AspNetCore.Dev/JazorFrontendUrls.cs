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
}
