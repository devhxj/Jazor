using System.Text.Json;
using System.Text.Json.Serialization;
using Jazor.RazorVue;
using Jazor.RazorVue.Emit;

namespace Jazor.Emit;

internal static class ManifestModuleKind
{
    public const string Mjs = "mjs";
    public const string Vue = "vue";
}

internal static class ManifestComponentModel
{
    public const string H = "h";
    public const string Sfc = "sfc";
}

internal sealed record ManifestModel(
    string RootAssemblyPath,
    DateTime GeneratedAtUtc,
    List<ManifestModuleEntry> Modules)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static ManifestModel? TryLoad(string manifestPath)
    {
        if (!File.Exists(manifestPath))
            return null;

        var json = File.ReadAllText(manifestPath);
        var manifest = JsonSerializer.Deserialize<ManifestModel>(json, JsonOptions);
        if (manifest is null)
            return null;

        var normalizedModules = NormalizeModules(manifest.Modules);

        return manifest with { Modules = normalizedModules };
    }

    public RazorVueManifestModel ToRazorVueManifest(string? componentModel = null)
    {
        var normalizedComponentModel = string.IsNullOrWhiteSpace(componentModel)
            ? null
            : NormalizeComponentModel(componentModel);
        var modules = Modules
            .Where(module => module.Component is not null &&
                             (normalizedComponentModel is null || module.Component.Model == normalizedComponentModel))
            .Select(ToRazorVueManifestEntry)
            .OrderBy(static module => module.RelativeModulePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.ComponentName, StringComparer.Ordinal)
            .ToList();

        if (modules.Count == 0)
        {
            return new RazorVueManifestModel(
                ResolveManifestAssemblyName(),
                GeneratedAtUtc,
                [],
                [],
                []);
        }

        return new RazorVueManifestModel(
            ResolveManifestAssemblyName(),
            GeneratedAtUtc,
            modules,
            NormalizeHostRequirementList(modules.SelectMany(static module => module.Styles).ToArray()),
            NormalizeHostRequirementList(modules.SelectMany(static module => module.PluginRequirements).ToArray()));
    }

    public ManifestModel WithRazorVueManifest(RazorVueManifestModel razorVueManifest, string componentModel)
    {
        ArgumentNullException.ThrowIfNull(razorVueManifest);

        var normalizedComponentModel = NormalizeComponentModel(componentModel);
        var modules = Modules
            .Where(module => !IsMatchingComponentModel(module, normalizedComponentModel))
            .Concat(razorVueManifest.Modules.Select(module => ToManifestModule(module, normalizedComponentModel)))
            .OrderBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.TypeName, StringComparer.Ordinal)
            .ToList();

        return this with
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Modules = modules
        };
    }

    public void Save(string manifestPath)
    {
        var directory = Path.GetDirectoryName(manifestPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var normalized = this with
        {
            Modules = NormalizeModules(Modules)
        };

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(normalized, JsonOptions));
    }

    private static List<ManifestModuleEntry> NormalizeModules(IEnumerable<ManifestModuleEntry> modules)
    {
        var normalizedModules = new List<ManifestModuleEntry>();
        var indexByRelativePath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var module in modules)
        {
            var normalizedModule = NormalizeModule(module);
            if (indexByRelativePath.TryGetValue(normalizedModule.RelativePath, out var existingIndex))
            {
                // Relative paths are the physical file identity for the unified manifest.
                // Keeping the later entry lets newer writer ownership heal stale duplicate rows
                // produced by older mixed-writer runs.
                normalizedModules[existingIndex] = normalizedModule;
                continue;
            }

            indexByRelativePath.Add(normalizedModule.RelativePath, normalizedModules.Count);
            normalizedModules.Add(normalizedModule);
        }

        return normalizedModules;
    }

    private static ManifestModuleEntry NormalizeModule(ManifestModuleEntry module)
    {
        var relativePath = NormalizeRelativePath(module.RelativePath);
        var component = NormalizeComponentMetadata(module, relativePath);
        var kind = NormalizeKind(module.Kind);
        ValidateModuleKindForComponent(kind, component, relativePath);

        return module with
        {
            RelativePath = relativePath,
            SourceMapPath = module.SourceMapPath is null ? null : NormalizeRelativePath(module.SourceMapPath),
            Kind = kind,
            Component = component
        };
    }

    private static string NormalizeKind(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
            return ManifestModuleKind.Mjs;

        var normalized = kind.Trim().ToLowerInvariant();
        if (normalized is ManifestModuleKind.Mjs or ManifestModuleKind.Vue)
            return normalized;

        throw new InvalidOperationException("Unsupported manifest module kind '" + kind + "'.");
    }

    private static ManifestComponentMetadata? NormalizeComponentMetadata(ManifestModuleEntry module, string relativePath)
    {
        if (module.Component is null)
            return null;

        var componentModel = NormalizeComponentModel(module.Component.Model);
        return module.Component with
        {
            Model = componentModel,
            ComponentId = NormalizeValue(module.Component.ComponentId, module.AssemblyName + "::" + module.TypeName),
            ModuleId = NormalizeValue(module.Component.ModuleId, relativePath),
            ComponentName = NormalizeValue(module.Component.ComponentName, module.TypeName),
            RouteTemplates = NormalizeRouteTemplates(module.Component.RouteTemplates),
            OriginMapPath = NormalizeRelativePath(NormalizeValue(module.Component.OriginMapPath, relativePath + ".origins.json")),
            Imports = NormalizeHostRequirementList(module.Component.Imports),
            Styles = NormalizeHostRequirementList(module.Component.Styles),
            PluginRequirements = NormalizeHostRequirementList(module.Component.PluginRequirements),
            StyleHash = module.Component.StyleHash ?? string.Empty
        };
    }

    private static void ValidateModuleKindForComponent(string kind, ManifestComponentMetadata? component, string relativePath)
    {
        if (component is null)
            return;

        if (component.Model == ManifestComponentModel.Sfc && kind != ManifestModuleKind.Vue)
            throw new InvalidOperationException($"SFC component manifest module '{relativePath}' must use kind '{ManifestModuleKind.Vue}'.");

        if (component.Model == ManifestComponentModel.H && kind != ManifestModuleKind.Mjs)
            throw new InvalidOperationException($"H component manifest module '{relativePath}' must use kind '{ManifestModuleKind.Mjs}'.");
    }

    private static string NormalizeComponentModel(string? model)
    {
        if (string.IsNullOrWhiteSpace(model))
            return ManifestComponentModel.H;

        var normalized = model.Trim().ToLowerInvariant();
        if (normalized is ManifestComponentModel.H or ManifestComponentModel.Sfc)
            return normalized;

        throw new InvalidOperationException("Unsupported manifest component model '" + model + "'.");
    }

    private static string NormalizeValue(string? value, string fallbackValue)
        => string.IsNullOrWhiteSpace(value) ? fallbackValue : value.Trim();

    private static List<string> NormalizeHostRequirementList(IReadOnlyList<string>? values)
        => RazorVueManifestSerializer.NormalizeHostRequirementList(values ?? []);

    private static List<string> NormalizeRouteTemplates(IReadOnlyList<string>? values)
        => RazorVueManifestSerializer.NormalizeRouteTemplates(values ?? []);

    private static bool IsMatchingComponentModel(ManifestModuleEntry module, string componentModel)
        => module.Component?.Model == componentModel;

    private static ManifestModuleEntry ToManifestModule(RazorVueManifestEntry module, string componentModel)
        => new(
            module.AssemblyName,
            module.ComponentName,
            module.ComponentId,
            module.RelativeModulePath,
            module.ContentHash,
            module.SourceMapPath,
            MapHash: null,
            ResolveArtifactKind(module.RelativeModulePath),
            new ManifestComponentMetadata(
                componentModel,
                module.ComponentId,
                module.ModuleId,
                module.ComponentName,
                RazorVueManifestSerializer.NormalizeRouteTemplates(module.RouteTemplates),
                module.OriginMapPath,
                NormalizeHostRequirementList(module.Imports),
                NormalizeHostRequirementList(module.Styles),
                NormalizeHostRequirementList(module.PluginRequirements),
                module.DescriptorHash,
                module.TemplateHash,
                module.LogicHash,
                module.ContentHash,
                module.HmrBoundaryKind,
                module.RequiresHydration,
                module.SupportsSsr,
                module.StyleHash ?? string.Empty));

    private static RazorVueManifestEntry ToRazorVueManifestEntry(ManifestModuleEntry module)
    {
        var component = module.Component
            ?? throw new InvalidOperationException("Manifest module does not contain component metadata.");

        return new RazorVueManifestEntry(
            module.AssemblyName,
            component.ComponentId,
            component.ModuleId,
            component.ComponentName,
            component.RouteTemplates,
            module.RelativePath,
            module.SourceMapPath ?? module.RelativePath + ".map",
            component.OriginMapPath,
            component.Imports,
            component.Styles,
            component.PluginRequirements,
            component.DescriptorHash,
            component.TemplateHash,
            component.LogicHash,
            component.ContentHash,
            component.HmrBoundaryKind,
            component.RequiresHydration,
            component.SupportsSsr,
            component.StyleHash ?? string.Empty,
            component.Model);
    }

    private static string ResolveArtifactKind(string relativeModulePath)
        => relativeModulePath.EndsWith(".vue", StringComparison.OrdinalIgnoreCase)
            ? ManifestModuleKind.Vue
            : ManifestModuleKind.Mjs;

    private string ResolveManifestAssemblyName()
    {
        if (Modules.Select(static module => module.AssemblyName).Distinct(StringComparer.Ordinal).Take(2).Count() == 1)
            return Modules[0].AssemblyName;

        var fileName = Path.GetFileNameWithoutExtension(RootAssemblyPath);
        return string.IsNullOrWhiteSpace(fileName) ? "Jazor.Emit" : fileName;
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("Manifest relative path cannot be empty.");

        if (Path.IsPathRooted(normalized))
            throw new InvalidOperationException($"Manifest relative path must be relative: '{relativePath}'.");

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"Manifest relative path cannot escape output directory: '{relativePath}'.");

        return string.Join("/", segments);
    }
}

internal sealed record ManifestModuleEntry(
    string AssemblyName,
    string TypeName,
    string Id,
    string RelativePath,
    string Hash,
    string? SourceMapPath = null,
    string? MapHash = null,
    string Kind = ManifestModuleKind.Mjs,
    ManifestComponentMetadata? Component = null);

internal sealed record ManifestComponentMetadata(
    string Model,
    string ComponentId,
    string ModuleId,
    string ComponentName,
    List<string> RouteTemplates,
    string OriginMapPath,
    List<string> Imports,
    List<string> Styles,
    List<string> PluginRequirements,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    string ContentHash,
    RazorVueHmrBoundaryKind HmrBoundaryKind,
    bool RequiresHydration,
    bool SupportsSsr,
    string StyleHash = "");
