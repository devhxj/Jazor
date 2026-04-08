using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jazor.Vue.Analysis;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class JazorVueImportUsageAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        JazorVueDiagnosticDescriptors.NamespaceImportInvokedAsFunction,
        JazorVueDiagnosticDescriptors.ComponentImportInvokedAsFunction
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
            return;

        if (UnwrapExpression(invocation.Expression) is not IdentifierNameSyntax identifierName)
            return;

        var aliasSymbol = context.SemanticModel.GetSymbolInfo(identifierName, context.CancellationToken).Symbol as IPropertySymbol;
        if (aliasSymbol is null)
            return;

        var containingType = aliasSymbol.ContainingType;
        if (containingType is null || !IsGeneratedAnalysisType(containingType))
            return;

        var symbolProperty = containingType.GetMembers(aliasSymbol.Name + "Symbol")
            .OfType<IPropertySymbol>()
            .FirstOrDefault(static member => member.IsStatic);
        if (symbolProperty is null)
            return;

        var descriptor = symbolProperty.Type.Name switch
        {
            "__JsNamespaceSymbol" => JazorVueDiagnosticDescriptors.NamespaceImportInvokedAsFunction,
            "__VueComponentSymbol" => JazorVueDiagnosticDescriptors.ComponentImportInvokedAsFunction,
            _ => null
        };
        if (descriptor is null)
            return;

        context.ReportDiagnostic(Diagnostic.Create(descriptor, identifierName.GetLocation(), aliasSymbol.Name));
    }

    private static bool IsGeneratedAnalysisType(INamedTypeSymbol type)
        => type.Name.StartsWith("__JazorAnalysis_", StringComparison.Ordinal) &&
           string.Equals(type.ContainingNamespace.ToDisplayString(), "Jazor.Vue.Generated.Analysis", StringComparison.Ordinal);

    private static ExpressionSyntax UnwrapExpression(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;

        return expression;
    }
}
