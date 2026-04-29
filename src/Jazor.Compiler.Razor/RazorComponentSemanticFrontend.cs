using System;
using System.Collections.Immutable;
using Jazor.RazorVue;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Extensibility;
using Microsoft.CodeAnalysis;

namespace Jazor.Compiler.Razor;

/// <summary>
/// Razor-specific semantic extraction belongs conceptually to the Razor project.
/// The compiler core now consumes a narrow interface so this implementation can
/// become the primary frontend once cross-target registration/loading is proven.
/// </summary>
internal sealed class RazorComponentSemanticFrontend : IRazorSemanticFrontend
{
    public string Name => "Jazor.Compiler.Razor";

    public bool CanHandle(Compilation compilation)
        => RazorVueCompilationContext.TryCreate(compilation) is not null;

    public RazorVueEntryKind ClassifyEntry(Compilation compilation, INamedTypeSymbol symbol)
        => GetRequiredContext(compilation).ClassifyEntry(symbol);

    public ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(Compilation compilation)
        => GetRequiredContext(compilation).CreateSemanticSnapshots();

    private static RazorVueCompilationContext GetRequiredContext(Compilation compilation)
        => RazorVueCompilationContext.TryCreate(compilation)
           ?? throw new InvalidOperationException("RazorComponentSemanticFrontend could not create a RazorVue compilation context.");
}
