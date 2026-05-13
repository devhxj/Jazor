using System.Collections.Generic;

namespace Jazor.Emit;

internal sealed record RazorVueCatalogRecord(
    string AssemblyName,
    IReadOnlyList<RazorVueEmitArtifactRecord> Artifacts);

internal sealed record RazorVueEmitArtifactRecord(
    string ComponentName,
    string RelativeModulePath,
    string ModuleCode,
    IReadOnlyList<string> RouteTemplates,
    IReadOnlyList<string> Imports,
    IReadOnlyList<string> Styles,
    IReadOnlyList<string> PluginRequirements,
    RazorVueEmitArtifactIdentity Identity,
    RazorVueEmitRuntimeHints Hints,
    IReadOnlyList<RazorVueEmitSourceOriginRecord> SourceOrigins);

internal sealed record RazorVueEmitArtifactIdentity(
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    RazorVueHmrBoundaryKind HmrBoundaryKind);

internal sealed record RazorVueEmitRuntimeHints(
    bool RequiresVueRuntime,
    bool RequiresHydration,
    bool SupportsSsr,
    bool UsesTeleport,
    bool UsesSuspense,
    bool UsesKeepAlive);

internal sealed record RazorVueEmitSourceOriginRecord(
    string SourceFilePath,
    int SourceSpanStart,
    int SourceSpanLength,
    string? GeneratedFilePath,
    int? GeneratedSpanStart,
    int? GeneratedSpanLength,
    int StartLine,
    int StartColumn,
    RazorVueMappingQualityRecord MappingQuality,
    RazorVueOriginProvenanceRecord Provenance);

internal enum RazorVueMappingQualityRecord
{
    ExactSource,
    MappedFromGenerated,
    GeneratedOnly
}

internal enum RazorVueOriginProvenanceRecord
{
    RazorSourceMap,
    GeneratedSyntaxLocation,
    GeneratedFallback
}
