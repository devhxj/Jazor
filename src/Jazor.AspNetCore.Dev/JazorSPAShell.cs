using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Jazor.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Jazor.AspNetCore.Dev;

internal static partial class JazorSPAShell
{
    private static readonly PathString AssetPath = new("/assets");

    internal static void Use(IApplicationBuilder app, JazorSPAOptions options, IWebHostEnvironment environment)
    {
        var development = environment.IsDevelopment();
        var mount = options.RequestPath;
        // The prefixed development proxy owns the entire mount, including Vite's HTML handling.
        if (development && mount.HasValue)
            return;

        var releaseFile = development
            ? null
            : Path.Combine(options.ResolveReleaseRoot(environment.ContentRootPath, environment.WebRootPath), options.GetReleaseEntryName());
        var indexPath = mount.Add(new PathString(development ? "/index.html" : "/" + Path.GetFileName(releaseFile!)));
        ShellTemplate? releaseTemplate = null;

        app.Use(async (context, next) =>
        {
            if (context.GetEndpoint() is null &&
                (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method)) &&
                string.Equals(context.Request.Path.Value, indexPath.Value, StringComparison.OrdinalIgnoreCase))
            {
                await WriteShellAsync(context, context.RequestAborted);
                return;
            }

            await next();
        });

        if (releaseFile is not null && Path.GetDirectoryName(releaseFile) is { } releaseRoot && Directory.Exists(releaseRoot))
        {
            var files = new PhysicalFileProvider(releaseRoot);
            app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>().ApplicationStopped.Register(files.Dispose);
            var assetPrefix = mount.Add(AssetPath);
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = mount,
                FileProvider = files,
                OnPrepareResponse = response =>
                {
                    var context = response.Context;
                    // Rollup's default hash suffix identifies versioned Vite output. Public
                    // files with stable names remain revalidatable, even under /assets.
                    var immutable = context.Request.Path.StartsWithSegments(assetPrefix) &&
                        VersionedAssetName().IsMatch(response.File.Name);
                    context.Response.Headers.CacheControl = immutable
                        ? "public, max-age=31536000, immutable"
                        : "no-cache, must-revalidate";
                }
            });
        }

        var developmentPath = options.GetDevelopmentRequestPath();
        // Compare mount-relative exclusions on the original request path so downstream routing sees it unchanged.
        var fallback = new JazorSpaFallbackOptions { RequireHtmlAcceptHeader = options.Fallback.RequireHtmlAcceptHeader };
        fallback.ExcludedPathPrefixes.Clear();
        foreach (var prefix in options.Fallback.ExcludedPathPrefixes)
            fallback.ExcludedPathPrefixes.Add(mount.Add(prefix));
        fallback.AllowedPathSuffixes.Clear();
        foreach (var suffix in options.Fallback.AllowedPathSuffixes)
            fallback.AllowedPathSuffixes.Add(suffix);

        app.UseWhen(
            context => mount.HasValue
                ? context.Request.Path.StartsWithSegments(mount, out var relative) &&
                    !relative.StartsWithSegments(AssetPath)
                : !context.Request.Path.StartsWithSegments(developmentPath),
            branch => branch.UseJazorSpaFallback(WriteShellAsync, fallback));

        async Task WriteShellAsync(HttpContext context, CancellationToken cancellationToken)
        {
            var head = HttpMethods.IsHead(context.Request.Method);
            ShellTemplate? template = null;
            if (development)
            {
                using var client = context.RequestServices.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(JazorViteDevelopmentServer.HttpClientName);
                using var request = new HttpRequestMessage(head ? HttpMethod.Head : HttpMethod.Get, options.GetReadyUri());
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    context.Response.StatusCode = (int)response.StatusCode;
                    return;
                }

                if (!head)
                    template = ShellTemplate.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            }
            else
            {
                template = Volatile.Read(ref releaseTemplate);
                if (template is null && !File.Exists(releaseFile))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                if (template is null && !head)
                {
                    // Cache only an available deployment template so absent output can still return 404.
                    template = ShellTemplate.Parse(await File.ReadAllTextAsync(releaseFile!, cancellationToken));
                    Volatile.Write(ref releaseTemplate, template);
                }
            }

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.Headers.CacheControl = "no-cache, must-revalidate";
            if (template is not null && !head)
                await context.Response.WriteAsync(template.Render(context.Request.PathBase.Add(mount)), cancellationToken);
        }
    }

    private sealed record ShellTemplate(string Html, int BaseInsertionIndex)
    {
        internal static ShellTemplate Parse(string html)
        {
            // Preserve an author's explicit base, including its target attribute.
            var head = HeadElement().Match(html);
            return new ShellTemplate(html, head.Success && !ExistingBaseElement().IsMatch(html)
                ? head.Index + head.Length
                : -1);
        }

        internal string Render(PathString pathBase)
        {
            if (BaseInsertionIndex < 0)
                return Html;

            // Relative Vite assets and router links resolve from the mount even on deep links.
            var href = HtmlEncoder.Default.Encode(pathBase.ToUriComponent().TrimEnd('/') + "/");
            return Html.Insert(BaseInsertionIndex, $"<base href=\"{href}\">");
        }
    }

    [GeneratedRegex(@"-[a-zA-Z0-9_-]{8}\.[^.]+$")]
    private static partial Regex VersionedAssetName();

    [GeneratedRegex(@"<base\b[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex ExistingBaseElement();

    [GeneratedRegex(@"<head\b[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex HeadElement();
}
