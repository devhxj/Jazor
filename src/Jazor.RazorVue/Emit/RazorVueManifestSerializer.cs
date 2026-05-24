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

    public static RazorVueManifestLoadResult Load(string manifestPath)
        => Load(manifestPath, componentModel: null);

    public static RazorVueManifestLoadResult Load(string manifestPath, string? componentModel)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
            throw new ArgumentException("Manifest path is required.", nameof(manifestPath));

        if (!File.Exists(manifestPath))
            return RazorVueManifestLoadResult.FileNotFound(manifestPath);

        try
        {
            var json = File.ReadAllText(manifestPath);
            if (TryLoadUnifiedManifest(json, componentModel, out var unifiedManifest))
            {
                return unifiedManifest.Modules.Count == 0
                    ? RazorVueManifestLoadResult.NoComponentEntries(manifestPath)
                    : RazorVueManifestLoadResult.Success(manifestPath, unifiedManifest);
            }

            var legacyManifest = JsonSerializer.Deserialize<RazorVueManifestModel>(json, JsonOptions);
            if (legacyManifest is null)
            {
                return RazorVueManifestLoadResult.Invalid(
                    manifestPath,
                    "Manifest JSON could not be deserialized.");
            }

            var normalizedManifest = NormalizeManifest(legacyManifest);
            var projectedManifest = FilterByComponentModelIfNeeded(normalizedManifest, componentModel);
            return projectedManifest.Modules.Count == 0
                ? RazorVueManifestLoadResult.NoComponentEntries(manifestPath)
                : RazorVueManifestLoadResult.Success(manifestPath, projectedManifest);
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException or InvalidOperationException)
        {
            return RazorVueManifestLoadResult.Invalid(manifestPath, ex.Message);
        }
    }

    public static RazorVueManifestModel? TryLoad(string manifestPath)
    {
        return Load(manifestPath).Manifest;
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

    public static List<string> NormalizeRouteTemplates(IReadOnlyList<string> values)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));

        return values
            .Select(static value => value?.Trim())
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value!)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static string NormalizeIdentityValue(string? currentValue, string fallbackValue)
        => string.IsNullOrWhiteSpace(currentValue) ? fallbackValue : currentValue!;

    private static string NormalizeSourceMapPath(string? currentValue, string relativeModulePath)
        => NormalizeManifestRelativePath(currentValue, relativeModulePath + ".map", "source map relative path");

    private static string NormalizeOriginMapPath(string? currentValue, string relativeModulePath)
        => NormalizeManifestRelativePath(currentValue, relativeModulePath + ".origins.json", "origin map relative path");

    private static bool TryLoadUnifiedManifest(string json, string? componentModel, out RazorVueManifestModel manifest)
    {
        var unifiedManifest = JsonSerializer.Deserialize<UnifiedJazorManifestModel>(json, UnifiedManifestJsonOptions);
        if (unifiedManifest is null)
        {
            manifest = null!;
            return false;
        }

        if (unifiedManifest.Modules is null)
        {
            if (!string.IsNullOrWhiteSpace(unifiedManifest.RootAssemblyPath))
                throw new InvalidDataException("Unified Jazor manifest modules cannot be null.");

            manifest = null!;
            return false;
        }

        if (!LooksLikeUnifiedManifest(unifiedManifest))
        {
            manifest = null!;
            return false;
        }

        if (unifiedManifest.Modules.Any(static module => module is null))
            throw new InvalidDataException("Unified Jazor manifest modules cannot contain null entries.");

        var componentModules = unifiedManifest.Modules
            .Where(static module => module.Component is not null)
            .Select(ToRazorVueManifestEntry)
            .Where(module => MatchesComponentModel(module, componentModel))
            .OrderBy(static module => module.RelativeModulePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.ComponentName, StringComparer.Ordinal)
            .ToList();

        manifest = new RazorVueManifestModel(
            ResolveManifestAssemblyName(unifiedManifest.RootAssemblyPath, componentModules),
            unifiedManifest.GeneratedAtUtc,
            componentModules,
            NormalizeHostRequirementList(componentModules.SelectMany(static module => module.Styles).ToList()),
            NormalizeHostRequirementList(componentModules.SelectMany(static module => module.PluginRequirements).ToList()));
        return true;
    }

    private static bool LooksLikeUnifiedManifest(UnifiedJazorManifestModel manifest)
        => !string.IsNullOrWhiteSpace(manifest.RootAssemblyPath)
           || manifest.Modules.Any(static module =>
               module is not null &&
               (!string.IsNullOrWhiteSpace(module.RelativePath)
               || !string.IsNullOrWhiteSpace(module.Kind)
               || !string.IsNullOrWhiteSpace(module.TypeName)
               || !string.IsNullOrWhiteSpace(module.Id)
               || module.Component is not null));

    private static RazorVueManifestModel NormalizeManifest(RazorVueManifestModel manifest)
    {
        if (manifest.Modules is null)
            throw new InvalidDataException("Legacy RazorVue manifest modules cannot be null.");

        if (manifest.Modules.Any(static module => module is null))
            throw new InvalidDataException("Legacy RazorVue manifest modules cannot contain null entries.");

        var normalizedModules = manifest.Modules
            .Select(NormalizeLegacyManifestEntry)
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

    private static RazorVueManifestEntry NormalizeLegacyManifestEntry(RazorVueManifestEntry module)
    {
        var assemblyName = NormalizeRequiredValue(module.AssemblyName, "legacy RazorVue manifest module assembly name");
        var componentName = NormalizeRequiredValue(module.ComponentName, "legacy RazorVue manifest module component name");
        var relativeModulePath = NormalizeManifestRelativePath(module.RelativeModulePath, null, "relative path");

        return module with
        {
            AssemblyName = assemblyName,
            ComponentName = componentName,
            ComponentId = NormalizeIdentityValue(
                module.ComponentId,
                assemblyName + "::" + componentName),
            ModuleId = NormalizeIdentityValue(
                module.ModuleId,
                relativeModulePath),
            RouteTemplates = NormalizeRouteTemplates(module.RouteTemplates ?? new List<string>()),
            RelativeModulePath = relativeModulePath,
            SourceMapPath = NormalizeSourceMapPath(
                module.SourceMapPath,
                relativeModulePath),
            OriginMapPath = NormalizeOriginMapPath(
                module.OriginMapPath,
                relativeModulePath),
            Imports = NormalizeHostRequirementList(module.Imports ?? new List<string>()),
            Styles = NormalizeHostRequirementList(module.Styles ?? new List<string>()),
            PluginRequirements = NormalizeHostRequirementList(module.PluginRequirements ?? new List<string>()),
            DescriptorHash = module.DescriptorHash ?? string.Empty,
            TemplateHash = module.TemplateHash ?? string.Empty,
            LogicHash = module.LogicHash ?? string.Empty,
            ContentHash = module.ContentHash ?? string.Empty,
            StyleHash = module.StyleHash ?? string.Empty,
            ComponentModel = NormalizeComponentModel(module.ComponentModel)
        };
    }

    private static RazorVueManifestModel FilterByComponentModelIfNeeded(RazorVueManifestModel manifest, string? componentModel)
    {
        if (string.IsNullOrWhiteSpace(componentModel))
            return manifest;

        var filteredModules = manifest.Modules
            .Where(module => MatchesComponentModel(module, componentModel))
            .ToList();

        return manifest with
        {
            Modules = filteredModules,
            Styles = NormalizeHostRequirementList(filteredModules.SelectMany(static module => module.Styles).ToList()),
            PluginRequirements = NormalizeHostRequirementList(filteredModules.SelectMany(static module => module.PluginRequirements).ToList())
        };
    }

    private static RazorVueManifestEntry ToRazorVueManifestEntry(UnifiedJazorManifestModule module)
    {
        var component = module.Component
            ?? throw new InvalidDataException("Unified Jazor manifest module does not contain component metadata.");
        var assemblyName = NormalizeRequiredValue(module.AssemblyName, "unified Jazor manifest component assembly name");
        var typeName = NormalizeRequiredValue(module.TypeName, "unified Jazor manifest component type name");
        var relativePath = NormalizeManifestRelativePath(module.RelativePath, null, "relative path");
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
            assemblyName,
            NormalizeIdentityValue(component.ComponentId, assemblyName + "::" + typeName),
            NormalizeIdentityValue(component.ModuleId, relativePath),
            NormalizeIdentityValue(component.ComponentName, typeName),
            NormalizeRouteTemplates(component.RouteTemplates ?? new List<string>()),
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
            component.StyleHash ?? string.Empty,
            componentModel);
    }

    private static string NormalizeRequiredValue(string? value, string description)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidDataException($"RazorVue manifest {description} cannot be empty.");

        return value!.Trim();
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

        var normalized = model!.Trim().ToLowerInvariant();
        return normalized is "h" or "sfc"
            ? normalized
            : throw new InvalidDataException("Unsupported manifest component model '" + model + "'.");
    }

    private static bool MatchesComponentModel(RazorVueManifestEntry module, string? componentModel)
        => string.IsNullOrWhiteSpace(componentModel)
           || string.Equals(
               NormalizeComponentModel(module.ComponentModel),
               NormalizeComponentModel(componentModel),
               StringComparison.Ordinal);

    private static string NormalizeManifestRelativePath(string? path, string? fallbackPath, string description)
    {
        var effectivePath = string.IsNullOrWhiteSpace(path) ? fallbackPath : path;
        if (string.IsNullOrWhiteSpace(effectivePath))
            throw new InvalidDataException($"RazorVue manifest {description} cannot be empty.");

        var rawPath = effectivePath!.Trim();
        if (StartsWithRootMarker(rawPath) || ContainsDriveQualifiedSegment(rawPath))
        {
            throw new InvalidDataException(
                $"RazorVue manifest {description} must be relative: '{effectivePath}'.");
        }

        var normalized = rawPath.Replace('\\', '/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        var segments = normalized
            .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Length == 0)
            throw new InvalidDataException($"RazorVue manifest {description} cannot be empty.");

        if (segments.Any(static segment => segment == ".."))
        {
            throw new InvalidDataException(
                $"RazorVue manifest {description} cannot escape output directory: '{effectivePath}'.");
        }

        return string.Join("/", segments);
    }

    private static bool StartsWithRootMarker(string path)
        => path.StartsWith("/", StringComparison.Ordinal) ||
           path.StartsWith("\\", StringComparison.Ordinal);

    private static bool ContainsDriveQualifiedSegment(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized
            .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Any(static segment =>
                segment.Length >= 2 &&
                segment[1] == ':' &&
                ((segment[0] >= 'A' && segment[0] <= 'Z') ||
                 (segment[0] >= 'a' && segment[0] <= 'z')));
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
        List<string>? RouteTemplates,
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

public enum RazorVueManifestLoadStatus
{
    Success,
    FileNotFound,
    NoComponentEntries,
    Invalid
}

public sealed record RazorVueManifestLoadResult(
    string ManifestPath,
    RazorVueManifestLoadStatus Status,
    RazorVueManifestModel? Manifest,
    string? Error)
{
    public bool IsSuccess => Status == RazorVueManifestLoadStatus.Success;

    public static RazorVueManifestLoadResult Success(string manifestPath, RazorVueManifestModel manifest)
        => new(manifestPath, RazorVueManifestLoadStatus.Success, manifest, null);

    public static RazorVueManifestLoadResult FileNotFound(string manifestPath)
        => new(manifestPath, RazorVueManifestLoadStatus.FileNotFound, null, null);

    public static RazorVueManifestLoadResult NoComponentEntries(string manifestPath)
        => new(manifestPath, RazorVueManifestLoadStatus.NoComponentEntries, null, null);

    public static RazorVueManifestLoadResult Invalid(string manifestPath, string error)
        => new(manifestPath, RazorVueManifestLoadStatus.Invalid, null, error);
}
