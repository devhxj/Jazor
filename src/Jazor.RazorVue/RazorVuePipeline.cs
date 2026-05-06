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

    public RazorVuePipeline(IRazorVueTemplateFrontend templateFrontend)
        : this(DefaultRazorSemanticFrontend.Instance, templateFrontend)
    {
    }

    public RazorVuePipeline(IRazorSemanticFrontend semanticFrontend, IRazorVueTemplateFrontend templateFrontend)
        : this(semanticFrontend, new RazorVueArtifactFactory(templateFrontend))
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

        var context = RazorVueCompilationContext.TryCreate(compilation);
        if (context is null || !_semanticFrontend.CanHandle(context))
            return _catalogBuilder.Build(compilation.AssemblyName ?? "Jazor.Assembly", ImmutableArray<VueCompiledArtifact>.Empty);

        var artifacts = _semanticFrontend.CreateSemanticSnapshots(context)
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

        if (!_semanticFrontend.CanHandle(context))
            return _catalogBuilder.Build(context.Compilation.AssemblyName ?? "Jazor.Assembly", ImmutableArray<VueCompiledArtifact>.Empty);

        var snapshots = _semanticFrontend.CreateSemanticSnapshots(context);
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
