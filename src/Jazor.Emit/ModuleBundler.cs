using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using DenoHost.Core;

namespace Jazor.Emit;

internal sealed class ModuleBundler
{
    private static readonly Regex ImportFromPattern = new(
        "(?<prefix>\\bfrom\\s+[\"'])(?<path>[^\"']+)(?<suffix>[\"'])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ImportOnlyPattern = new(
        "(?<prefix>\\bimport\\s+[\"'])(?<path>[^\"']+)(?<suffix>[\"'])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task<BundleResult> BundleAsync(BundleOptions options)
    {
        var manifest = ManifestModel.TryLoad(options.ManifestPath);
        if (manifest is null)
            return BundleResult.Fail(6, $"Manifest was not found: '{options.ManifestPath}'.");

        var relativePaths = manifest.Modules
            .Select(static module => module.RelativePath.Replace('\\', '/'))
            .Where(static relativePath => !string.IsNullOrWhiteSpace(relativePath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static relativePath => relativePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (relativePaths.Length == 0)
            return BundleResult.Fail(7, $"No modules were found in '{options.ManifestPath}'.");

        Directory.CreateDirectory(options.InputDirectory);

        var outputDirectory = Path.GetDirectoryName(options.OutputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        var bundleWorkspace = Path.Combine(options.InputDirectory, "__jazor_bundle__");
        if (Directory.Exists(bundleWorkspace))
            Directory.Delete(bundleWorkspace, recursive: true);

        Directory.CreateDirectory(bundleWorkspace);

        var knownPaths = relativePaths.ToDictionary(
            static relativePath => relativePath,
            static relativePath => relativePath,
            StringComparer.OrdinalIgnoreCase);

        var rootAssemblyName = GetRootAssemblyName(manifest);
        var entryRelativePaths = manifest.Modules
            .Where(module => StringComparer.OrdinalIgnoreCase.Equals(module.AssemblyName, rootAssemblyName))
            .Select(static module => module.RelativePath.Replace('\\', '/'))
            .Where(static relativePath => !string.IsNullOrWhiteSpace(relativePath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static relativePath => relativePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (entryRelativePaths.Length == 0)
            entryRelativePaths = relativePaths;

        foreach (var relativePath in relativePaths)
        {
            var sourcePath = Path.Combine(options.InputDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var targetPath = Path.Combine(bundleWorkspace, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            var content = await File.ReadAllTextAsync(sourcePath);
            var rewritten = RewriteModuleImports(content, relativePath, knownPaths);
            await File.WriteAllTextAsync(targetPath, rewritten, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        var tempEntryPath = Path.Combine(bundleWorkspace, "__jazor_bundle_entry__.mjs");
        var entrySource = string.Join(
            Environment.NewLine,
            entryRelativePaths.Select(static relativePath => $"export * from \"./{relativePath}\";"));

        await File.WriteAllTextAsync(tempEntryPath, entrySource, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        try
        {
            var commandArgs = new[]
            {
                "bundle",
                "--platform",
                "browser",
                "-o",
                options.OutputPath,
                Path.GetFileName(tempEntryPath)
            };

            await Deno.Execute(
                new DenoExecuteBaseOptions
                {
                    WorkingDirectory = bundleWorkspace
                },
                commandArgs);

            return BundleResult.Success(options.OutputPath, relativePaths.Length);
        }
        catch (Exception ex)
        {
            return BundleResult.Fail(8, ex.ToString());
        }
        finally
        {
            try
            {
                if (Directory.Exists(bundleWorkspace))
                    Directory.Delete(bundleWorkspace, recursive: true);
            }
            catch
            {
            }
        }
    }

    private static string GetRootAssemblyName(ManifestModel manifest)
    {
        try
        {
            return AssemblyName.GetAssemblyName(manifest.RootAssemblyPath).Name ??
                Path.GetFileNameWithoutExtension(manifest.RootAssemblyPath);
        }
        catch
        {
            return Path.GetFileNameWithoutExtension(manifest.RootAssemblyPath);
        }
    }

    private static string RewriteModuleImports(string content, string currentModulePath, IReadOnlyDictionary<string, string> knownPaths)
    {
        var rewritten = ImportFromPattern.Replace(content, match => RewriteMatch(match, currentModulePath, knownPaths));
        rewritten = ImportOnlyPattern.Replace(rewritten, match => RewriteMatch(match, currentModulePath, knownPaths));
        return rewritten;
    }

    private static string RewriteMatch(Match match, string currentModulePath, IReadOnlyDictionary<string, string> knownPaths)
    {
        var importPath = match.Groups["path"].Value.Replace('\\', '/');
        if (importPath.StartsWith("./", StringComparison.Ordinal) ||
            importPath.StartsWith("../", StringComparison.Ordinal) ||
            importPath.StartsWith("/", StringComparison.Ordinal) ||
            importPath.Contains(':', StringComparison.Ordinal))
        {
            return match.Value;
        }

        if (!knownPaths.TryGetValue(importPath, out var targetPath))
            return match.Value;

        var currentDirectory = Path.GetDirectoryName(currentModulePath.Replace('/', Path.DirectorySeparatorChar)) ?? string.Empty;
        var relativePath = Path.GetRelativePath(
            string.IsNullOrEmpty(currentDirectory) ? "." : currentDirectory,
            targetPath.Replace('/', Path.DirectorySeparatorChar))
            .Replace('\\', '/');

        if (!relativePath.StartsWith("./", StringComparison.Ordinal) &&
            !relativePath.StartsWith("../", StringComparison.Ordinal))
        {
            relativePath = "./" + relativePath;
        }

        return $"{match.Groups["prefix"].Value}{relativePath}{match.Groups["suffix"].Value}";
    }
}

internal sealed record BundleResult(
    bool IsSuccess,
    int ExitCode,
    string? Error,
    string? OutputPath,
    int ModuleCount)
{
    public static BundleResult Success(string outputPath, int moduleCount)
        => new(true, 0, null, outputPath, moduleCount);

    public static BundleResult Fail(int exitCode, string error)
        => new(false, exitCode, error, null, 0);
}
