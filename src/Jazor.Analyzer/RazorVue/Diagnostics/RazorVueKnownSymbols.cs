using Jazor.RazorVue;
using Jazor.RazorVue.Discovery;
using Microsoft.CodeAnalysis;

namespace Jazor.Analyzer;

/// <summary>
/// Analyzer-side facade over <see cref="RazorVueCompilationSymbols"/> with
/// commonly used classification and misuse checks.
/// </summary>
internal sealed class RazorVueKnownSymbols
{
    private RazorVueKnownSymbols(RazorVueCompilationSymbols symbols)
    {
        Symbols = symbols;
    }

    public RazorVueCompilationSymbols Symbols { get; }

    public static RazorVueKnownSymbols? TryCreate(Compilation compilation)
    {
        var symbols = RazorVueCompilationSymbols.TryCreate(compilation);
        return symbols is null ? null : new RazorVueKnownSymbols(symbols);
    }

    public bool HasECMAScriptModuleAttribute(INamedTypeSymbol symbol)
        => RazorVueEntryClassifier.HasECMAScriptModuleAttribute(symbol, Symbols);

    public RazorVueEntryKind Classify(INamedTypeSymbol symbol)
        => RazorVueEntryClassifier.Classify(symbol, Symbols);

    public bool IsDirectComponentBaseEntry(INamedTypeSymbol symbol)
        => RazorVueEntryClassifier.IsDirectComponentBaseEntry(symbol, Symbols);

    public bool IsRazorVueComponent(INamedTypeSymbol symbol)
        => Classify(symbol) == RazorVueEntryKind.RazorVueComponent;

    public bool IsInRazorVueScope(INamedTypeSymbol symbol)
        => RazorVueEntryClassifier.IsInRazorVueScope(symbol, Symbols);

    public bool IsStateHasChanged(IMethodSymbol method)
        => method.Name == "StateHasChanged" &&
           DerivesFrom(method.ContainingType, Symbols.ComponentBase);

    public bool IsShouldRender(IMethodSymbol method)
        => method.Name == "ShouldRender" &&
           method.Parameters.Length == 0 &&
           method.ReturnType.SpecialType == SpecialType.System_Boolean &&
           DerivesFrom(method.ContainingType, Symbols.ComponentBase);

    public bool IsSupportedLifecycleCandidate(IMethodSymbol method)
        => method.Name is "OnInitialized" or "OnInitializedAsync" or "OnParametersSet" or "OnParametersSetAsync" or "OnAfterRender" or "OnAfterRenderAsync" or "Dispose" or "DisposeAsync" &&
           DerivesFrom(method.ContainingType, Symbols.ComponentBase);

    public bool IsSetParametersAsync(IMethodSymbol method)
    {
        if (method.Name != "SetParametersAsync" || method.Parameters.Length != 1)
            return false;

        if (Symbols.ParameterView is not null &&
            !SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type.OriginalDefinition, Symbols.ParameterView))
        {
            return false;
        }

        return DerivesFrom(method.ContainingType, Symbols.ComponentBase);
    }

    private static bool DerivesFrom(ITypeSymbol? symbol, INamedTypeSymbol baseType)
    {
        for (var current = symbol; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseType))
                return true;
        }

        return false;
    }
}
