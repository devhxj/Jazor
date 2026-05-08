using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.RazorSdk;

internal sealed class RazorVueRazorSourceGeneratorSemanticFrontend : Jazor.RazorVue.Extensibility.IRazorSemanticFrontend
{
    private readonly ImmutableArray<RazorVueRazorSourceGeneratorDocument> _sourceGeneratorDocuments;

    public RazorVueRazorSourceGeneratorSemanticFrontend(
        ImmutableArray<RazorVueRazorSourceGeneratorDocument> sourceGeneratorDocuments)
    {
        _sourceGeneratorDocuments = sourceGeneratorDocuments.IsDefault
            ? ImmutableArray<RazorVueRazorSourceGeneratorDocument>.Empty
            : sourceGeneratorDocuments;
    }

    public string Name => "Jazor.RazorVue.RazorSdk.RazorVueRazorSourceGeneratorSemanticFrontend";

    public bool CanHandle(Jazor.RazorVue.RazorVueCompilationContext context)
        => context is not null;

    public RazorVueEntryKind ClassifyEntry(Jazor.RazorVue.RazorVueCompilationContext context, INamedTypeSymbol symbol)
        => RazorVueRazorDocumentSemanticFrontend.Instance.ClassifyEntry(context, symbol);

    public ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(Jazor.RazorVue.RazorVueCompilationContext context)
        => RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context, _sourceGeneratorDocuments);
}
