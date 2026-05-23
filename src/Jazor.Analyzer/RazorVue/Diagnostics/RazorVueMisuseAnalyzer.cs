using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Jazor.RazorVue;
using Jazor.RazorVue.Lowering;

namespace Jazor.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RazorVueMisuseAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        RazorVueDiagnosticDescriptors.StateHasChangedNotSupported,
        RazorVueDiagnosticDescriptors.RenderControlOrLifecycleNotSupported,
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
            if (IsSupportedShouldRender(knownSymbols, method))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                RazorVueDiagnosticDescriptors.RenderControlOrLifecycleNotSupported,
                location));
            return;
        }

        if (knownSymbols.IsSupportedLifecycleCandidate(method))
        {
            if (IsSupportedLifecycleMethod(context.Compilation, method))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                RazorVueDiagnosticDescriptors.RenderControlOrLifecycleNotSupported,
                location));
            return;
        }

        if (knownSymbols.IsSetParametersAsync(method))
        {
            if (IsSupportedSetParametersAsync(context.Compilation, method))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                RazorVueDiagnosticDescriptors.SetParametersAsyncNotSupported,
                location));
        }
    }

    private static bool IsSupportedShouldRender(RazorVueKnownSymbols knownSymbols, IMethodSymbol method)
        => IsSupportedShouldRender(knownSymbols, method, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));

    private static bool IsSupportedShouldRender(
        RazorVueKnownSymbols knownSymbols,
        IMethodSymbol method,
        HashSet<IMethodSymbol> visitedMethods)
    {
        if (!visitedMethods.Add(method) || method.DeclaringSyntaxReferences.Length == 0)
            return false;

        if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            return false;

        if (methodSyntax.ExpressionBody is not null)
            return IsSupportedShouldRenderExpression(knownSymbols, method, methodSyntax.ExpressionBody.Expression, visitedMethods);

        if (methodSyntax.Body?.Statements.Count != 1 ||
            methodSyntax.Body.Statements[0] is not ReturnStatementSyntax { Expression: not null } returnStatement)
        {
            return false;
        }

        return IsSupportedShouldRenderExpression(knownSymbols, method, returnStatement.Expression, visitedMethods);
    }

    private static bool IsSupportedSetParametersAsync(Compilation compilation, IMethodSymbol method)
    {
        var razorVueContext = RazorVueCompilationContext.TryCreate(compilation);
        if (razorVueContext is null)
            return false;

        var candidate = razorVueContext.DiscoverComponentCandidates()
            .FirstOrDefault(candidate =>
                SymbolEqualityComparer.Default.Equals(candidate.ComponentSymbol, method.ContainingType));
        if (candidate is null)
            return false;

        var snapshot = razorVueContext.CreateSemanticSnapshot(candidate);
        return RazorVueSetupAndLifecycleLoweringSupport.DescribeSetParametersAsyncShape(snapshot, method) != "unsupported";
    }

    private static bool IsSupportedLifecycleMethod(Compilation compilation, IMethodSymbol method)
    {
        var razorVueContext = RazorVueCompilationContext.TryCreate(compilation);
        if (razorVueContext is null)
            return false;

        var candidate = razorVueContext.DiscoverComponentCandidates()
            .FirstOrDefault(candidate =>
                SymbolEqualityComparer.Default.Equals(candidate.ComponentSymbol, method.ContainingType));
        if (candidate is null)
            return false;

        var snapshot = razorVueContext.CreateSemanticSnapshot(candidate);
        var supportShape = RazorVueSetupAndLifecycleLoweringSupport.DescribeLifecycleSupportShape(
            snapshot,
            method,
            allowFirstRenderPayload: method.Name is "OnAfterRender" or "OnAfterRenderAsync");

        return supportShape != "unsupported";
    }

    private static bool IsSupportedLifecycleMethod(IMethodSymbol method)
        => IsSupportedLifecycleMethod(method, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));

    private static bool IsSupportedLifecycleMethod(
        IMethodSymbol method,
        HashSet<IMethodSymbol> visitedMethods)
    {
        if (!visitedMethods.Add(method))
            return false;

        if (method.DeclaringSyntaxReferences.Length == 0)
            return true;

        if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            return false;

        if (methodSyntax.ExpressionBody is not null)
        {
            if (TryAnalyzeBaseLifecyclePassThrough(method, methodSyntax.ExpressionBody.Expression, visitedMethods, out var isSupportedBasePassThrough))
                return isSupportedBasePassThrough;

            return IsSupportedLifecycleExpression(method, methodSyntax.ExpressionBody.Expression);
        }

        if (methodSyntax.Body is null)
            return false;

        if (methodSyntax.Body.Statements.Count == 0)
            return true;

        if (methodSyntax.Body.Statements.Count == 1 &&
            TryAnalyzeBaseLifecyclePassThrough(method, methodSyntax.Body.Statements[0], visitedMethods, out var isSupportedSingleStatementPassThrough))
        {
            return isSupportedSingleStatementPassThrough;
        }

        if (methodSyntax.Body.Statements.Count == 2 &&
            TryAnalyzeBaseLifecyclePassThrough(method, methodSyntax.Body.Statements[0], visitedMethods, out var isSupportedTrailingNoOpPassThrough) &&
            methodSyntax.Body.Statements[1] is ReturnStatementSyntax trailingNoOpReturn &&
            (trailingNoOpReturn.Expression is null || IsNoOpLifecycleExpression(method, trailingNoOpReturn.Expression)))
        {
            return isSupportedTrailingNoOpPassThrough;
        }

        if (methodSyntax.Body.Statements.Count == 2 &&
            methodSyntax.Body.Statements[0] is ExpressionStatementSyntax leadingExpression &&
            methodSyntax.Body.Statements[1] is ReturnStatementSyntax trailingReturn &&
            (trailingReturn.Expression is null || IsNoOpLifecycleExpression(method, trailingReturn.Expression)))
        {
            return IsSupportedLifecycleEmitExpression(leadingExpression.Expression);
        }

        if (methodSyntax.Body.Statements.Count >= 2 &&
            methodSyntax.Body.Statements.Take(methodSyntax.Body.Statements.Count - 1).All(static statement => statement is LocalDeclarationStatementSyntax))
        {
            return IsSupportedLifecycleExpression(
                method,
                methodSyntax.Body.Statements[methodSyntax.Body.Statements.Count - 1] switch
                {
                    ExpressionStatementSyntax expressionStatement => expressionStatement.Expression,
                    ReturnStatementSyntax { Expression: not null } returnStatement => returnStatement.Expression,
                    _ => null!
                });
        }

        if (methodSyntax.Body.Statements.Count != 1)
            return false;

        return methodSyntax.Body.Statements[0] switch
        {
            ExpressionStatementSyntax expressionStatement => IsSupportedLifecycleEmitExpression(expressionStatement.Expression),
            ReturnStatementSyntax returnStatement when returnStatement.Expression is null || IsNoOpLifecycleExpression(method, returnStatement.Expression) => true,
            ReturnStatementSyntax returnStatement when returnStatement.Expression is not null => IsSupportedLifecycleExpression(method, returnStatement.Expression),
            _ => false
        };
    }

    private static bool IsNoOpTaskExpression(IMethodSymbol method, ExpressionSyntax expression)
    {
        return IsNoOpAwaitableExpression(
            expression,
            allowBareDefaultLiteral: IsNonGenericValueTaskType(method.ReturnType));
    }

    private static bool IsNoOpLifecycleExpression(IMethodSymbol method, ExpressionSyntax expression)
    {
        return IsNoOpAwaitableExpression(
            expression,
            allowBareDefaultLiteral: IsNonGenericValueTaskType(method.ReturnType));
    }

    private static bool IsSupportedInvokeAsyncExpression(ExpressionSyntax expression)
    {
        expression = UnwrapExpression(expression);
        if (expression is AwaitExpressionSyntax awaitExpression)
            expression = UnwrapExpression(awaitExpression.Expression);
        if (TryUnwrapValueTaskCreation(null, expression, out var wrappedExpression))
            expression = wrappedExpression;

        return expression is InvocationExpressionSyntax invocation &&
               invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
               memberAccess.Name.Identifier.ValueText == "InvokeAsync" &&
               invocation.ArgumentList.Arguments.Count <= 1;
    }

    private static bool IsSupportedLifecycleExpression(IMethodSymbol method, ExpressionSyntax expression)
    {
        if (IsNoOpLifecycleExpression(method, expression))
            return true;

        if (TryAnalyzeBaseLifecyclePassThrough(method, expression, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default), out var isSupportedBasePassThrough))
            return isSupportedBasePassThrough;

        return IsSupportedLifecycleEmitExpression(expression);
    }

    private static bool IsSupportedLifecycleEmitExpression(ExpressionSyntax expression)
        => IsSupportedInvokeAsyncExpression(expression);

    private static bool IsConstantTrueShouldRenderExpression(ExpressionSyntax expression)
    {
        expression = UnwrapExpression(expression);
        return expression.IsKind(SyntaxKind.TrueLiteralExpression);
    }

    private static bool IsSupportedShouldRenderExpression(
        RazorVueKnownSymbols knownSymbols,
        IMethodSymbol method,
        ExpressionSyntax expression,
        HashSet<IMethodSymbol> visitedMethods)
    {
        expression = UnwrapExpression(expression);
        if (IsConstantTrueShouldRenderExpression(expression))
            return true;

        if (expression is not InvocationExpressionSyntax invocationExpression)
            return false;

        if (invocationExpression.Expression is not MemberAccessExpressionSyntax
            {
                Expression: BaseExpressionSyntax,
                Name.Identifier.ValueText: "ShouldRender"
            } ||
            invocationExpression.ArgumentList.Arguments.Count != 0)
        {
            return false;
        }

        for (var current = method.ContainingType.BaseType; current is not null; current = current.BaseType)
        {
            var candidate = current.GetMembers("ShouldRender")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(static member =>
                    !member.IsStatic &&
                    member.Parameters.Length == 0 &&
                    member.ReturnType.SpecialType == SpecialType.System_Boolean);
            if (candidate is null)
                continue;

            if (SymbolEqualityComparer.Default.Equals(candidate.ContainingType.OriginalDefinition, knownSymbols.Symbols.ComponentBase))
                return true;

            return IsSupportedShouldRender(knownSymbols, candidate, visitedMethods);
        }

        return false;
    }

    private static bool TryAnalyzeBaseLifecyclePassThrough(
        IMethodSymbol method,
        StatementSyntax statement,
        HashSet<IMethodSymbol> visitedMethods,
        out bool isSupported)
        => statement switch
        {
            ExpressionStatementSyntax expressionStatement =>
                TryAnalyzeBaseLifecyclePassThrough(method, expressionStatement.Expression, visitedMethods, out isSupported),
            ReturnStatementSyntax returnStatement when returnStatement.Expression is not null =>
                TryAnalyzeBaseLifecyclePassThrough(method, returnStatement.Expression, visitedMethods, out isSupported),
            _ =>
                ReturnNoBaseLifecycleAnalysis(out isSupported)
        };

    private static bool TryAnalyzeBaseLifecyclePassThrough(
        IMethodSymbol method,
        ExpressionSyntax expression,
        HashSet<IMethodSymbol> visitedMethods,
        out bool isSupported)
    {
        isSupported = false;
        if (!IsBaseLifecyclePassThroughCall(method, expression))
            return false;

        var baseMethod = FindBaseLifecycleMethod(method);
        if (baseMethod is null)
            return false;

        if (baseMethod.DeclaringSyntaxReferences.Length == 0)
        {
            isSupported = IsDefaultComponentBaseLifecycleMethod(baseMethod);
            return true;
        }

        isSupported = IsSupportedLifecycleMethod(baseMethod, visitedMethods);
        return true;
    }

    private static bool TryUnwrapValueTaskCreation(Compilation? compilation, ExpressionSyntax expression, out ExpressionSyntax innerExpression)
    {
        innerExpression = expression;
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

    private static bool IsNoOpAwaitableExpression(ExpressionSyntax expression, bool allowBareDefaultLiteral)
    {
        expression = UnwrapExpression(expression);
        if (expression is AwaitExpressionSyntax awaitExpression)
            return IsNoOpAwaitableExpression(awaitExpression.Expression, allowBareDefaultLiteral);

        if (TryUnwrapValueTaskCreation(null, expression, out var wrappedExpression))
            return IsNoOpAwaitableExpression(wrappedExpression, allowBareDefaultLiteral: true);

        if (expression.IsKind(SyntaxKind.DefaultLiteralExpression))
            return allowBareDefaultLiteral;

        if (expression is DefaultExpressionSyntax defaultExpression)
            return IsNonGenericValueTaskType(defaultExpression.Type.ToString());

        return IsKnownCompletedTaskExpression(expression);
    }

    private static bool IsKnownCompletedTaskExpression(ExpressionSyntax expression)
    {
        var text = expression.ToString().Trim();
        return text == "Task.CompletedTask" ||
               text == "System.Threading.Tasks.Task.CompletedTask" ||
               text == "ValueTask.CompletedTask" ||
               text == "System.Threading.Tasks.ValueTask.CompletedTask";
    }

    private static bool IsNonGenericTaskType(Compilation compilation, ITypeSymbol? type)
    {
        var taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        return taskType is not null &&
               type is not null &&
               SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, taskType);
    }

    private static bool IsNonGenericValueTaskType(ITypeSymbol? type)
        => type is INamedTypeSymbol
        {
            Name: "ValueTask",
            Arity: 0,
            ContainingNamespace: { }
        } namedType &&
           namedType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks";

    private static bool IsNonGenericValueTaskType(string typeName)
        => typeName == "ValueTask" ||
           typeName == "System.Threading.Tasks.ValueTask";

    private static bool IsBaseLifecyclePassThroughCall(IMethodSymbol method, ExpressionSyntax expression)
    {
        expression = UnwrapExpression(expression);
        if (expression is AwaitExpressionSyntax awaitExpression)
            expression = UnwrapExpression(awaitExpression.Expression);
        if (TryUnwrapValueTaskCreation(null, expression, out var wrappedExpression))
            expression = wrappedExpression;

        if (expression is not InvocationExpressionSyntax invocation ||
            invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
            memberAccess.Expression is not BaseExpressionSyntax ||
            !string.Equals(memberAccess.Name.Identifier.ValueText, method.Name, System.StringComparison.Ordinal) ||
            invocation.ArgumentList.Arguments.Count != method.Parameters.Length)
        {
            return false;
        }

        for (var index = 0; index < method.Parameters.Length; index++)
        {
            var argument = UnwrapExpression(invocation.ArgumentList.Arguments[index].Expression);
            if (argument is not IdentifierNameSyntax identifier ||
                !string.Equals(identifier.Identifier.ValueText, method.Parameters[index].Name, System.StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static IMethodSymbol? FindBaseLifecycleMethod(IMethodSymbol method)
    {
        for (var current = method.ContainingType.BaseType; current is not null; current = current.BaseType)
        {
            var candidate = current.GetMembers(method.Name)
                .OfType<IMethodSymbol>()
                .FirstOrDefault(member =>
                    !member.IsStatic &&
                    member.Parameters.Length == method.Parameters.Length &&
                    ParametersMatch(member, method));
            if (candidate is not null)
                return candidate;
        }

        return null;
    }

    private static bool ParametersMatch(IMethodSymbol candidate, IMethodSymbol method)
    {
        for (var index = 0; index < method.Parameters.Length; index++)
        {
            if (!SymbolEqualityComparer.Default.Equals(
                    candidate.Parameters[index].Type.OriginalDefinition,
                    method.Parameters[index].Type.OriginalDefinition))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsDefaultComponentBaseLifecycleMethod(IMethodSymbol method)
        => method.Name is "OnInitialized" or "OnInitializedAsync" or "OnParametersSet" or "OnParametersSetAsync" or "OnAfterRender" or "OnAfterRenderAsync" &&
           method.ContainingType is
           {
               Name: "ComponentBase",
               ContainingNamespace: { } ns
           } &&
           ns.ToDisplayString() == "Microsoft.AspNetCore.Components";

    private static bool ReturnNoBaseLifecycleAnalysis(out bool isSupported)
    {
        isSupported = false;
        return false;
    }

    private static ExpressionSyntax UnwrapExpression(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;

        return expression;
    }

}
