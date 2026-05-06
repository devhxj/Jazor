using System.Collections.Immutable;

namespace Jazor.RazorVue.Artifacts;

internal sealed record RazorVueCatalog(
    string AssemblyName,
    ImmutableArray<VueCompiledArtifact> Artifacts);
