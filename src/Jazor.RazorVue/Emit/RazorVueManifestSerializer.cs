using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jazor.RazorVue;

namespace Jazor.RazorVue.Emit;

public static class RazorVueManifestSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions UnifiedManifestJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static RazorVueManifestModel? TryLoad(string manifestPath)
    {
        if (!File.Exists(manifestPath))
            return null;

        var json = File.ReadAllText(manifestPath);
        var manifest = TryDeserializeUnifiedManifest(json)
            ?? JsonSerializer.Deserialize<RazorVueManifestModel>(json, JsonOptions);
        if (manifest is null)
            return null;

        manifest = ProjectUnifiedManifestIfNeeded(manifest);

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

    private static RazorVueManifestModel? TryDeserializeUnifiedManifest(string json)
    {
        var manifest = JsonSerializer.Deserialize<UnifiedJazorManifestModel>(json, UnifiedManifestJsonOptions);
        if (manifest?.Modules is null)
            return null;

        var componentModules = manifest.Modules
            .Where(static module => module.Component is not null)
            .Select(ToRazorVueManifestEntry)
            .OrderBy(static module => module.RelativeModulePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.ComponentName, StringComparer.Ordinal)
            .ToList();
        if (componentModules.Count == 0)
            return null;

        return new RazorVueManifestModel(
            ResolveManifestAssemblyName(manifest.RootAssemblyPath, componentModules),
            manifest.GeneratedAtUtc,
            componentModules,
            NormalizeHostRequirementList(componentModules.SelectMany(static module => module.Styles).ToList()),
            NormalizeHostRequirementList(componentModules.SelectMany(static module => module.PluginRequirements).ToList()));
    }

    private static RazorVueManifestModel ProjectUnifiedManifestIfNeeded(RazorVueManifestModel manifest)
    {
        if (manifest.Modules.Count != 0)
            return manifest;

        return manifest;
    }

    private static RazorVueManifestEntry ToRazorVueManifestEntry(UnifiedJazorManifestModule module)
    {
        var component = module.Component
            ?? throw new InvalidDataException("Unified Jazor manifest module does not contain component metadata.");
        var relativePath = NormalizeRelativeModulePath(module.RelativePath);
        var componentModel = NormalizeComponentModel(component.Model);

        if (componentModel == "sfc" && !string.Equals(NormalizeModuleKind(module.Kind), "vue", StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Unified Jazor manifest SFC component '{relativePath}' must use kind 'vue'.");
        }

        if (componentModel == "h" && !string.Equals(NormalizeModuleKind(module.Kind), "mjs", StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Unified Jazor manifest H component '{relativePath}' must use kind 'mjs'.");
        }

        return new RazorVueManifestEntry(
            module.AssemblyName,
            NormalizeIdentityValue(component.ComponentId, module.AssemblyName + "::" + module.TypeName),
            NormalizeIdentityValue(component.ModuleId, relativePath),
            NormalizeIdentityValue(component.ComponentName, module.TypeName),
            relativePath,
            NormalizeSourceMapPath(module.SourceMapPath, relativePath),
            NormalizeOriginMapPath(component.OriginMapPath, relativePath),
            NormalizeHostRequirementList(component.Imports ?? new List<string>()),
            NormalizeHostRequirementList(component.Styles ?? new List<string>()),
            NormalizeHostRequirementList(component.PluginRequirements ?? new List<string>()),
            component.DescriptorHash ?? string.Empty,
            component.TemplateHash ?? string.Empty,
            component.LogicHash ?? string.Empty,
            component.ContentHash ?? module.Hash ?? string.Empty,
            component.HmrBoundaryKind,
            component.RequiresHydration,
            component.SupportsSsr,
            component.StyleHash ?? string.Empty);
    }

    private static string NormalizeModuleKind(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
            return "mjs";

        return kind!.Trim().ToLowerInvariant();
    }

    private static string NormalizeComponentModel(string? model)
    {
        if (string.IsNullOrWhiteSpace(model))
            return "h";

        return model!.Trim().ToLowerInvariant();
    }

    private static string NormalizeRelativeModulePath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        var segments = normalized
            .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Any(static segment => segment == ".."))
            throw new InvalidDataException($"Unified Jazor manifest relative path cannot escape output directory: '{relativePath}'.");

        return string.Join("/", segments);
    }

    private static string ResolveManifestAssemblyName(string? rootAssemblyPath, IReadOnlyList<RazorVueManifestEntry> modules)
    {
        var assemblyNames = modules
            .Select(static module => module.AssemblyName)
            .Distinct(StringComparer.Ordinal)
            .Take(2)
            .ToArray();
        if (assemblyNames.Length == 1)
            return assemblyNames[0];

        var fileName = string.IsNullOrWhiteSpace(rootAssemblyPath)
            ? null
            : Path.GetFileNameWithoutExtension(rootAssemblyPath);
        return string.IsNullOrWhiteSpace(fileName) ? "Jazor.RazorVue" : fileName!;
    }

    private sealed record UnifiedJazorManifestModel(
        string? RootAssemblyPath,
        DateTime GeneratedAtUtc,
        List<UnifiedJazorManifestModule>? Modules);

    private sealed record UnifiedJazorManifestModule(
        string AssemblyName,
        string TypeName,
        string Id,
        string RelativePath,
        string? Hash,
        string? SourceMapPath,
        string? Kind,
        UnifiedJazorManifestComponent? Component);

    private sealed record UnifiedJazorManifestComponent(
        string? Model,
        string? ComponentId,
        string? ModuleId,
        string? ComponentName,
        string? OriginMapPath,
        List<string>? Imports,
        List<string>? Styles,
        List<string>? PluginRequirements,
        string? DescriptorHash,
        string? TemplateHash,
        string? LogicHash,
        string? ContentHash,
        RazorVueHmrBoundaryKind HmrBoundaryKind,
        bool RequiresHydration,
        bool SupportsSsr,
        string? StyleHash);
}
