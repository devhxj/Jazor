using System.Collections.Immutable;

namespace Jazor.RazorVue.Artifacts;

public sealed record RazorVueCatalog(
    string AssemblyName,
    ImmutableArray<VueCompiledArtifact> Artifacts);
