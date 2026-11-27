using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

/// <summary>Loads the bundler's static CSS closure for the configured browser entry.</summary>
internal sealed class JazorSPAAssets
{
    private readonly Lazy<IReadOnlyList<string>> _stylesheets;

    public JazorSPAAssets(IOptions<JazorSPAOptions> options, IWebHostEnvironment environment)
    {
        // Vite writes the stylesheet links into an HTML entry itself.
        _stylesheets = new(() => environment.IsDevelopment() || options.Value.IsJoltApp
            ? Array.Empty<string>()
            : ReadStylesheets(options.Value, environment));
    }

    public IReadOnlyList<string> Stylesheets => _stylesheets.Value;

    private static IReadOnlyList<string> ReadStylesheets(JazorSPAOptions options, IWebHostEnvironment environment)
    {
        var releasePath = options.ReleaseEntryRelativePath.Replace('\\', '/');
        var manifestPath = Path.Combine(options.ResolveReleaseRoot(environment.ContentRootPath, environment.WebRootPath), "manifest.json");
        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var chunks = manifest.RootElement;
        var entry = chunks.EnumerateObject().Single(chunk =>
            chunk.Value.GetProperty("file").GetString() == Path.GetFileName(releasePath));
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var styles = new HashSet<string>(StringComparer.Ordinal);
        var urls = new List<string>();

        // Static imports must precede their importing entry's CSS. Dynamic imports own their
        // styles at load time; preloading them here would change cascade/order and defeat splitting.
        // manifest 是唯一资源清单，visited 也保持共享依赖与循环图的确定性。
        void Visit(string key)
        {
            if (!visited.Add(key))
                return;
            var chunk = chunks.GetProperty(key);
            if (chunk.TryGetProperty("imports", out var imports))
                foreach (var import in imports.EnumerateArray())
                    Visit(import.GetString()!);
            if (chunk.TryGetProperty("css", out var css))
                foreach (var style in css.EnumerateArray())
                {
                    var path = style.GetString()!;
                    if (styles.Add(path))
                        urls.Add(options.BuildPublicUrl(path));
                }
        }

        Visit(entry.Name);
        return urls.ToArray();
    }
}
