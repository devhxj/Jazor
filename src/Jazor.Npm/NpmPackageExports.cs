using System.Text.Json;

namespace Jazor.Npm;

internal static class NpmPackageExports
{
    // A workspace's hoisted dependencies may live above the importing package.
    // Keep missing nearer candidates so the editor notices a later local install.
    internal static IEnumerable<string> ManifestCandidates(string directory, string package)
    {
        for (var parent = new DirectoryInfo(directory); parent is not null; parent = parent.Parent)
        {
            var manifest = Path.Combine(parent.FullName, "node_modules", package, "package.json");
            yield return manifest;
            if (File.Exists(manifest))
                yield break;
        }
    }

    internal static bool HasRoot(JsonElement manifest)
    {
        if (!manifest.TryGetProperty("exports", out var exports))
            return true;
        if (exports.ValueKind != JsonValueKind.Object)
            return exports.ValueKind != JsonValueKind.Null;
        var subpaths = false;
        foreach (var entry in exports.EnumerateObject())
        {
            if (entry.Name == "." && entry.Value.ValueKind != JsonValueKind.Null)
                return true;
            subpaths |= entry.Name.StartsWith(".", StringComparison.Ordinal);
        }
        return !subpaths;
    }
}
