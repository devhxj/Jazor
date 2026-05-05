using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.Common.Emit;

public static class RazorVueManifestSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static RazorVueManifestModel? TryLoad(string manifestPath)
    {
        if (!File.Exists(manifestPath))
            return null;

        var json = File.ReadAllText(manifestPath);
        var manifest = JsonSerializer.Deserialize<RazorVueManifestModel>(json, JsonOptions);
        if (manifest is null)
            return null;

        var normalizedModules = manifest.Modules
            .Select(static module => module with
            {
                ComponentId = NormalizeIdentityValue(
                    module.ComponentId,
                    module.AssemblyName + "::" + module.ComponentName),
                ModuleId = NormalizeIdentityValue(
                    module.ModuleId,
                    module.RelativeModulePath),
                SourceMapPath = NormalizeSourceMapPath(
                    module.SourceMapPath,
                    module.RelativeModulePath),
                OriginMapPath = NormalizeOriginMapPath(
                    module.OriginMapPath,
                    module.RelativeModulePath),
                StyleHash = module.StyleHash ?? string.Empty,
                Styles = NormalizeHostRequirementList(module.Styles),
                PluginRequirements = NormalizeHostRequirementList(module.PluginRequirements)
            })
            .ToList();

        return manifest with
        {
            Modules = normalizedModules,
            Styles = NormalizeHostRequirementList(
                manifest.Styles is not null
                    ? manifest.Styles
                    : normalizedModules.SelectMany(static module => module.Styles).ToList()),
            PluginRequirements = NormalizeHostRequirementList(
                manifest.PluginRequirements is not null
                    ? manifest.PluginRequirements
                    : normalizedModules.SelectMany(static module => module.PluginRequirements).ToList())
        };
    }

    public static void Save(this RazorVueManifestModel manifest, string manifestPath)
    {
        if (manifest is null)
            throw new ArgumentNullException(nameof(manifest));

        var directory = Path.GetDirectoryName(manifestPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions));
    }

    public static List<string> NormalizeHostRequirementList(IReadOnlyList<string> values)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));

        return values
            .Select(static value => value?.Trim())
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value!)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToList();
    }

    private static string NormalizeIdentityValue(string? currentValue, string fallbackValue)
        => string.IsNullOrWhiteSpace(currentValue) ? fallbackValue : currentValue!;

    private static string NormalizeSourceMapPath(string? currentValue, string relativeModulePath)
        => string.IsNullOrWhiteSpace(currentValue) ? relativeModulePath + ".map" : currentValue!;

    private static string NormalizeOriginMapPath(string? currentValue, string relativeModulePath)
        => string.IsNullOrWhiteSpace(currentValue) ? relativeModulePath + ".origins.json" : currentValue!;
}
