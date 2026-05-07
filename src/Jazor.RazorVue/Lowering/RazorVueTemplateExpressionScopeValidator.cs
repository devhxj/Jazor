using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Lowering;

internal static class RazorVueTemplateExpressionScopeValidator
{
    private static readonly ImmutableHashSet<ILocalSymbol> EmptyLocalScope =
        ImmutableHashSet<ILocalSymbol>.Empty.WithComparer(SymbolEqualityComparer.Default);
    private static readonly ImmutableHashSet<IParameterSymbol> EmptyParameterScope =
        ImmutableHashSet<IParameterSymbol>.Empty.WithComparer(SymbolEqualityComparer.Default);

    public static void Validate(
        RazorVueSemanticSnapshot snapshot,
        IOperation expression,
        ImmutableHashSet<ILocalSymbol>? allowedLocalSymbols = null,
        ImmutableHashSet<IParameterSymbol>? allowedParameterSymbols = null)
    {
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));
        if (expression is null)
            throw new ArgumentNullException(nameof(expression));

        var localScope = allowedLocalSymbols ?? EmptyLocalScope;
        var parameterScope = allowedParameterSymbols ?? EmptyParameterScope;
        foreach (var operation in EnumerateSelfAndDescendants(expression))
        {
            switch (RazorVueOperationNormalizer.Unwrap(operation))
            {
                case ILocalReferenceOperation localReference
                    when !localScope.Contains(localReference.Local):
                    throw CreateUnsupportedExpressionException(
                        snapshot,
                        localReference,
                        localReference.Local.Name);
                case IParameterReferenceOperation parameterReference
                    when !parameterScope.Contains(parameterReference.Parameter) &&
                         !IsExpressionLocalLambdaParameter(parameterReference):
                    throw CreateUnsupportedExpressionException(
                        snapshot,
                        parameterReference,
                        parameterReference.Parameter.Name);
            }
        }
    }

    public static ImmutableHashSet<TSymbol> AddIfPresent<TSymbol>(
        ImmutableHashSet<TSymbol>? set,
        TSymbol? symbol)
        where TSymbol : class, ISymbol
    {
        var effectiveSet = set ?? ImmutableHashSet<TSymbol>.Empty.WithComparer(SymbolEqualityComparer.Default);
        return symbol is null ? effectiveSet : effectiveSet.Add(symbol);
    }

    private static IEnumerable<IOperation> EnumerateSelfAndDescendants(IOperation root)
    {
        yield return root;
        foreach (var descendant in root.Descendants())
            yield return descendant;
    }

    private static bool IsExpressionLocalLambdaParameter(IParameterReferenceOperation parameterReference)
        => parameterReference.Parameter.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.LambdaMethod or MethodKind.AnonymousFunction };

    private static RazorVueCompilationIssueException CreateUnsupportedExpressionException(
        RazorVueSemanticSnapshot snapshot,
        IOperation expression,
        string symbolName)
    {
        var origin = expression.Syntax is null
            ? snapshot.Origins.FirstOrDefault()
            : RazorVueSourceOrigin.FromLocation(expression.Syntax.GetLocation(), RazorVueOriginKind.Template);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedTemplateEncoding,
            RazorVueIssueSeverity.Error,
            $"RazorVue template expression cannot hoist component-local expression '{symbolName}' in component '{snapshot.Descriptor.FullName}'. Promote it to a supported field/property/helper result or keep the expression within template scope.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, snapshot.Descriptor.FullName, origin);
    }
}
