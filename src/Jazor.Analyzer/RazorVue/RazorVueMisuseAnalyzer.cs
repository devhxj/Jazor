using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
            if (IsSupportedShouldRender(method))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                RazorVueDiagnosticDescriptors.ShouldRenderNotSupported,
                location));
            return;
        }

        if (knownSymbols.IsSetParametersAsync(method))
        {
            if (IsSupportedSetParametersAsync(method))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                RazorVueDiagnosticDescriptors.SetParametersAsyncNotSupported,
                location));
        }
    }

    private static bool IsSupportedShouldRender(IMethodSymbol method)
    {
        if (method.DeclaringSyntaxReferences.Length == 0)
            return false;

        if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            return false;

        if (methodSyntax.ExpressionBody is not null)
            return IsConstantTrueShouldRenderExpression(methodSyntax.ExpressionBody.Expression);

        if (methodSyntax.Body?.Statements.Count != 1 ||
            methodSyntax.Body.Statements[0] is not ReturnStatementSyntax { Expression: not null } returnStatement)
        {
            return false;
        }

        // Only a literal `true` override is equivalent to RazorVue's default
        // render behavior. Any conditional gate still carries Blazor-only semantics.
        return IsConstantTrueShouldRenderExpression(returnStatement.Expression);
    }

    private static bool IsSupportedSetParametersAsync(IMethodSymbol method)
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

        var statements = methodSyntax.Body.Statements;
        var index = 0;
        var sawBaseCall = false;
        if (IsBaseSetParametersAsyncStatement(method, statements[0]))
        {
            sawBaseCall = true;
            index++;
        }

        if (index >= statements.Count)
            return true;

        if (TryGetSetParametersAsyncNoOpOrEmit(statements[index], out var hasEmit))
        {
            index++;
            if (index == statements.Count)
                return !hasEmit || sawBaseCall;

            if (index == statements.Count - 1 &&
                IsNoOpSetParametersAsyncStatement(statements[index]))
            {
                return !hasEmit || sawBaseCall;
            }
        }

        return false;
    }

    private static bool IsBaseSetParametersAsyncCall(IMethodSymbol method, ExpressionSyntax expression)
    {
        expression = UnwrapExpression(expression);
        if (expression is AwaitExpressionSyntax awaitExpression)
            expression = UnwrapExpression(awaitExpression.Expression);
        if (TryUnwrapValueTaskCreation(expression, out var wrappedExpression))
            expression = wrappedExpression;

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

    private static bool IsBaseSetParametersAsyncStatement(IMethodSymbol method, StatementSyntax statement)
        => statement switch
        {
            ExpressionStatementSyntax expressionStatement => IsBaseSetParametersAsyncCall(method, expressionStatement.Expression),
            ReturnStatementSyntax returnStatement when returnStatement.Expression is not null =>
                IsBaseSetParametersAsyncCall(method, returnStatement.Expression),
            _ => false
        };

    private static bool IsNoOpTaskExpression(ExpressionSyntax expression)
    {
        expression = UnwrapExpression(expression);
        if (expression is AwaitExpressionSyntax awaitExpression)
            expression = UnwrapExpression(awaitExpression.Expression);
        if (TryUnwrapValueTaskCreation(expression, out var wrappedExpression))
            expression = wrappedExpression;

        var text = expression.ToString().Trim();
        return text == "Task.CompletedTask" ||
               text == "ValueTask.CompletedTask" ||
               text == "default" ||
               text == "default(ValueTask)" ||
               text == "default(System.Threading.Tasks.ValueTask)";
    }

    private static bool IsNoOpSetParametersAsyncStatement(StatementSyntax statement)
        => statement switch
        {
            ReturnStatementSyntax returnStatement when returnStatement.Expression is null => true,
            ReturnStatementSyntax returnStatement when returnStatement.Expression is not null =>
                IsNoOpTaskExpression(returnStatement.Expression),
            ExpressionStatementSyntax expressionStatement => IsNoOpTaskExpression(expressionStatement.Expression),
            _ => false
        };

    private static bool TryGetSetParametersAsyncNoOpOrEmit(StatementSyntax statement, out bool hasEmit)
    {
        hasEmit = false;
        switch (statement)
        {
            case ReturnStatementSyntax returnStatement when returnStatement.Expression is null:
                return true;
            case ReturnStatementSyntax returnStatement when returnStatement.Expression is not null:
                if (IsNoOpTaskExpression(returnStatement.Expression))
                    return true;

                hasEmit = IsSupportedInvokeAsyncExpression(returnStatement.Expression);
                return hasEmit;
            case ExpressionStatementSyntax expressionStatement:
                if (IsNoOpTaskExpression(expressionStatement.Expression))
                    return true;

                hasEmit = IsSupportedInvokeAsyncExpression(expressionStatement.Expression);
                return hasEmit;
            default:
                return false;
        }
    }

    private static bool IsSupportedInvokeAsyncExpression(ExpressionSyntax expression)
    {
        expression = UnwrapExpression(expression);
        if (expression is AwaitExpressionSyntax awaitExpression)
            expression = UnwrapExpression(awaitExpression.Expression);
        if (TryUnwrapValueTaskCreation(expression, out var wrappedExpression))
            expression = wrappedExpression;

        return expression is InvocationExpressionSyntax invocation &&
               invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
               memberAccess.Name.Identifier.ValueText == "InvokeAsync" &&
               invocation.ArgumentList.Arguments.Count <= 1;
    }

    private static bool IsConstantTrueShouldRenderExpression(ExpressionSyntax expression)
    {
        expression = UnwrapExpression(expression);
        return expression.IsKind(SyntaxKind.TrueLiteralExpression);
    }

    private static bool TryUnwrapValueTaskCreation(ExpressionSyntax expression, out ExpressionSyntax innerExpression)
    {
        innerExpression = null!;
        expression = UnwrapExpression(expression);
        if (expression is not ObjectCreationExpressionSyntax creation ||
            creation.ArgumentList?.Arguments.Count != 1)
        {
            return false;
        }

        var typeName = creation.Type.ToString();
        if (typeName != "ValueTask" &&
            typeName != "System.Threading.Tasks.ValueTask")
        {
            return false;
        }

        innerExpression = UnwrapExpression(creation.ArgumentList.Arguments[0].Expression);
        return true;
    }

    private static ExpressionSyntax UnwrapExpression(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;

        return expression;
    }
}
