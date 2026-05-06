using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Extensibility;

/// <summary>
/// Defines the narrow seam between compiler orchestration and Razor-specific
/// semantic extraction. The long-term goal is for Razor-owned projects to
/// implement this contract without forcing Jazor.Compiler to own every
/// frontend detail forever.
/// </summary>
internal interface IRazorSemanticFrontend
{
    string Name { get; }

    bool CanHandle(RazorVueCompilationContext context);

    RazorVueEntryKind ClassifyEntry(RazorVueCompilationContext context, INamedTypeSymbol symbol);

    ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(RazorVueCompilationContext context);
}
