using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Jazor.RazorVue.Artifacts;

internal sealed class RazorVueSfcCatalogBuilder
{
    public RazorVueSfcCatalog Build(string assemblyName, ImmutableArray<VueSfcArtifact> artifacts)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
            throw new ArgumentException("Assembly name cannot be empty.", nameof(assemblyName));

        var normalizedArtifacts = artifacts.IsDefault
            ? ImmutableArray<VueSfcArtifact>.Empty
            : artifacts
                .Select(static artifact => artifact with
                {
                    RelativeSfcPath = NormalizeRelativePath(artifact.RelativeSfcPath)
                })
                .OrderBy(static artifact => artifact.RelativeSfcPath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static artifact => artifact.ComponentName, StringComparer.Ordinal)
                .ToImmutableArray();

        return new RazorVueSfcCatalog(assemblyName, normalizedArtifacts);
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new InvalidOperationException("RazorVue SFC artifact relative path cannot be empty.");

        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        if (Path.IsPathRooted(normalized))
            throw new InvalidOperationException($"RazorVue SFC artifact relative path must be relative: '{relativePath}'.");

        var segments = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"RazorVue SFC artifact relative path cannot escape output directory: '{relativePath}'.");

        return string.Join("/", segments);
    }
}
