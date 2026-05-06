using System.Collections.Immutable;

namespace Jazor.RazorVue.Artifacts;

internal sealed record RazorVueSfcCatalog(
    string AssemblyName,
    ImmutableArray<VueSfcArtifact> Artifacts);
