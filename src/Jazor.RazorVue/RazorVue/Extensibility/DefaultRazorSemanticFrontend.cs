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

    public bool CanHandle(Compilation compilation)
        => RazorVueCompilationContext.TryCreate(compilation) is not null;

    public RazorVueEntryKind ClassifyEntry(Compilation compilation, INamedTypeSymbol symbol)
        => GetRequiredContext(compilation).ClassifyEntry(symbol);

    public ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(Compilation compilation)
        => GetRequiredContext(compilation).CreateSemanticSnapshots();

    private static RazorVueCompilationContext GetRequiredContext(Compilation compilation)
        // Keep a compiler-local fallback until the Razor project becomes the
        // primary semantic frontend through a proven registration/loading path.
        => RazorVueCompilationContext.TryCreate(compilation)
           ?? throw new InvalidOperationException("The default Razor semantic frontend could not create a RazorVue compilation context.");
}

