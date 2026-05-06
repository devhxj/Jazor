using System;
using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Extensibility;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.RazorSdk;

internal sealed class RazorVueRazorDocumentSemanticFrontend : IRazorSemanticFrontend
{
    public static RazorVueRazorDocumentSemanticFrontend Instance { get; } = new();

    private RazorVueRazorDocumentSemanticFrontend()
    {
    }

    public string Name => "Jazor.RazorVue.RazorSdk.RazorVueRazorDocumentSemanticFrontend";

    public bool CanHandle(Jazor.RazorVue.RazorVueCompilationContext context)
        => context is not null;

    public RazorVueEntryKind ClassifyEntry(Jazor.RazorVue.RazorVueCompilationContext context, INamedTypeSymbol symbol)
        => GetRequiredContext(context).ClassifyEntry(symbol);

    public ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(Jazor.RazorVue.RazorVueCompilationContext context)
    {
        var requiredContext = GetRequiredContext(context);
        var builder = ImmutableArray.CreateBuilder<RazorVueSemanticSnapshot>();

        foreach (var candidate in requiredContext.DiscoverComponentCandidates())
        {
            builder.Add(requiredContext.CreateSemanticSnapshot(
                candidate,
                RazorVueRazorDocumentLocator.TryResolvePrimaryDocumentPath(candidate),
                RazorVueRazorDocumentLocator.ResolveImportDocumentPaths(candidate)));
        }

        return builder.ToImmutable();
    }

    private static Jazor.RazorVue.RazorVueCompilationContext GetRequiredContext(Jazor.RazorVue.RazorVueCompilationContext context)
        => context ?? throw new InvalidOperationException("The Razor SDK semantic frontend expected a valid RazorVue compilation context.");
}
