using Jazor.Emit.SourceMaps;
using ECMAScript.Internal.SourceMaps;
using System.Text;
using System.Text.Json;

namespace Jazor.Emit;

internal sealed class RazorVueModuleWriter
{
    private const string HostRequirementsModuleRelativePath = "__jazor/razorvue-host.mjs";
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly SourceMapBuilder ModuleMapBuilder = new();
    private static readonly SourceMapWriter ModuleMapWriter = new();
    private static readonly JsonSerializerOptions OriginJsonOptions = new() { WriteIndented = true };

    public WriteResult Write(
        string rootAssemblyPath,
        string outputDirectory,
        string manifestPath,
        IReadOnlyList<RazorVueCatalogRecord> catalogs,
        bool clean)
    {
        Directory.CreateDirectory(outputDirectory);

        var normalizedOutputDirectory = EnsureDirectorySeparator(Path.GetFullPath(outputDirectory));
        var existingManifest = RazorVueManifestSerializer.TryLoad(manifestPath);
        var artifacts = catalogs
            .SelectMany(static catalog => catalog.Artifacts)
            .OrderBy(static artifact => artifact.RelativeModulePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static artifact => artifact.ComponentName, StringComparer.Ordinal)
            .ToArray();

        var written = 0;
        var skipped = 0;
        var deleted = 0;

        var nextManifest = RazorVueManifestFactory.Create(rootAssemblyPath, catalogs);
        var hostRequirementsModulePath = GetHostRequirementsModulePath(normalizedOutputDirectory);

        foreach (var artifact in artifacts)
        {
            var targetPath = GetTargetPath(normalizedOutputDirectory, artifact.RelativeModulePath);
            var mapPath = GetSourceMapPath(targetPath);
            var originMapPath = GetOriginMapPath(targetPath);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            var sourceMap = ModuleMapBuilder.BuildModuleMap(
                artifact.RelativeModulePath,
                artifact.ModuleCode,
                artifact.SourceOrigins,
                TryReadSourceContent);
            var moduleCode = ModuleMapWriter.AppendSourceMappingUrl(artifact.ModuleCode, Path.GetFileName(mapPath));
            var mapJson = ModuleMapWriter.Write(sourceMap);
            var originJson = BuildOriginMapJson(artifact);

            var moduleChanged = !File.Exists(targetPath)
                || !string.Equals(File.ReadAllText(targetPath), moduleCode, StringComparison.Ordinal);
            if (moduleChanged)
                File.WriteAllText(targetPath, moduleCode, Utf8WithoutBom);

            var mapChanged = !File.Exists(mapPath)
                || !string.Equals(File.ReadAllText(mapPath), mapJson, StringComparison.Ordinal);
            if (mapChanged)
                File.WriteAllText(mapPath, mapJson, Utf8WithoutBom);

            var originChanged = !File.Exists(originMapPath)
                || !string.Equals(File.ReadAllText(originMapPath), originJson, StringComparison.Ordinal);
            if (originChanged)
                File.WriteAllText(originMapPath, originJson, Utf8WithoutBom);

            if (moduleChanged || mapChanged || originChanged)
                written++;
            else
                skipped++;
        }

        if (nextManifest.Modules.Count > 0)
        {
            var hostRequirementsDirectory = Path.GetDirectoryName(hostRequirementsModulePath);
            if (!string.IsNullOrWhiteSpace(hostRequirementsDirectory))
                Directory.CreateDirectory(hostRequirementsDirectory);

            var hostRequirementsCode = BuildHostRequirementsModule(nextManifest);
            var hostRequirementsChanged = !File.Exists(hostRequirementsModulePath)
                || !string.Equals(File.ReadAllText(hostRequirementsModulePath), hostRequirementsCode, StringComparison.Ordinal);

            if (hostRequirementsChanged)
            {
                File.WriteAllText(hostRequirementsModulePath, hostRequirementsCode, Utf8WithoutBom);
                written++;
            }
            else
            {
                skipped++;
            }
        }

        if (clean && existingManifest is not null)
        {
            var currentPaths = nextManifest.Modules
                .Select(static module => module.RelativeModulePath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var oldModule in existingManifest.Modules)
            {
                if (currentPaths.Contains(oldModule.RelativeModulePath))
                    continue;

                DeleteIfExists(GetTargetPath(normalizedOutputDirectory, oldModule.RelativeModulePath), ref deleted);
                DeleteIfExists(GetTargetPath(normalizedOutputDirectory, oldModule.RelativeModulePath) + ".map", ref deleted);
                DeleteIfExists(GetTargetPath(normalizedOutputDirectory, oldModule.RelativeModulePath) + ".origins.json", ref deleted);
            }
        }

        if (clean && nextManifest.Modules.Count == 0)
            DeleteIfExists(hostRequirementsModulePath, ref deleted);

        nextManifest.Save(manifestPath);
        return WriteResult.Success(written, skipped, deleted);
    }

    public static string GetManifestPath(string baseManifestPath)
    {
        var directory = Path.GetDirectoryName(baseManifestPath) ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(baseManifestPath);
        var extension = Path.GetExtension(baseManifestPath);
        return Path.Combine(directory, fileName + "-razorvue" + extension);
    }

    public static string GetHostRequirementsModulePath(string outputDirectory)
        => GetTargetPath(EnsureDirectorySeparator(Path.GetFullPath(outputDirectory)), HostRequirementsModuleRelativePath);

    private static string? TryReadSourceContent(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            return null;

        try
        {
            return File.Exists(sourcePath) ? File.ReadAllText(sourcePath) : null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    private static string GetSourceMapPath(string modulePath)
        => modulePath + ".map";

    private static string GetOriginMapPath(string modulePath)
        => modulePath + ".origins.json";

    private static void DeleteIfExists(string path, ref int deleted)
    {
        if (!File.Exists(path))
            return;

        File.Delete(path);
        deleted++;
    }

    private static string GetTargetPath(string normalizedOutputDirectory, string relativePath)
    {
        var relativePlatformPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(normalizedOutputDirectory, relativePlatformPath));
        if (!fullPath.StartsWith(normalizedOutputDirectory, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Refusing to write outside output directory: '{relativePath}'.");

        return fullPath;
    }

    private static string EnsureDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;

    private static string BuildHostRequirementsModule(RazorVueManifestModel manifest)
    {
        var assemblyNameLiteral = System.Text.Json.JsonSerializer.Serialize(manifest.AssemblyName);
        var generatedAtUtcLiteral = System.Text.Json.JsonSerializer.Serialize(manifest.GeneratedAtUtc.ToString("O"));
        var stylesLiteral = BuildStringArrayLiteral(manifest.Styles ?? []);
        var modulesLiteral = BuildHostModulesLiteral(manifest.Modules);
        var pluginRequirementsLiteral = BuildStringArrayLiteral(manifest.PluginRequirements ?? []);

        // Keep unbundled host metadata on the same explicit contract shape the bundle
        // sidecars expose, so host consumers do not need separate parsing branches.
        return $$"""
        export const razorVueHostAssemblyName = {{assemblyNameLiteral}};
        export const razorVueHostGeneratedAtUtc = {{generatedAtUtcLiteral}};
        export const razorVueStyles = Object.freeze({{stylesLiteral}});
        export const razorVuePluginRequirements = Object.freeze({{pluginRequirementsLiteral}});
        export const razorVueHostModules = Object.freeze({{modulesLiteral}});
        export const razorVueHostRequirements = Object.freeze({
          assemblyName: razorVueHostAssemblyName,
          generatedAtUtc: razorVueHostGeneratedAtUtc,
          styles: razorVueStyles,
          pluginRequirements: razorVuePluginRequirements,
          modules: razorVueHostModules
        });
        """.ReplaceLineEndings("\n");
    }

    private static string BuildHostModulesLiteral(IReadOnlyList<RazorVueManifestEntry> modules)
        => System.Text.Json.JsonSerializer.Serialize(
            modules.Select(static module => new
            {
                assemblyName = module.AssemblyName,
                componentId = module.ComponentId,
                moduleId = module.ModuleId,
                componentName = module.ComponentName,
                relativeModulePath = module.RelativeModulePath,
                sourceMapPath = module.SourceMapPath,
                originMapPath = module.OriginMapPath,
                styles = module.Styles,
                pluginRequirements = module.PluginRequirements,
                descriptorHash = module.DescriptorHash,
                templateHash = module.TemplateHash,
                logicHash = module.LogicHash,
                contentHash = module.ContentHash,
                hmrBoundaryKind = module.HmrBoundaryKind,
                requiresHydration = module.RequiresHydration,
                supportsSsr = module.SupportsSsr
            }));

    private static string BuildStringArrayLiteral(IReadOnlyList<string> values)
        => "[" + string.Join(", ", values.Select(static value => System.Text.Json.JsonSerializer.Serialize(value))) + "]";

    private static string BuildOriginMapJson(RazorVueEmitArtifactRecord artifact)
        => JsonSerializer.Serialize(
            new
            {
                componentId = artifact.Identity.ComponentId,
                moduleId = artifact.Identity.ModuleId,
                componentName = artifact.ComponentName,
                relativeModulePath = artifact.RelativeModulePath,
                origins = artifact.SourceOrigins.Select(static origin => new
                {
                    sourceFilePath = origin.SourceFilePath,
                    sourceSpanStart = origin.SourceSpanStart,
                    sourceSpanLength = origin.SourceSpanLength,
                    generatedFilePath = origin.GeneratedFilePath,
                    generatedSpanStart = origin.GeneratedSpanStart,
                    generatedSpanLength = origin.GeneratedSpanLength,
                    startLine = origin.StartLine,
                    startColumn = origin.StartColumn,
                    mappingQuality = origin.MappingQuality,
                    provenance = origin.Provenance
                })
            },
            OriginJsonOptions);
}
