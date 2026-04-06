using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Discovery;

internal static class RazorVueEntryClassifier
{
    private static readonly SymbolEqualityComparer Comparer = SymbolEqualityComparer.Default;

    public static bool HasECMAScriptModuleAttribute(INamedTypeSymbol symbol, RazorVueCompilationSymbols symbols)
        => symbol.GetAttributes().Any(attribute => Comparer.Equals(attribute.AttributeClass, symbols.ECMAScriptModuleAttribute));

    public static RazorVueEntryKind Classify(INamedTypeSymbol symbol, RazorVueCompilationSymbols symbols)
    {
        if (!HasECMAScriptModuleAttribute(symbol, symbols))
            return RazorVueEntryKind.None;

        // Static module classes stay on the existing path even though they share
        // the same [ECMAScriptModule] entry marker with RazorVue components.
        if (symbol.IsStatic)
            return RazorVueEntryKind.StaticModule;

        if (!DerivesFrom(symbol, symbols.ComponentBase))
            return RazorVueEntryKind.None;

        return DerivesFrom(symbol, symbols.JazorComponent)
            ? RazorVueEntryKind.RazorVueComponent
            : RazorVueEntryKind.Invalid;
    }

    public static bool IsDirectComponentBaseEntry(INamedTypeSymbol symbol, RazorVueCompilationSymbols symbols)
        => HasECMAScriptModuleAttribute(symbol, symbols) &&
           !symbol.IsStatic &&
           Comparer.Equals(symbol.BaseType?.OriginalDefinition, symbols.ComponentBase);

    public static bool IsInRazorVueScope(INamedTypeSymbol symbol, RazorVueCompilationSymbols symbols)
    {
        for (var current = symbol; current is not null; current = current.ContainingType)
        {
            var entryKind = Classify(current, symbols);
            if (entryKind is RazorVueEntryKind.RazorVueComponent or RazorVueEntryKind.Invalid)
                return true;
        }

        return false;
    }

    public static IMethodSymbol? FindBuildRenderTreeMethod(INamedTypeSymbol symbol)
        => FindHierarchyMethod(symbol, "BuildRenderTree", static method =>
            method.Parameters.Length == 1 &&
            method.MethodKind == MethodKind.Ordinary);

    public static IMethodSymbol? FindOnInitializedMethod(INamedTypeSymbol symbol)
        => FindOrdinaryMethod(symbol, "OnInitialized", parameterCount: 0);

    public static IMethodSymbol? FindOnInitializedAsyncMethod(INamedTypeSymbol symbol)
        => FindOrdinaryMethod(symbol, "OnInitializedAsync", parameterCount: 0);

    public static IMethodSymbol? FindOnParametersSetMethod(INamedTypeSymbol symbol)
        => FindOrdinaryMethod(symbol, "OnParametersSet", parameterCount: 0);

    public static IMethodSymbol? FindOnParametersSetAsyncMethod(INamedTypeSymbol symbol)
        => FindOrdinaryMethod(symbol, "OnParametersSetAsync", parameterCount: 0);

    public static IMethodSymbol? FindOnAfterRenderMethod(INamedTypeSymbol symbol)
        => FindOrdinaryMethod(symbol, "OnAfterRender", parameterCount: 1);

    public static IMethodSymbol? FindOnAfterRenderAsyncMethod(INamedTypeSymbol symbol)
        => FindOrdinaryMethod(symbol, "OnAfterRenderAsync", parameterCount: 1);

    public static IMethodSymbol? FindShouldRenderMethod(INamedTypeSymbol symbol)
        => FindOrdinaryMethod(symbol, "ShouldRender", parameterCount: 0);

    public static IMethodSymbol? FindSetParametersAsyncMethod(INamedTypeSymbol symbol)
        => FindOrdinaryMethod(symbol, "SetParametersAsync", parameterCount: 1);

    public static IMethodSymbol? FindDisposeMethod(INamedTypeSymbol symbol)
        => FindOrdinaryMethod(symbol, "Dispose", parameterCount: 0);

    public static IMethodSymbol? FindDisposeAsyncMethod(INamedTypeSymbol symbol)
        => FindOrdinaryMethod(symbol, "DisposeAsync", parameterCount: 0);

    public static ImmutableArray<IMethodSymbol> FindLogicMethods(INamedTypeSymbol symbol)
    {
        var builder = ImmutableArray.CreateBuilder<IMethodSymbol>();
        var seenSignatures = new HashSet<string>(StringComparer.Ordinal);

        for (var current = symbol; current is not null; current = current.BaseType)
        {
            foreach (var method in current.GetMembers().OfType<IMethodSymbol>())
            {
                if (method.MethodKind != MethodKind.Ordinary || method.IsStatic)
                    continue;

                if (!method.Locations.Any(static location => location.IsInSource))
                    continue;

                if (method.Name is "BuildRenderTree" or "OnInitialized" or "OnInitializedAsync" or "OnParametersSet" or "OnParametersSetAsync" or "OnAfterRender" or "OnAfterRenderAsync" or "ShouldRender" or "SetParametersAsync" or "Dispose" or "DisposeAsync")
                    continue;

                var signature = method.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
                if (!seenSignatures.Add(signature))
                    continue;

                builder.Add(method);
            }
        }

        return builder.ToImmutable();
    }

    private static IMethodSymbol? FindOrdinaryMethod(INamedTypeSymbol symbol, string methodName, int parameterCount)
        => FindHierarchyMethod(symbol, methodName, method =>
            method.MethodKind == MethodKind.Ordinary &&
            method.Parameters.Length == parameterCount);

    private static IMethodSymbol? FindHierarchyMethod(
        INamedTypeSymbol symbol,
        string methodName,
        Func<IMethodSymbol, bool> predicate)
    {
        for (var current = symbol; current is not null; current = current.BaseType)
        {
            var method = current.GetMembers(methodName)
                .OfType<IMethodSymbol>()
                .FirstOrDefault(candidate =>
                    !candidate.IsStatic &&
                    candidate.Locations.Any(static location => location.IsInSource) &&
                    predicate(candidate));
            if (method is not null)
                return method;
        }

        return null;
    }

    private static bool DerivesFrom(ITypeSymbol? symbol, INamedTypeSymbol baseType)
    {
        for (var current = symbol as INamedTypeSymbol; current is not null; current = current.BaseType)
        {
            if (Comparer.Equals(current.OriginalDefinition, baseType))
                return true;
        }

        return false;
    }
}

