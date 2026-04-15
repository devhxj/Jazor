using Jazor.VueHost.Frontend.Deno.Hosting;
using System.Text.RegularExpressions;

namespace Jazor.VueHost.DevServer;

internal sealed class DenoFrontendModuleCompiler : IFrontendModuleCompiler
{
    private static readonly Regex JavaScriptDependencyPattern = new(
        @"\bimport\s+(?:[^'"";]*?\s+from\s*)?[""'](?<specifier>[^""']+)[""']|\bexport\s+[^'"";]*?\s+from\s*[""'](?<specifier>[^""']+)[""']|\bimport\s*\(\s*[""'](?<specifier>[^""']+)[""']\s*\)",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private readonly IDenoVolarHost _host;

    public DenoFrontendModuleCompiler(IDenoVolarHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public async ValueTask<FrontendModuleCompilation?> CompileSfcAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken)
    {
        var result = await _host.CompileSfcAsync(
            documentPath,
            text,
            Path.GetFileName(documentPath),
            cancellationToken);
        if (result is null)
        {
            return null;
        }

        return new FrontendModuleCompilation
        {
            JavaScript = result.JsContent,
            StyleContent = result.CssContent,
            Dependencies = ExtractJavaScriptDependencies(result.JsContent),
            SourceMap = null,
            SupportsHmr = result.SupportsHmr
        };
    }

    public ValueTask<FrontendModuleCompilation?> CompileTypeScriptAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken)
        => CompileTypeScriptCoreAsync(documentPath, text, cancellationToken);

    private async ValueTask<FrontendModuleCompilation?> CompileTypeScriptCoreAsync(
        string documentPath,
        string text,
        CancellationToken cancellationToken)
    {
        var result = await _host.CompileTypeScriptAsync(
            documentPath,
            text,
            Path.GetFileName(documentPath),
            cancellationToken);
        if (result is null)
        {
            return null;
        }

        return new FrontendModuleCompilation
        {
            JavaScript = result.JsContent,
            StyleContent = null,
            Dependencies = ExtractJavaScriptDependencies(result.JsContent),
            SourceMap = null
        };
    }

    internal static IReadOnlyList<string> ExtractJavaScriptDependencies(string javaScript)
    {
        ArgumentNullException.ThrowIfNull(javaScript);

        var dependencies = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in JavaScriptDependencyPattern.Matches(javaScript))
        {
            var specifier = match.Groups["specifier"].Value;
            if (specifier.Length == 0 || !seen.Add(specifier))
            {
                continue;
            }

            dependencies.Add(specifier);
        }

        return dependencies;
    }
}
