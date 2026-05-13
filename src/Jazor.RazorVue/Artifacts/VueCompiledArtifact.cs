using System.Collections.Immutable;

namespace Jazor.RazorVue.Artifacts;

internal sealed record VueCompiledArtifact(
    string ComponentName,
    string RelativeModulePath,
    string ModuleCode,
    ImmutableArray<string> RouteTemplates,
    ImmutableArray<string> Imports,
    ImmutableArray<string> Styles,
    ImmutableArray<string> PluginRequirements,
    VueArtifactIdentity Identity,
    VueRuntimeHints Hints,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record VueArtifactIdentity(
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    HmrBoundaryKind HmrBoundaryKind);

internal sealed record VueRuntimeHints(
    bool RequiresVueRuntime,
    bool RequiresHydration,
    bool SupportsSsr,
    bool UsesTeleport,
    bool UsesSuspense,
    bool UsesKeepAlive);

internal enum HmrBoundaryKind
{
    Unknown,
    TemplateOnly,
    LogicSafe,
    FullReloadRequired
}
