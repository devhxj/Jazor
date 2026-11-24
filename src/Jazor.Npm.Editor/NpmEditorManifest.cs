using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.Npm.Editor;

internal static class NpmEditorManifest
{
    internal static string[] RootNamespaces(IEnumerable<string> packages, CSharpCompilation compilation, HashSet<string> packagesWithRoot)
    {
        // Group by C# namespace but keep the original specifier for package.json lookups.
        var roots = packages.ToLookup(NpmNames.NamespaceFor, StringComparer.Ordinal);
        var conflicts = new HashSet<string>(StringComparer.Ordinal);
        foreach (var group in roots)
        {
            for (var start = group.Key.IndexOf(".Exports", StringComparison.Ordinal); start >= 0;
                 start = group.Key.IndexOf(".Exports", start + 8, StringComparison.Ordinal))
                if (start + 8 == group.Key.Length || group.Key[start + 8] == '.')
                    conflicts.Add(group.Key.Substring(0, start));
        }
        return roots.Where(group => group.Count() == 1 && packagesWithRoot.Contains(group.First()) &&
                group.Key.Split('.').All(IsIdentifier) && !conflicts.Contains(group.Key) &&
                !NpmSourceRequests.HasVisibleExports(compilation.GlobalNamespace, group.Key.Split('.')))
            .Select(group => group.Key)
            .OrderBy(name => name, StringComparer.Ordinal).ToArray();
    }

    private static bool IsIdentifier(string part)
    {
        var token = SyntaxFactory.ParseToken("@" + part);
        return token.IsKind(SyntaxKind.IdentifierToken) && token.ValueText == part && !token.ContainsDiagnostics;
    }

    internal static bool HasPackageRoot(string directory, string package)
    {
        var path = NpmPackageExports.ManifestCandidates(directory, package).FirstOrDefault(File.Exists);
        if (path is null)
            return true; // A declared dependency can be restored on the first selected member.
        try
        {
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            return json.RootElement.ValueKind == JsonValueKind.Object && NpmPackageExports.HasRoot(json.RootElement);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return false;
        }
    }

    internal static bool SamePath(string left, string right) => string.Equals(left, right,
        Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    internal static string[] ReadPackageNames(string text)
    {
        try
        {
            using var json = JsonDocument.Parse(text);
            if (json.RootElement.ValueKind != JsonValueKind.Object)
                return [];
            return new[] { "dependencies", "devDependencies", "peerDependencies", "optionalDependencies" }
                .Where(section => json.RootElement.TryGetProperty(section, out var dependencies) &&
                    dependencies.ValueKind == JsonValueKind.Object)
                .SelectMany(section => json.RootElement.GetProperty(section).EnumerateObject().Select(property => property.Name))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }
        catch (JsonException)
        {
            // The workspace can contain an incomplete manifest while it is edited.
            return [];
        }
    }
}
