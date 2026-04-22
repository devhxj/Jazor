using System.Text.Json;

namespace Jolt.Build;

internal static class DenoBuildImportMapGenerator
{
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

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
        rootDirectory = Path.GetFullPath(rootDirectory);

        var imports = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["vue"] = "npm:vue@3",
            ["vue/"] = "npm:vue@3/"
        };

        var packageJsonPath = Path.Combine(rootDirectory, "package.json");
        if (TryResolveTrustedProjectFilePath(rootDirectory, packageJsonPath, out var trustedPackageJsonPath))
        {
            using var packageJson = JsonDocument.Parse(await File.ReadAllTextAsync(trustedPackageJsonPath, cancellationToken));
            AddPackageImports(packageJson.RootElement, "dependencies", imports);
            AddPackageImports(packageJson.RootElement, "devDependencies", imports);
        }

        var jazorDirectory = EnsureTrustedBuildMetadataPath(
            rootDirectory,
            rootDirectory,
            Path.Combine(rootDirectory, ".jazor"),
            allowMissingLeaf: true);
        Directory.CreateDirectory(jazorDirectory);

        var importMapPath = EnsureTrustedBuildMetadataPath(
            rootDirectory,
            jazorDirectory,
            Path.Combine(jazorDirectory, "build.importmap.json"),
            allowMissingLeaf: true);
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

    internal static bool IsTrustedBuildMetadataPath(
        string rootDirectory,
        string candidatePath,
        FileAttributes attributes)
    {
        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var fullCandidatePath = Path.GetFullPath(candidatePath);
        return IsInsideRoot(fullRootDirectory, fullCandidatePath)
            && (attributes & FileAttributes.ReparsePoint) == 0;
    }

    private static string EnsureTrustedBuildMetadataPath(
        string rootDirectory,
        string boundaryDirectory,
        string candidatePath,
        bool allowMissingLeaf = false)
    {
        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var fullBoundaryDirectory = Path.GetFullPath(boundaryDirectory);
        var fullCandidatePath = Path.GetFullPath(candidatePath);
        if (!IsInsideRoot(fullRootDirectory, fullBoundaryDirectory))
        {
            throw new InvalidOperationException(
                $"Build metadata boundary '{fullBoundaryDirectory}' must stay inside project root '{fullRootDirectory}'.");
        }

        if (!IsInsideRoot(fullBoundaryDirectory, fullCandidatePath))
        {
            throw new InvalidOperationException(
                $"Build metadata path '{fullCandidatePath}' must stay inside trusted boundary '{fullBoundaryDirectory}'.");
        }

        var inspectionPath = GetExistingTrustInspectionPath(fullCandidatePath, allowMissingLeaf);
        while (!string.IsNullOrWhiteSpace(inspectionPath))
        {
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(inspectionPath);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new InvalidOperationException(
                    $"Build metadata path '{fullCandidatePath}' became unavailable while validating '{inspectionPath}'.",
                    ex);
            }
            catch (FileNotFoundException ex)
            {
                throw new InvalidOperationException(
                    $"Build metadata path '{fullCandidatePath}' became unavailable while validating '{inspectionPath}'.",
                    ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException(
                    $"Build metadata path '{fullCandidatePath}' could not be validated because '{inspectionPath}' is not readable.",
                    ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException(
                    $"Build metadata path '{fullCandidatePath}' could not be validated because '{inspectionPath}' is not accessible.",
                    ex);
            }

            // .jazor 元数据文件也必须留在工作区内，且链路上不能穿过 reparse point。
            if (!IsTrustedBuildMetadataPath(fullRootDirectory, inspectionPath, attributes))
            {
                throw new InvalidOperationException(
                    $"Build metadata path '{fullCandidatePath}' traverses an untrusted reparse point inside project root '{fullRootDirectory}'.");
            }

            if (string.Equals(inspectionPath, fullRootDirectory, PathComparison))
            {
                return fullCandidatePath;
            }

            inspectionPath = GetContainingDirectoryPath(inspectionPath);
        }

        throw new InvalidOperationException(
            $"Build metadata path '{fullCandidatePath}' could not be validated within project root '{fullRootDirectory}'.");
    }

    private static bool TryResolveTrustedProjectFilePath(
        string rootDirectory,
        string candidatePath,
        out string trustedPath)
    {
        trustedPath = string.Empty;
        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var fullCandidatePath = Path.GetFullPath(candidatePath);
        if (!IsInsideRoot(fullRootDirectory, fullCandidatePath) || !File.Exists(fullCandidatePath))
        {
            return false;
        }

        try
        {
            trustedPath = EnsureTrustedBuildMetadataPath(
                fullRootDirectory,
                fullRootDirectory,
                fullCandidatePath);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static string GetExistingTrustInspectionPath(
        string candidatePath,
        bool allowMissingLeaf)
    {
        if (File.Exists(candidatePath) || Directory.Exists(candidatePath))
        {
            return candidatePath;
        }

        if (!allowMissingLeaf)
        {
            throw new FileNotFoundException($"Build metadata path '{candidatePath}' was not found.", candidatePath);
        }

        return GetContainingDirectoryPath(candidatePath);
    }

    private static string GetContainingDirectoryPath(string path)
        => Path.GetDirectoryName(path)
            ?? Path.GetPathRoot(path)
            ?? string.Empty;

    private static bool IsInsideRoot(string rootDirectory, string absolutePath)
    {
        var relativePath = Path.GetRelativePath(rootDirectory, absolutePath);
        return string.Equals(relativePath, ".", StringComparison.Ordinal)
            || (!string.Equals(relativePath, "..", StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath));
    }
}
