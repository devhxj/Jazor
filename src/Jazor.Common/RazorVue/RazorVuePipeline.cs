using System;
using System.Collections.Immutable;
using System.Linq;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.Lowering;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue;

// RazorVuePipeline now lives in Jazor.RazorVue because it is RazorVue core orchestration,
// not Roslyn generator host glue. Jazor.RazorVue.Analysis should only call into this type.
internal sealed class RazorVuePipeline
{
    // Seam: Roslyn-facing hosts may invoke this pipeline, but RazorVue semantic ownership lives in Jazor.RazorVue.

    private readonly IRazorSemanticFrontend _semanticFrontend;
    private readonly IRazorVueArtifactLowerer _artifactLowerer;
    private readonly RazorVueCatalogBuilder _catalogBuilder = new();

    public RazorVuePipeline()
        : this(DefaultRazorSemanticFrontend.Instance)
    {
    }

    public RazorVuePipeline(IRazorSemanticFrontend semanticFrontend)
        : this(semanticFrontend, new RazorVueArtifactFactory())
    {
    }

    public RazorVuePipeline(IRazorSemanticFrontend semanticFrontend, IRazorVueArtifactLowerer artifactLowerer)
    {
        _semanticFrontend = semanticFrontend ?? throw new ArgumentNullException(nameof(semanticFrontend));
        _artifactLowerer = artifactLowerer ?? throw new ArgumentNullException(nameof(artifactLowerer));
    }

    public RazorVueCatalog Execute(Compilation compilation)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        if (!_semanticFrontend.CanHandle(compilation))
            return _catalogBuilder.Build(compilation.AssemblyName ?? "Jazor.Assembly", ImmutableArray<VueCompiledArtifact>.Empty);

        // Resolve the shared compilation context once per pipeline execution so
        // the semantic frontend and lowerer stay aligned on the same snapshot view.
        var context = RazorVueCompilationContext.TryCreate(compilation)
            ?? throw new InvalidOperationException("RazorVue compilation context was expected once the semantic frontend accepted the compilation.");

        var artifacts = _semanticFrontend.CreateSemanticSnapshots(compilation)
            .Select(snapshot => _artifactLowerer.Lower(
                context,
                snapshot))
            .ToImmutableArray();

        return _catalogBuilder.Build(compilation.AssemblyName ?? "Jazor.Assembly", artifacts);
    }

    public RazorVueCatalog Execute(RazorVueCompilationContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var snapshots = _semanticFrontend.CreateSemanticSnapshots(context.Compilation);
        var artifacts = snapshots.IsDefault
            ? ImmutableArray<VueCompiledArtifact>.Empty
            : snapshots
                .Select(snapshot => _artifactLowerer.Lower(context, snapshot))
                .ToImmutableArray();

        return _catalogBuilder.Build(context.Compilation.AssemblyName ?? "Jazor.Assembly", artifacts);
    }

    public RazorVueCatalog Execute(string assemblyName, ImmutableArray<RazorVueSemanticSnapshot> snapshots)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
            throw new ArgumentException("Assembly name cannot be empty.", nameof(assemblyName));

        var artifacts = snapshots.IsDefault
            ? ImmutableArray<VueCompiledArtifact>.Empty
            : snapshots
                .Select(snapshot => _artifactLowerer.Lower(snapshot))
                .ToImmutableArray();

        return _catalogBuilder.Build(assemblyName, artifacts);
    }
}
