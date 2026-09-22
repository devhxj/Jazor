using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;

namespace Jazor.AspNetCore;

/// <summary>Creates Jazor web hosts with a source- and publish-layout-aware content root.</summary>
public static class JazorWebApplication
{
    /// <summary>Creates a builder whose content root works for source and publish layouts.</summary>
    /// <remarks>优先使用带标准 entry.js 的应用输出目录（兼容性 fallback 仍受支持），其次调用源文件目录；否则使用含 wwwroot 的输出目录或源文件目录。建议从 Program.cs 直接调用以保留正确的 CallerFilePath。本方法不注册 Jazor 服务或中间件。</remarks>
    /// <param name="args">宿主命令行参数。</param>
    /// <param name="sourceFilePath">调用源文件路径，通常由编译器自动填入，用于定位项目目录。</param>
    /// <returns>已选择 content root 的 WebApplicationBuilder。</returns>
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
        return File.Exists(Path.Combine(artifactRoot, JazorArtifactOptions.EntryProbeRelativePath)) ||
               File.Exists(Path.Combine(artifactRoot, JazorArtifactOptions.BundleProbeRelativePath));
    }
}
