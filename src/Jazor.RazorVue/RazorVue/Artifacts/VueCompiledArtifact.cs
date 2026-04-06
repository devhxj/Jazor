using System.Collections.Immutable;

namespace Jazor.RazorVue.Artifacts;

public sealed record VueCompiledArtifact(
    string ComponentName,
    string RelativeModulePath,
    string ModuleCode,
    ImmutableArray<string> Imports,
    ImmutableArray<string> Styles,
    VueArtifactIdentity Identity,
    VueRuntimeHints Hints,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

public sealed record VueArtifactIdentity(
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    HmrBoundaryKind HmrBoundaryKind);

public sealed record VueRuntimeHints(
    bool RequiresVueRuntime,
    bool RequiresHydration,
    bool SupportsSsr,
    bool UsesTeleport,
    bool UsesSuspense,
    bool UsesKeepAlive);

public enum HmrBoundaryKind
{
    Unknown,
    TemplateOnly,
    LogicSafe,
    FullReloadRequired
}
