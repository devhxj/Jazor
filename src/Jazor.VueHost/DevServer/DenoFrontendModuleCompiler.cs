using Jazor.VueHost.Frontend.Deno.Hosting;
using Jazor.VueHost.Frontend.Deno.Protocol;
using System.Text.RegularExpressions;

namespace Jazor.VueHost.DevServer;

internal sealed class DenoFrontendModuleCompiler : IFrontendModuleCompiler
{
    private static readonly Regex JavaScriptDependencyPattern = new(
        @"\bimport\s+(?:[^'"";]*?\s+from\s*)?[""'](?<specifier>[^""']+)[""']|\bexport\s+[^'"";]*?\s+from\s*[""'](?<specifier>[^""']+)[""']|\bimport\s*\(\s*[""'](?<specifier>[^""']+)[""']\s*\)",
        RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex VueStyleSourcePattern = new(
        @"<style\b[^>]*\bsrc\s*=\s*(?<quote>[""'])(?<specifier>[^""']+)\k<quote>[^>]*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

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
            StyleFragments = CreateStyleFragments(result.StyleFragments, documentPath, result.CssContent),
            Dependencies = CombineDependencies(
                ExtractJavaScriptDependencies(result.JsContent),
                ExtractVueStyleDependencies(text)),
            EmbeddedStyleDependencies = ExtractVueStyleDependencies(text),
            SourceMap = result.JsSourceMap,
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
            SourceMap = result.JsSourceMap
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

    internal static IReadOnlyList<string> ExtractVueStyleDependencies(string sfcText)
    {
        ArgumentNullException.ThrowIfNull(sfcText);

        var dependencies = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in VueStyleSourcePattern.Matches(sfcText))
        {
            var specifier = match.Groups["specifier"].Value.Trim();
            if (specifier.Length == 0 || !seen.Add(specifier))
            {
                continue;
            }

            dependencies.Add(specifier);
        }

        return dependencies;
    }

    private static IReadOnlyList<string> CombineDependencies(params IReadOnlyList<string>[] dependencyGroups)
    {
        var combined = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var dependencyGroup in dependencyGroups)
        {
            foreach (var dependency in dependencyGroup)
            {
                if (dependency.Length == 0 || !seen.Add(dependency))
                {
                    continue;
                }

                combined.Add(dependency);
            }
        }

        return combined;
    }

    private static IReadOnlyList<CompiledStyleFragment> CreateStyleFragments(
        IReadOnlyList<DenoSfcStyleFragmentResult>? fragments,
        string documentPath,
        string? cssContent)
    {
        if (fragments is { Count: > 0 })
        {
            return fragments
                .Where(static fragment => !string.IsNullOrWhiteSpace(fragment.CssContent))
                .Select(fragment => new CompiledStyleFragment
                {
                    Content = fragment.CssContent,
                    SourcePath = string.IsNullOrWhiteSpace(fragment.SourcePath)
                        ? documentPath
                        : fragment.SourcePath,
                    SourceLineStart = fragment.SourceLineStart,
                    SourceLineCount = fragment.SourceLineCount
                })
                .ToArray();
        }

        return string.IsNullOrWhiteSpace(cssContent)
            ? []
            :
            [
                new CompiledStyleFragment
                {
                    Content = cssContent!,
                    SourcePath = documentPath
                }
            ];
    }
}
