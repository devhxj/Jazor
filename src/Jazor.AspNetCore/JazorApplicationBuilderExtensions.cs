using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Jazor.AspNetCore;

public static class JazorApplicationBuilderExtensions
{
    private const string SourceMapContentType = "application/json";

    public static IApplicationBuilder UseJazorStaticFiles(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseJazorStaticFiles(new StaticFileOptions());
    }

    public static IApplicationBuilder UseJazorStaticFiles(
        this IApplicationBuilder app,
        Action<StaticFileOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new StaticFileOptions();
        configure(options);
        return app.UseJazorStaticFiles(options);
    }

    public static IApplicationBuilder UseJazorStaticFiles(
        this IApplicationBuilder app,
        StaticFileOptions options)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);

        options.ContentTypeProvider = WrapContentTypeProvider(options.ContentTypeProvider);

        app.UseStaticFiles(options);
        return app;
    }

    public static IApplicationBuilder UseJazorDevelopmentAssets(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseJazorDevelopmentAssets(configure: (Action<JazorDevelopmentAssetOptions>?)null);
    }

    public static IApplicationBuilder UseJazorDevelopmentAssets(
        this IApplicationBuilder app,
        Action<JazorDevelopmentAssetOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = new JazorDevelopmentAssetOptions();
        configure?.Invoke(options);
        return app.UseJazorDevelopmentAssets(options);
    }

    public static IApplicationBuilder UseJazorDevelopmentAssets(
        this IApplicationBuilder app,
        JazorDevelopmentAssetOptions options)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);

        if (!TryCreateMountContext(app, options, out var mountContext))
            return app;

        var staticFileOptions = new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(mountContext.OutputRootPath),
            RequestPath = mountContext.RequestPath,
            ContentTypeProvider = CreateContentTypeProvider()
        };
        if (mountContext.OnPrepareResponse is not null)
            staticFileOptions.OnPrepareResponse = mountContext.OnPrepareResponse;

        app.UseJazorStaticFiles(staticFileOptions);

        if (mountContext.ReturnNotFoundWhenMountedPathMisses)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments(mountContext.RequestPath))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                await next();
            });
        }

        return app;
    }

    private static bool TryCreateMountContext(
        IApplicationBuilder app,
        JazorDevelopmentAssetOptions options,
        out JazorDevelopmentAssetMountContext mountContext)
    {
        var environment = app.ApplicationServices.GetService<IWebHostEnvironment>()
            ?? throw new InvalidOperationException("Jazor development assets require IWebHostEnvironment from the ASP.NET Core host.");

        var requestPath = NormalizeRequestPath(options.RequestPath);
        var outputRootPath = ResolveDevelopmentOutputRootPath(environment.ContentRootPath, options);
        var entryModulePath = ResolveEntryModulePath(outputRootPath, options.EntryModuleRelativePath);
        if (!File.Exists(entryModulePath))
        {
            mountContext = null!;
            return false;
        }

        mountContext = new JazorDevelopmentAssetMountContext(
            requestPath,
            outputRootPath,
            options.OnPrepareResponse,
            options.ReturnNotFoundWhenMountedPathMisses);
        return true;
    }

    private static PathString NormalizeRequestPath(PathString requestPath)
    {
        if (!requestPath.HasValue || string.IsNullOrWhiteSpace(requestPath.Value))
            throw new ArgumentException("Jazor development asset request path is required.", nameof(requestPath));

        if (!requestPath.Value.StartsWith('/'))
            throw new ArgumentException("Jazor development asset request path must start with '/'.", nameof(requestPath));

        if (string.Equals(requestPath.Value, "/", StringComparison.Ordinal))
            throw new ArgumentException("Jazor development asset request path cannot be the application root.", nameof(requestPath));

        return requestPath;
    }

    private static FileExtensionContentTypeProvider CreateContentTypeProvider()
    {
        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings[".map"] = SourceMapContentType;
        return provider;
    }

    private static IContentTypeProvider WrapContentTypeProvider(IContentTypeProvider? contentTypeProvider)
    {
        var innerProvider = contentTypeProvider ?? CreateContentTypeProvider();

        if (innerProvider is FileExtensionContentTypeProvider fileExtensionProvider)
        {
            fileExtensionProvider.Mappings[".map"] = SourceMapContentType;
            return fileExtensionProvider;
        }

        return new SourceMapAwareContentTypeProvider(innerProvider);
    }

    private static string ResolveDevelopmentOutputRootPath(string contentRootPath, JazorDevelopmentAssetOptions options)
    {
        var configuredPath = options.DevelopmentOutputRootPath;
        if (!string.IsNullOrWhiteSpace(configuredPath))
            return GetFullPath(contentRootPath, configuredPath);

        if (string.IsNullOrWhiteSpace(options.DevelopmentOutputDirectoryName))
            throw new ArgumentException("Jazor development output directory name is required when no explicit root path is configured.", nameof(options));

        return Path.GetFullPath(Path.Combine(contentRootPath, options.DevelopmentOutputDirectoryName));
    }

    private static string ResolveEntryModulePath(string outputRootPath, string entryModuleRelativePath)
    {
        if (string.IsNullOrWhiteSpace(entryModuleRelativePath))
            throw new ArgumentException("Jazor development entry module path is required.", nameof(entryModuleRelativePath));

        if (Path.IsPathRooted(entryModuleRelativePath))
            throw new ArgumentException("Jazor development entry module path must be relative.", nameof(entryModuleRelativePath));

        var candidatePath = Path.GetFullPath(Path.Combine(outputRootPath, entryModuleRelativePath));
        if (!IsPathWithinRoot(outputRootPath, candidatePath))
            throw new ArgumentException("Jazor development entry module path must stay within the configured output root.", nameof(entryModuleRelativePath));

        return candidatePath;
    }

    private static string GetFullPath(string contentRootPath, string configuredPath)
    {
        var resolvedPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(contentRootPath, configuredPath);
        return Path.GetFullPath(resolvedPath);
    }

    private static bool IsPathWithinRoot(string rootPath, string candidatePath)
    {
        var normalizedRootPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        var normalizedCandidatePath = Path.GetFullPath(candidatePath);

        return normalizedCandidatePath.StartsWith(
            normalizedRootPath + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedCandidatePath, normalizedRootPath, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class SourceMapAwareContentTypeProvider(IContentTypeProvider innerProvider) : IContentTypeProvider
    {
        public bool TryGetContentType(string subpath, out string contentType)
        {
            if (subpath.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
            {
                contentType = SourceMapContentType;
                return true;
            }

            return innerProvider.TryGetContentType(subpath, out contentType!);
        }
    }

    private sealed record JazorDevelopmentAssetMountContext(
        PathString RequestPath,
        string OutputRootPath,
        Action<StaticFileResponseContext>? OnPrepareResponse,
        bool ReturnNotFoundWhenMountedPathMisses);
}
