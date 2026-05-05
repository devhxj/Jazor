using Jazor.RazorVue.Artifacts;

namespace Jazor.RazorVue.Extensibility;

internal interface IRazorVueSfcArtifactLowerer
{
    VueSfcArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot);
}
