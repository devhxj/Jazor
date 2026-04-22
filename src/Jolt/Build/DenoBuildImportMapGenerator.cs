using System.Text.Json;

namespace Jolt.Build;

internal static class DenoBuildImportMapGenerator
{
    private static readonly JsonSerializerOptions ImportMapSerializerOptions = new()
    {
        WriteIndented = true
    };

    public static async Task<string> GenerateAsync(
        string rootDirectory,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        cancellationToken.ThrowIfCancellationRequested();

        var imports = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["vue"] = "npm:vue@3",
            ["vue/"] = "npm:vue@3/"
        };

        var packageJsonPath = Path.Combine(rootDirectory, "package.json");
        if (File.Exists(packageJsonPath))
        {
            using var packageJson = JsonDocument.Parse(await File.ReadAllTextAsync(packageJsonPath, cancellationToken));
            AddPackageImports(packageJson.RootElement, "dependencies", imports);
            AddPackageImports(packageJson.RootElement, "devDependencies", imports);
        }

        var jazorDirectory = Path.Combine(rootDirectory, ".jazor");
        Directory.CreateDirectory(jazorDirectory);

        var importMapPath = Path.Combine(jazorDirectory, "build.importmap.json");
        var serializedImportMap = SerializeImportMap(imports);
        if (File.Exists(importMapPath))
        {
            var existingImportMap = await File.ReadAllTextAsync(importMapPath, cancellationToken);
            if (string.Equals(existingImportMap, serializedImportMap, StringComparison.Ordinal))
            {
                return importMapPath;
            }
        }

        await File.WriteAllTextAsync(importMapPath, serializedImportMap, cancellationToken);
        return importMapPath;
    }

    private static string SerializeImportMap(IReadOnlyDictionary<string, string> imports)
    {
        var orderedImports = imports
            .OrderBy(static entry => entry.Key, StringComparer.Ordinal)
            .ToDictionary(
                static entry => entry.Key,
                static entry => entry.Value,
                StringComparer.Ordinal);
        return JsonSerializer.Serialize(new { imports = orderedImports }, ImportMapSerializerOptions);
    }

    private static void AddPackageImports(
        JsonElement root,
        string propertyName,
        IDictionary<string, string> imports)
    {
        if (!root.TryGetProperty(propertyName, out var dependencies)
            || dependencies.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var dependency in dependencies.EnumerateObject())
        {
            var version = dependency.Value.GetString();
            if (!IsSupportedNpmVersion(version))
            {
                continue;
            }

            imports[dependency.Name] = $"npm:{dependency.Name}@{version}";
            imports[dependency.Name + "/"] = $"npm:{dependency.Name}@{version}/";
        }
    }

    private static bool IsSupportedNpmVersion(string? version)
        => !string.IsNullOrWhiteSpace(version)
            && !version.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
            && !version.StartsWith("workspace:", StringComparison.OrdinalIgnoreCase)
            && !version.StartsWith("link:", StringComparison.OrdinalIgnoreCase);
}
