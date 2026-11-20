using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Jazor.AspNetCore;

/// <summary>Request-scoped values passed to the Jolt server entry and browser hydration.</summary>
/// <param name="Props">Serializable props for the root component.</param>
/// <param name="Providers">Serializable Vue provider values.</param>
public sealed record JoltSsrRequest(
    object? Props = null,
    IReadOnlyList<JazorSsrProvider>? Providers = null);

/// <summary>Rendered Vue fragments and the final hydration state.</summary>
/// <param name="Html">HTML for the root mount element.</param>
/// <param name="Teleports">HTML fragments keyed by their Vue Teleport selectors.</param>
/// <param name="SerializedState">The versioned JSON envelope after server prefetch.</param>
public sealed record JoltSsrRenderResult(
    string Html,
    IReadOnlyDictionary<string, string> Teleports,
    string SerializedState);

/// <summary>Executes the Jolt production server entry in a pooled NodeApi environment.</summary>
public interface IJoltSsrRenderer
{
    /// <summary>Renders a single application-relative URL, including its query string.</summary>
    Task<JoltSsrRenderResult> RenderAsync(
        string url,
        JoltSsrRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>Hosts the client and server artifacts produced by a Jolt SSR build.</summary>
public static class JoltSsrExtensions
{
    private const string IndexPath = "/index.html";

    /// <summary>Registers a production distribution directory and its SSR renderer.</summary>
    /// <remarks>Relative paths resolve from the ASP.NET Core content root. Restart the host when deploying a new distribution.</remarks>
    public static IServiceCollection AddJoltSsr(
        this IServiceCollection services,
        string distributionDirectory,
        int? workerCount = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(distributionDirectory);
        services.AddSingleton(serviceProvider => new JoltSsrDistribution(
            Path.GetFullPath(distributionDirectory, serviceProvider.GetRequiredService<IWebHostEnvironment>().ContentRootPath)));
        services.AddSingleton<IJoltSsrRenderer>(serviceProvider => new JoltSsrRenderer(
            serviceProvider.GetRequiredService<JoltSsrDistribution>(),
            workerCount ?? Math.Clamp(Environment.ProcessorCount, 1, 4)));
        return services;
    }

    /// <summary>Serves built assets and renders eligible GET and HEAD navigation requests.</summary>
    /// <remarks>
    /// Configure ASP.NET Core PathBase before this middleware and build with the
    /// corresponding Vite base. The request factory runs for each rendered GET.
    /// </remarks>
    public static IApplicationBuilder UseJoltSsr(
        this IApplicationBuilder app,
        Func<HttpContext, CancellationToken, Task<JoltSsrRequest>> requestFactory,
        Action<JazorSpaFallbackOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(requestFactory);
        var distribution = app.ApplicationServices.GetRequiredService<JoltSsrDistribution>();
        var renderer = app.ApplicationServices.GetRequiredService<IJoltSsrRenderer>();

        // index.html is the server template. Only the rendered document is served.
        app.UseWhen(
            static context => !context.Request.Path.Equals(IndexPath, StringComparison.OrdinalIgnoreCase),
            branch => branch.UseStaticFiles(new StaticFileOptions { FileProvider = distribution.ClientFiles }));

        return app.UseJazorSpaFallback(async (context, cancellationToken) =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            if (HttpMethods.IsHead(context.Request.Method))
                return;

            var request = await requestFactory(context, cancellationToken).ConfigureAwait(false);
            var path = context.Request.Path.HasValue ? context.Request.Path : new PathString("/");
            var url = UriHelper.BuildRelative(path: path, query: context.Request.QueryString);
            var result = await renderer.RenderAsync(url, request, cancellationToken).ConfigureAwait(false);
            await context.Response.WriteAsync(distribution.Compose(result), cancellationToken).ConfigureAwait(false);
        }, configure);
    }
}

internal sealed partial class JoltSsrDistribution : IDisposable
{
    private readonly string _index;
    private readonly HashSet<string> _teleportTargets = new(StringComparer.Ordinal);

    public JoltSsrDistribution(string path)
    {
        var clientDirectory = Path.Combine(path, "client");
        var indexPath = Path.Combine(clientDirectory, "index.html");
        _index = File.ReadAllText(indexPath);
        var serverDirectory = Path.Combine(path, "server");
        var serverEntry = Path.Combine(serverDirectory, "entry-server.mjs");
        if (!File.Exists(serverEntry))
            throw new FileNotFoundException("Jolt SSR server entry was not found.", serverEntry);
        Artifacts = new SsrArtifacts(serverDirectory, serverEntry, string.Empty, IsBundled: true, IsJolt: true);

        var matches = SlotPattern().Matches(_index);
        if (matches.Count(static match => match.Groups[1].Value == "outlet") != 1 ||
            matches.Count(static match => match.Groups[1].Value == "state") != 1)
            throw new InvalidOperationException("Jolt SSR index.html must have one <!--jolt-ssr-outlet--> and one <!--jolt-ssr-state-->.");
        foreach (var match in matches.Cast<Match>().Where(static match => match.Groups[2].Success))
        {
            var selector = match.Groups[2].Value;
            if (!_teleportTargets.Add(selector))
                throw new InvalidOperationException($"Jolt SSR index.html has duplicate Teleport slots for '{selector}'.");
        }

        ClientFiles = new PhysicalFileProvider(clientDirectory);
    }

    public SsrArtifacts Artifacts { get; }
    public PhysicalFileProvider ClientFiles { get; }

    public string Compose(JoltSsrRenderResult result)
    {
        foreach (var selector in result.Teleports.Keys)
        {
            if (!_teleportTargets.Contains(selector))
                throw new InvalidOperationException($"Jolt SSR index.html is missing <!--jolt-ssr-teleport:{selector}-->.");
        }

        // A single pass over the built template preserves Vite URLs and keeps
        // arbitrary text produced by Vue from becoming a second substitution.
        return SlotPattern().Replace(_index, match => match.Groups[1].Value switch
        {
            "outlet" => result.Html,
            "state" => "<script id=\"__jazor_ssr_state\" type=\"application/json\">" + result.SerializedState + "</script>",
            _ => result.Teleports.GetValueOrDefault(match.Groups[2].Value, string.Empty)
        });
    }

    public void Dispose() => ClientFiles.Dispose();

    [GeneratedRegex("<!--jolt-ssr-(outlet|state|teleport:(.*?))-->")]
    private static partial Regex SlotPattern();
}

internal sealed class JoltSsrRenderer(JoltSsrDistribution distribution, int workerCount) : IJoltSsrRenderer, IAsyncDisposable
{
    private readonly SsrRenderer _renderer = new(() => distribution.Artifacts, workerCount);

    public async Task<JoltSsrRenderResult> RenderAsync(
        string url,
        JoltSsrRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        var options = SsrRenderer.JsonOptions;
        var state = JazorSsrStateEnvelope.Create(request.Props, request.Providers, authentication: null);
        var response = await _renderer.RenderCoreAsync(
            modulePath: null, JsonSerializer.SerializeToElement(state, options), url, cancellationToken).ConfigureAwait(false);
        var result = JsonSerializer.Deserialize<RenderResponse>(response, options);
        if (result?.Html is null || result.State.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Jolt SSR entry must render an HTML string and preserve the state envelope.");

        // JavaScript may update state during prefetch. Encode the resulting JSON
        // for a script element using the same encoder as the Jazor SSR path.
        return new JoltSsrRenderResult(
            result.Html,
            result.Teleports ?? new Dictionary<string, string>(),
            JsonSerializer.Serialize(result.State, options));
    }

    public ValueTask DisposeAsync() => _renderer.DisposeAsync();

    private sealed record RenderResponse(
        [property: JsonPropertyName("html")] string? Html,
        [property: JsonPropertyName("teleports")] Dictionary<string, string>? Teleports,
        [property: JsonPropertyName("state")] JsonElement State);
}
