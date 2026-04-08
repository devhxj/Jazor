using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.Emit;

internal sealed class RazorVueUpdatePlanWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public void Write(string outputPath, RazorVueManifestModel? previous, RazorVueManifestModel? current, RazorVueManifestDiffResult diff)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var plan = new RazorVueUpdatePlanModel(
            GeneratedAtUtc: DateTime.UtcNow,
            PreviousAssemblyName: previous?.AssemblyName,
            CurrentAssemblyName: current?.AssemblyName,
            Action: diff.Action,
            Reason: diff.Reason,
            TopLevelMetadataChanged: diff.TopLevelMetadataChanged,
            Modules: diff.Modules.Select(static module => new RazorVueUpdatePlanModule(
                module.AssemblyName,
                module.ComponentId,
                module.ModuleId,
                module.ComponentName,
                module.RelativeModulePath,
                module.Action,
                module.Reason,
                module.DescriptorChanged,
                module.TemplateChanged,
                module.LogicChanged,
                module.ContentChanged)).ToList());

        File.WriteAllText(outputPath, JsonSerializer.Serialize(plan, JsonOptions));
    }

    public static string GetUpdatePlanPath(string bundlePath)
        => Path.Combine(
            Path.GetDirectoryName(bundlePath) ?? string.Empty,
            Path.GetFileNameWithoutExtension(bundlePath) + ".razorvue.update-plan.json");
}

internal sealed record RazorVueUpdatePlanModel(
    DateTime GeneratedAtUtc,
    string? PreviousAssemblyName,
    string? CurrentAssemblyName,
    RazorVueHotUpdateAction Action,
    string Reason,
    bool TopLevelMetadataChanged,
    IReadOnlyList<RazorVueUpdatePlanModule> Modules);

internal sealed record RazorVueUpdatePlanModule(
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
