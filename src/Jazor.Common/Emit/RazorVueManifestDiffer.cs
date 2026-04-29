using System.Collections.Generic;
using System.Linq;
using ECMAScript.Contract.RazorVue;

namespace ECMAScript.Contract.Emit;

public static class RazorVueManifestDiffer
{
    public static RazorVueManifestDiffResult Diff(RazorVueManifestModel? previous, RazorVueManifestModel? current)
    {
        if (previous is null || current is null)
        {
            return new RazorVueManifestDiffResult(
                RazorVueHotUpdateAction.FullReload,
                previous is null
                    ? "Previous RazorVue manifest is missing."
                    : "Current RazorVue manifest is missing.",
                [],
                TopLevelMetadataChanged: true);
        }

        var moduleDiffs = new List<RazorVueManifestModuleDiff>();
        var topLevelReason = GetTopLevelFullReloadReason(previous, current);
        var previousByComponentId = previous.Modules.ToDictionary(
            static module => module.ComponentId,
            StringComparer.Ordinal);
        var currentByComponentId = current.Modules.ToDictionary(
            static module => module.ComponentId,
            StringComparer.Ordinal);

        foreach (var currentModule in current.Modules)
        {
            if (!previousByComponentId.TryGetValue(currentModule.ComponentId, out var previousModule))
            {
                moduleDiffs.Add(CreateFullReloadDiff(currentModule, "Component was added or its stable identity changed."));
                continue;
            }

            moduleDiffs.Add(DiffModule(previousModule, currentModule));
        }

        foreach (var previousModule in previous.Modules)
        {
            if (!currentByComponentId.ContainsKey(previousModule.ComponentId))
                moduleDiffs.Add(CreateFullReloadDiff(previousModule, "Component was removed from the manifest."));
        }

        if (!string.IsNullOrWhiteSpace(topLevelReason))
        {
            // Host-level dependency drift invalidates the whole update plan even if
            // individual modules look hot-safe in isolation.
            var fullReloadReason = topLevelReason!;
            moduleDiffs = current.Modules
                .Select(module => CreateFullReloadDiff(module, fullReloadReason))
                .Concat(moduleDiffs.Where(static diff => diff.Action == RazorVueHotUpdateAction.FullReload))
                .GroupBy(static diff => diff.ComponentId, StringComparer.Ordinal)
                .Select(static group => group.First())
                .OrderBy(static diff => diff.RelativeModulePath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static diff => diff.ComponentName, StringComparer.Ordinal)
                .ToList();
        }
        else
        {
            moduleDiffs = moduleDiffs
                .OrderBy(static diff => diff.RelativeModulePath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static diff => diff.ComponentName, StringComparer.Ordinal)
                .ToList();
        }

        return new RazorVueManifestDiffResult(
            ComputeOverallAction(moduleDiffs, !string.IsNullOrWhiteSpace(topLevelReason)),
            topLevelReason ?? GetPrimaryReason(moduleDiffs),
            moduleDiffs,
            TopLevelMetadataChanged: !string.IsNullOrWhiteSpace(topLevelReason));
    }

    private static RazorVueManifestModuleDiff DiffModule(RazorVueManifestEntry previous, RazorVueManifestEntry current)
    {
        var descriptorChanged = !StringComparer.Ordinal.Equals(previous.DescriptorHash, current.DescriptorHash);
        var templateChanged = !StringComparer.Ordinal.Equals(previous.TemplateHash, current.TemplateHash);
        var logicChanged = !StringComparer.Ordinal.Equals(previous.LogicHash, current.LogicHash);
        var contentChanged = !StringComparer.Ordinal.Equals(previous.ContentHash, current.ContentHash);

        if (HasIdentityOrContractDrift(previous, current))
            return CreateFullReloadDiff(current, "Component identity or host contract changed.", descriptorChanged, templateChanged, logicChanged, contentChanged);

        if (!descriptorChanged && !templateChanged && !logicChanged && !contentChanged)
            return new RazorVueManifestModuleDiff(current.AssemblyName, current.ComponentId, current.ModuleId, current.ComponentName, current.RelativeModulePath, RazorVueHotUpdateAction.None, "No material change.", false, false, false, false);

        if (descriptorChanged)
            return CreateFullReloadDiff(current, "Public component descriptor changed.", descriptorChanged, templateChanged, logicChanged, contentChanged);

        if (current.HmrBoundaryKind is RazorVueHmrBoundaryKind.FullReloadRequired or RazorVueHmrBoundaryKind.Unknown ||
            previous.HmrBoundaryKind is RazorVueHmrBoundaryKind.FullReloadRequired or RazorVueHmrBoundaryKind.Unknown)
        {
            return CreateFullReloadDiff(current, "HMR boundary does not prove a hot-safe update.", descriptorChanged, templateChanged, logicChanged, contentChanged);
        }

        if (logicChanged)
        {
            if (current.HmrBoundaryKind == RazorVueHmrBoundaryKind.LogicSafe && !descriptorChanged)
            {
                return new RazorVueManifestModuleDiff(
                    current.AssemblyName,
                    current.ComponentId,
                    current.ModuleId,
                    current.ComponentName,
                    current.RelativeModulePath,
                    RazorVueHotUpdateAction.LogicPatch,
                    templateChanged
                        ? "Template and logic changed within a logic-safe boundary."
                        : "Logic changed within a logic-safe boundary.",
                    descriptorChanged,
                    templateChanged,
                    logicChanged,
                    contentChanged);
            }

            return CreateFullReloadDiff(current, "Logic changed outside a logic-safe boundary.", descriptorChanged, templateChanged, logicChanged, contentChanged);
        }

        if (templateChanged)
        {
            return new RazorVueManifestModuleDiff(
                current.AssemblyName,
                current.ComponentId,
                current.ModuleId,
                current.ComponentName,
                current.RelativeModulePath,
                RazorVueHotUpdateAction.TemplatePatch,
                "Template hash changed while descriptor and logic stayed stable.",
                descriptorChanged,
                templateChanged,
                logicChanged,
                contentChanged);
        }

        return CreateFullReloadDiff(current, "Module content changed outside split hash classification.", descriptorChanged, templateChanged, logicChanged, contentChanged);
    }

    private static bool HasIdentityOrContractDrift(RazorVueManifestEntry previous, RazorVueManifestEntry current)
        => !StringComparer.Ordinal.Equals(previous.AssemblyName, current.AssemblyName) ||
           !StringComparer.Ordinal.Equals(previous.ComponentId, current.ComponentId) ||
           !StringComparer.Ordinal.Equals(previous.ModuleId, current.ModuleId) ||
           !StringComparer.Ordinal.Equals(previous.RelativeModulePath, current.RelativeModulePath) ||
           !StringComparer.Ordinal.Equals(previous.SourceMapPath, current.SourceMapPath) ||
           !StringComparer.Ordinal.Equals(previous.OriginMapPath, current.OriginMapPath) ||
           previous.RequiresHydration != current.RequiresHydration ||
           previous.SupportsSsr != current.SupportsSsr ||
           !(previous.Imports ?? []).SequenceEqual(current.Imports ?? [], StringComparer.Ordinal) ||
           !(previous.Styles ?? []).SequenceEqual(current.Styles ?? [], StringComparer.Ordinal) ||
           !(previous.PluginRequirements ?? []).SequenceEqual(current.PluginRequirements ?? [], StringComparer.Ordinal);

    private static string? GetTopLevelFullReloadReason(RazorVueManifestModel previous, RazorVueManifestModel current)
    {
        if (!(previous.Styles ?? []).SequenceEqual(current.Styles ?? [], StringComparer.Ordinal))
            return "Top-level RazorVue style requirements changed.";

        if (!(previous.PluginRequirements ?? []).SequenceEqual(current.PluginRequirements ?? [], StringComparer.Ordinal))
            return "Top-level RazorVue plugin requirements changed.";

        return null;
    }

    private static RazorVueManifestModuleDiff CreateFullReloadDiff(
        RazorVueManifestEntry module,
        string reason,
        bool descriptorChanged = false,
        bool templateChanged = false,
        bool logicChanged = false,
        bool contentChanged = false)
        => new(
            module.AssemblyName,
            module.ComponentId,
            module.ModuleId,
            module.ComponentName,
            module.RelativeModulePath,
            RazorVueHotUpdateAction.FullReload,
            reason,
            descriptorChanged,
            templateChanged,
            logicChanged,
            contentChanged);

    private static RazorVueHotUpdateAction ComputeOverallAction(IReadOnlyList<RazorVueManifestModuleDiff> moduleDiffs, bool topLevelMetadataChanged)
    {
        if (topLevelMetadataChanged)
            return RazorVueHotUpdateAction.FullReload;

        if (moduleDiffs.Any(static diff => diff.Action == RazorVueHotUpdateAction.FullReload))
            return RazorVueHotUpdateAction.FullReload;

        if (moduleDiffs.Any(static diff => diff.Action == RazorVueHotUpdateAction.LogicPatch))
            return RazorVueHotUpdateAction.LogicPatch;

        if (moduleDiffs.Any(static diff => diff.Action == RazorVueHotUpdateAction.TemplatePatch))
            return RazorVueHotUpdateAction.TemplatePatch;

        return RazorVueHotUpdateAction.None;
    }

    private static string GetPrimaryReason(IReadOnlyList<RazorVueManifestModuleDiff> moduleDiffs)
        => moduleDiffs.FirstOrDefault(static diff => diff.Action != RazorVueHotUpdateAction.None)?.Reason
           ?? "No material change.";
}

public sealed record RazorVueManifestDiffResult(
    RazorVueHotUpdateAction Action,
    string Reason,
    IReadOnlyList<RazorVueManifestModuleDiff> Modules,
    bool TopLevelMetadataChanged);

public sealed record RazorVueManifestModuleDiff(
    string AssemblyName,
    string ComponentId,
    string ModuleId,
    string ComponentName,
    string RelativeModulePath,
    RazorVueHotUpdateAction Action,
    string Reason,
    bool DescriptorChanged,
    bool TemplateChanged,
    bool LogicChanged,
    bool ContentChanged);

public enum RazorVueHotUpdateAction
{
    None,
    TemplatePatch,
    LogicPatch,
    FullReload
}
