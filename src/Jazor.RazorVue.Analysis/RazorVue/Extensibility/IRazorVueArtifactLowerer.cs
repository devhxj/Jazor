using Jazor.RazorVue.Analysis.Artifacts;

namespace Jazor.RazorVue.Analysis.Extensibility;

/// <summary>
/// Converts compiler-owned semantic snapshots into Vue artifacts. Keeping this
/// contract explicit prevents the pipeline from collapsing back into direct
/// string generation.
/// </summary>
public interface IRazorVueArtifactLowerer
{
    VueCompiledArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot);

    VueCompiledArtifact Lower(RazorVueSemanticSnapshot snapshot);
}

