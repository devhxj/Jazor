using System.Collections.Immutable;
using System.Linq;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.Lowering;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue;

internal sealed class RazorVueSfcPipeline
{
    private readonly IRazorSemanticFrontend _semanticFrontend;
    private readonly IRazorVueSfcArtifactLowerer _artifactLowerer;
    private readonly RazorVueSfcCatalogBuilder _catalogBuilder = new();

    public RazorVueSfcPipeline()
        : this(DefaultRazorSemanticFrontend.Instance)
    {
    }

    public RazorVueSfcPipeline(IRazorSemanticFrontend semanticFrontend)
        : this(semanticFrontend, new RazorVueSfcArtifactFactory())
    {
    }

    public RazorVueSfcPipeline(IRazorSemanticFrontend semanticFrontend, IRazorVueSfcArtifactLowerer artifactLowerer)
    {
        _semanticFrontend = semanticFrontend ?? throw new ArgumentNullException(nameof(semanticFrontend));
        _artifactLowerer = artifactLowerer ?? throw new ArgumentNullException(nameof(artifactLowerer));
    }

    public RazorVueSfcCatalog Execute(Compilation compilation)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        if (!_semanticFrontend.CanHandle(compilation))
            return _catalogBuilder.Build(compilation.AssemblyName ?? "Jazor.Assembly", ImmutableArray<VueSfcArtifact>.Empty);

        var context = RazorVueCompilationContext.TryCreate(compilation)
            ?? throw new InvalidOperationException("RazorVue compilation context was expected once the semantic frontend accepted the compilation.");

        var artifacts = _semanticFrontend.CreateSemanticSnapshots(compilation)
            .Select(snapshot => _artifactLowerer.Lower(context, snapshot))
            .ToImmutableArray();

        return _catalogBuilder.Build(compilation.AssemblyName ?? "Jazor.Assembly", artifacts);
    }

    public RazorVueSfcCatalog Execute(RazorVueCompilationContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var snapshots = _semanticFrontend.CreateSemanticSnapshots(context.Compilation);
        var artifacts = snapshots.IsDefault
            ? ImmutableArray<VueSfcArtifact>.Empty
            : snapshots
                .Select(snapshot => _artifactLowerer.Lower(context, snapshot))
                .ToImmutableArray();

        return _catalogBuilder.Build(context.Compilation.AssemblyName ?? "Jazor.Assembly", artifacts);
    }
}
