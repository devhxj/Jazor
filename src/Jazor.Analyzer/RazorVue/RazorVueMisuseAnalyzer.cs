using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace Jazor.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RazorVueMisuseAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        RazorVueDiagnosticDescriptors.StateHasChangedNotSupported,
        RazorVueDiagnosticDescriptors.ShouldRenderNotSupported,
        RazorVueDiagnosticDescriptors.SetParametersAsyncNotSupported
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation invocation)
            return;

        var containingType = context.ContainingSymbol?.ContainingType;
        if (containingType is null)
            return;

        var knownSymbols = RazorVueKnownSymbols.TryCreate(context.Compilation);
        if (knownSymbols is null || !knownSymbols.IsRazorVueComponent(containingType))
            return;

        if (!knownSymbols.IsStateHasChanged(invocation.TargetMethod))
            return;

        // Vue drives update scheduling in RazorVue, so StateHasChanged must not
        // silently survive as a hidden Blazor semantic.
        context.ReportDiagnostic(Diagnostic.Create(
            RazorVueDiagnosticDescriptors.StateHasChangedNotSupported,
            invocation.Syntax.GetLocation()));
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IMethodSymbol method || method.MethodKind is not MethodKind.Ordinary and not MethodKind.ReducedExtension)
            return;

        var containingType = method.ContainingType;
        if (containingType is null)
            return;

        var knownSymbols = RazorVueKnownSymbols.TryCreate(context.Compilation);
        if (knownSymbols is null || !knownSymbols.IsRazorVueComponent(containingType))
            return;

        var location = method.Locations.FirstOrDefault(static x => x.IsInSource) ?? Location.None;
        if (knownSymbols.IsShouldRender(method))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                RazorVueDiagnosticDescriptors.ShouldRenderNotSupported,
                location));
            return;
        }

        if (knownSymbols.IsSetParametersAsync(method))
        {
            if (IsSupportedNoOpSetParametersAsync(method))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                RazorVueDiagnosticDescriptors.SetParametersAsyncNotSupported,
                location));
        }
    }

    private static bool IsSupportedNoOpSetParametersAsync(IMethodSymbol method)
    {
        if (method.DeclaringSyntaxReferences.Length == 0)
            return false;

        if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            return false;

        if (methodSyntax.ExpressionBody is not null)
            return IsBaseSetParametersAsyncCall(method, methodSyntax.ExpressionBody.Expression);

        if (methodSyntax.Body is null)
            return false;

        if (methodSyntax.Body.Statements.Count == 0)
            return true;

        if (methodSyntax.Body.Statements.Count == 1)
        {
            return methodSyntax.Body.Statements[0] switch
            {
                ExpressionStatementSyntax expressionStatement => IsBaseSetParametersAsyncCall(method, expressionStatement.Expression),
                ReturnStatementSyntax returnStatement when returnStatement.Expression is null => true,
                ReturnStatementSyntax returnStatement when returnStatement.Expression is not null =>
                    IsNoOpTaskExpression(returnStatement.Expression) ||
                    IsBaseSetParametersAsyncCall(method, returnStatement.Expression),
                _ => false
            };
        }

        if (methodSyntax.Body.Statements.Count == 2 &&
            methodSyntax.Body.Statements[0] is ExpressionStatementSyntax leadingExpression &&
            methodSyntax.Body.Statements[1] is ReturnStatementSyntax trailingReturn)
        {
            return IsBaseSetParametersAsyncCall(method, leadingExpression.Expression) &&
                   (trailingReturn.Expression is null || IsNoOpTaskExpression(trailingReturn.Expression));
        }

        return false;
    }

    private static bool IsBaseSetParametersAsyncCall(IMethodSymbol method, ExpressionSyntax expression)
    {
        expression = UnwrapExpression(expression);
        if (expression is AwaitExpressionSyntax awaitExpression)
            expression = UnwrapExpression(awaitExpression.Expression);

        if (expression is not InvocationExpressionSyntax invocation ||
            invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
            memberAccess.Expression is not BaseExpressionSyntax ||
            memberAccess.Name.Identifier.ValueText != "SetParametersAsync" ||
            invocation.ArgumentList.Arguments.Count != 1)
        {
            return false;
        }

        var argument = UnwrapExpression(invocation.ArgumentList.Arguments[0].Expression);
        return argument is IdentifierNameSyntax identifier &&
               identifier.Identifier.ValueText == method.Parameters[0].Name;
    }

    private static bool IsNoOpTaskExpression(ExpressionSyntax expression)
    {
        expression = UnwrapExpression(expression);
        if (expression is AwaitExpressionSyntax awaitExpression)
            expression = UnwrapExpression(awaitExpression.Expression);

        var text = expression.ToString().Trim();
        return text == "Task.CompletedTask" ||
               text == "ValueTask.CompletedTask" ||
               text == "default" ||
               text == "default(ValueTask)" ||
               text == "default(System.Threading.Tasks.ValueTask)";
    }

    private static ExpressionSyntax UnwrapExpression(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;

        return expression;
    }
}
