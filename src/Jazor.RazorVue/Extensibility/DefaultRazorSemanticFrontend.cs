using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Extensibility;

/// <summary>
/// Built-in semantic frontend that projects RazorVue snapshots from a Roslyn compilation.
/// </summary>
internal sealed class DefaultRazorSemanticFrontend : IRazorSemanticFrontend
{
    public static DefaultRazorSemanticFrontend Instance { get; } = new();

    private DefaultRazorSemanticFrontend()
    {
    }

    public string Name => "Jazor.Compiler.DefaultRazorFrontend";

    public bool CanHandle(RazorVueCompilationContext context)
        => context is not null;

    public RazorVueEntryKind ClassifyEntry(RazorVueCompilationContext context, INamedTypeSymbol symbol)
        => GetRequiredContext(context).ClassifyEntry(symbol);

    public ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(RazorVueCompilationContext context)
        => GetRequiredContext(context).CreateSemanticSnapshots();

    private static RazorVueCompilationContext GetRequiredContext(RazorVueCompilationContext context)
        => context ?? throw new InvalidOperationException("The default Razor semantic frontend expected a valid RazorVue compilation context.");
}
