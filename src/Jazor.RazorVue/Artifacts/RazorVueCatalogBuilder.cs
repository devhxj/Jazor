using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Jazor.RazorVue.Artifacts;

internal sealed class RazorVueCatalogBuilder
{
    public RazorVueCatalog Build(string assemblyName, ImmutableArray<VueCompiledArtifact> artifacts)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
            throw new ArgumentException("Assembly name cannot be empty.", nameof(assemblyName));

        var normalizedArtifacts = artifacts.IsDefault
            ? ImmutableArray<VueCompiledArtifact>.Empty
            : artifacts
                .Select(static artifact => artifact with
                {
                    RelativeModulePath = NormalizeRelativePath(artifact.RelativeModulePath)
                })
                .OrderBy(static artifact => artifact.RelativeModulePath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static artifact => artifact.ComponentName, StringComparer.Ordinal)
                .ToImmutableArray();

        return new RazorVueCatalog(assemblyName, normalizedArtifacts);
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new InvalidOperationException("RazorVue artifact relative path cannot be empty.");

        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        if (Path.IsPathRooted(normalized))
            throw new InvalidOperationException($"RazorVue artifact relative path must be relative: '{relativePath}'.");

        var segments = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"RazorVue artifact relative path cannot escape output directory: '{relativePath}'.");

        return string.Join("/", segments);
    }
}
