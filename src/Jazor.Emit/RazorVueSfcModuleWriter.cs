using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jazor.Common.SourceMaps;
using Jazor.Emit.SourceMaps;

namespace Jazor.Emit;

internal sealed class RazorVueSfcModuleWriter
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly SourceMapBuilder ModuleMapBuilder = new();
    private static readonly SourceMapWriter ModuleMapWriter = new();
    private static readonly JsonSerializerOptions OriginJsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public WriteResult Write(
        string rootAssemblyPath,
        string outputDirectory,
        string manifestPath,
        IReadOnlyList<RazorVueSfcCatalogRecord> catalogs,
        bool clean)
    {
        Directory.CreateDirectory(outputDirectory);

        var normalizedOutputDirectory = EnsureDirectorySeparator(Path.GetFullPath(outputDirectory));
        var existingManifest = RazorVueManifestSerializer.TryLoad(manifestPath);
        var artifacts = catalogs
            .SelectMany(static catalog => catalog.Artifacts)
            .OrderBy(static artifact => artifact.RelativeSfcPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static artifact => artifact.ComponentName, StringComparer.Ordinal)
            .ToArray();

        var written = 0;
        var skipped = 0;
        var deleted = 0;

        var nextManifest = RazorVueSfcManifestFactory.Create(rootAssemblyPath, catalogs);
        var hostRequirementsModulePath = RazorVueModuleWriter.GetHostRequirementsModulePath(outputDirectory);

        foreach (var artifact in artifacts)
        {
            var targetPath = GetTargetPath(normalizedOutputDirectory, artifact.RelativeSfcPath);
            var mapPath = targetPath + ".map";
            var originMapPath = targetPath + ".origins.json";
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            var sourceMap = ModuleMapBuilder.BuildModuleMap(
                artifact.RelativeSfcPath,
                artifact.SfcText,
                artifact.SourceOrigins.Select(ToLegacyOrigin).ToArray(),
                TryReadSourceContent);
            var mapJson = ModuleMapWriter.Write(sourceMap);
            var originJson = BuildOriginMapJson(artifact);

            var artifactChanged = !File.Exists(targetPath)
                || !string.Equals(File.ReadAllText(targetPath), artifact.SfcText, StringComparison.Ordinal);
            if (artifactChanged)
                File.WriteAllText(targetPath, artifact.SfcText, Utf8WithoutBom);

            var mapChanged = !File.Exists(mapPath)
                || !string.Equals(File.ReadAllText(mapPath), mapJson, StringComparison.Ordinal);
            if (mapChanged)
                File.WriteAllText(mapPath, mapJson, Utf8WithoutBom);

            var originChanged = !File.Exists(originMapPath)
                || !string.Equals(File.ReadAllText(originMapPath), originJson, StringComparison.Ordinal);
            if (originChanged)
                File.WriteAllText(originMapPath, originJson, Utf8WithoutBom);

            if (artifactChanged || mapChanged || originChanged)
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

    private static RazorVueEmitSourceOriginRecord ToLegacyOrigin(RazorVueEmitSfcSourceOriginRecord origin)
        => new(
            origin.SourceFilePath,
            origin.SourceSpanStart,
            origin.SourceSpanLength,
            origin.GeneratedFilePath,
            origin.GeneratedSpanStart,
            origin.GeneratedSpanLength,
            origin.StartLine,
            origin.StartColumn,
            origin.MappingQuality,
            origin.Provenance);

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
        var assemblyNameLiteral = JsonSerializer.Serialize(manifest.AssemblyName);
        var generatedAtUtcLiteral = JsonSerializer.Serialize(manifest.GeneratedAtUtc.ToString("O"));
        var stylesLiteral = BuildStringArrayLiteral(manifest.Styles ?? []);
        var modulesLiteral = BuildHostModulesLiteral(manifest.Modules);
        var pluginRequirementsLiteral = BuildStringArrayLiteral(manifest.PluginRequirements ?? []);

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
        => JsonSerializer.Serialize(
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
                styleHash = module.StyleHash,
                hmrBoundaryKind = module.HmrBoundaryKind,
                requiresHydration = module.RequiresHydration,
                supportsSsr = module.SupportsSsr
            }));

    private static string BuildStringArrayLiteral(IReadOnlyList<string> values)
        => "[" + string.Join(", ", values.Select(static value => JsonSerializer.Serialize(value))) + "]";

    private static string BuildOriginMapJson(RazorVueEmitSfcArtifactRecord artifact)
        => JsonSerializer.Serialize(
            new
            {
                componentId = artifact.Identity.ComponentId,
                moduleId = artifact.Identity.ModuleId,
                componentName = artifact.ComponentName,
                relativeSfcPath = artifact.RelativeSfcPath,
                descriptorHash = artifact.Identity.DescriptorHash,
                templateHash = artifact.Identity.TemplateHash,
                logicHash = artifact.Identity.LogicHash,
                styleHash = artifact.Identity.StyleHash,
                templateBlock = new
                {
                    textLength = artifact.TemplateBlock.Text.Length,
                    origins = artifact.TemplateBlock.SourceOrigins.Select(ToOriginJsonModel).ToArray()
                },
                scriptSetupBlock = new
                {
                    language = artifact.ScriptSetupBlock.Language,
                    textLength = artifact.ScriptSetupBlock.Text.Length,
                    origins = artifact.ScriptSetupBlock.SourceOrigins.Select(ToOriginJsonModel).ToArray()
                },
                styleBlocks = artifact.StyleBlocks.Select(static block => new
                {
                    isScoped = block.IsScoped,
                    moduleName = block.ModuleName,
                    language = block.Language,
                    sourceFilePath = block.SourceFilePath,
                    textLength = block.Text.Length,
                    origins = block.SourceOrigins.Select(ToOriginJsonModel).ToArray()
                }).ToArray(),
                customBlocks = artifact.CustomBlocks.Select(static block => new
                {
                    name = block.Name,
                    language = block.Language,
                    sourceFilePath = block.SourceFilePath,
                    textLength = block.Text.Length,
                    attributes = block.Attributes.Select(static attribute => new
                    {
                        name = attribute.Name,
                        value = attribute.Value
                    }).ToArray(),
                    origins = block.SourceOrigins.Select(ToOriginJsonModel).ToArray()
                }).ToArray(),
                origins = artifact.SourceOrigins.Select(ToOriginJsonModel).ToArray()
            },
            OriginJsonOptions);

    private static object ToOriginJsonModel(RazorVueEmitSfcSourceOriginRecord origin)
        => new
        {
            originKind = origin.OriginKind,
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
        };
}
