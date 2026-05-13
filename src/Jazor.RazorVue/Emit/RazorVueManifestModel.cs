using System.Collections.Generic;
using Jazor.RazorVue;

namespace Jazor.RazorVue.Emit;

public sealed record RazorVueManifestModel(
    string AssemblyName,
    DateTime GeneratedAtUtc,
    List<RazorVueManifestEntry> Modules,
    List<string>? Styles = null,
    List<string>? PluginRequirements = null);

public sealed record RazorVueManifestEntry(
    string AssemblyName,
    string ComponentId,
    string ModuleId,
    string ComponentName,
    List<string> RouteTemplates,
    string RelativeModulePath,
    string SourceMapPath,
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
    string StyleHash = "",
    string ComponentModel = "h");
