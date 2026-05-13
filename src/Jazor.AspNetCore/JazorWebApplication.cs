using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;

namespace Jazor.AspNetCore;

public static class JazorWebApplication
{
    public static WebApplicationBuilder CreateBuilder(
        string[] args,
        [CallerFilePath] string sourceFilePath = "")
    {
        ArgumentNullException.ThrowIfNull(args);

        return WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = ResolveContentRootPath(sourceFilePath)
        });
    }

    private static string ResolveContentRootPath(string sourceFilePath)
    {
        var appBaseDirectory = Path.GetFullPath(AppContext.BaseDirectory);
        if (Directory.Exists(Path.Combine(appBaseDirectory, "wwwroot")))
            return appBaseDirectory;

        return Path.GetDirectoryName(sourceFilePath)
            ?? throw new InvalidOperationException("Cannot determine Jazor web application content root.");
    }
}
