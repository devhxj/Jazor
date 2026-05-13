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
        bool clean,
        bool writeHostRequirements = true)
    {
        Directory.CreateDirectory(outputDirectory);

        var normalizedOutputDirectory = EnsureDirectorySeparator(Path.GetFullPath(outputDirectory));
        var existingManifest = ManifestModel.TryLoad(manifestPath);
        var artifacts = catalogs
            .SelectMany(static catalog => catalog.Artifacts)
            .OrderBy(static artifact => artifact.RelativeSfcPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static artifact => artifact.ComponentName, StringComparer.Ordinal)
            .ToArray();

        var written = 0;
        var skipped = 0;
        var deleted = 0;

        var nextRazorVueManifest = RazorVueSfcManifestFactory.Create(rootAssemblyPath, catalogs);

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

        if (clean && existingManifest is not null)
        {
            var currentPaths = nextRazorVueManifest.Modules
                .Select(static module => module.RelativeModulePath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var oldModule in existingManifest.ToRazorVueManifest(ManifestComponentModel.Sfc).Modules)
            {
                if (currentPaths.Contains(oldModule.RelativeModulePath))
                    continue;

                DeleteIfExists(GetTargetPath(normalizedOutputDirectory, oldModule.RelativeModulePath), ref deleted);
                DeleteIfExists(GetTargetPath(normalizedOutputDirectory, oldModule.RelativeModulePath) + ".map", ref deleted);
                DeleteIfExists(GetTargetPath(normalizedOutputDirectory, oldModule.RelativeModulePath) + ".origins.json", ref deleted);
            }
        }

        var nextManifest = existingManifest ?? new ManifestModel(rootAssemblyPath, DateTime.UtcNow, []);
        nextManifest = nextManifest.WithRazorVueManifest(nextRazorVueManifest, ManifestComponentModel.Sfc);
        nextManifest.Save(manifestPath);

        var hostRequirementsWriteResult = writeHostRequirements
            ? new RazorVueHostRequirementsModuleWriter().Sync(outputDirectory, nextManifest)
            : WriteResult.Success(0, 0, 0);
        if (!hostRequirementsWriteResult.IsSuccess)
            return hostRequirementsWriteResult;

        return WriteResult.Success(
            written + hostRequirementsWriteResult.Written,
            skipped + hostRequirementsWriteResult.Skipped,
            deleted + hostRequirementsWriteResult.Deleted);
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
