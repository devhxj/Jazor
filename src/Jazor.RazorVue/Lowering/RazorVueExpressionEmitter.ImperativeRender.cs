using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Lowering;

internal sealed partial class RazorVueExpressionEmitter
{
    private string? _imperativeBuilderAlias;
    private Dictionary<IParameterSymbol, string>? _imperativeBuilderParameterTargets;
    private Dictionary<ILocalSymbol, IOperation>? _imperativeRenderFragmentLocalInitializers;
    private Dictionary<ILocalSymbol, IOperation>? _imperativeStaticMarkupLocalInitializers;
    private HashSet<IMethodSymbol>? _imperativeMaterializedLocalRenderFragmentFactories;

    private string EmitImperativeBlockBody(
        RazorVueImperativeBlockNode imperative,
        string builderAlias)
    {
        foreach (var operation in imperative.Operations)
            EnsureSupportedImperativeOperation(operation);

        var bodyArgument = new SenseArgument(Sense.FunctionBody, UseImportAliases: true);
        var visibleLocalAliases = new Dictionary<ILocalSymbol, string>(SymbolEqualityComparer.Default);
        var imperativeBuilderTargets = new Dictionary<IParameterSymbol, string>(SymbolEqualityComparer.Default);
        var locallyDeclaredLocals = RazorVueOperationLocalCollector.CollectDeclaredLocals(imperative.Operations);
        foreach (var local in imperative.VisibleLocals)
        {
            if (locallyDeclaredLocals.Contains(local))
                continue;

            visibleLocalAliases[local] = local.Name;
        }

        foreach (var parameter in imperative.VisibleParameters)
        {
            if (IsRenderTreeBuilderType(parameter.Type))
                imperativeBuilderTargets[parameter] = builderAlias;
        }

        var imperativeRenderFragmentLocalInitializers =
            RazorVueImperativeRenderFragmentCarrierHelper.CollectSourceStableLocalRenderFragmentInitializers(
                _snapshot.Compilation,
                imperative.Operations);
        var imperativeStaticMarkupLocalInitializers =
            RazorVueSourceStableLocalInitializerHelper.CollectSourceStableLocalInitializers(
                _snapshot.Compilation,
                imperative.Operations,
                RazorVueStaticMarkupValueHelper.IsStaticMarkupCarrierType);
        var materializedLocalRenderFragmentFactories =
            CollectMaterializedImperativeLocalRenderFragmentFactories(imperative.Operations);

        return WithImperativeBuilderAlias(
            builderAlias,
            () => WithImperativeBuilderParameterTargets(
                imperativeBuilderTargets,
                () => WithImperativeRenderFragmentLocalInitializers(
                    imperativeRenderFragmentLocalInitializers,
                    () => WithImperativeMaterializedLocalRenderFragmentFactories(
                        materializedLocalRenderFragmentFactories,
                        () => WithImperativeStaticMarkupLocalInitializers(
                            imperativeStaticMarkupLocalInitializers,
                            () => WithScopedLocalAliases(
                                visibleLocalAliases,
                                () => WithScopedParameterAliases(
                                    imperative.VisibleParameters,
                                    imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(),
                                    () =>
                                    {
                                        try
                                        {
                                            var statements = _semanticWalker.TranslateStatementSequence(imperative.Operations, bodyArgument);
                                            var functionBody = NormalizeImperativeFunctionBody(
                                                new FunctionBody(NodeList.From(statements), strict: true),
                                                builderAlias,
                                                appendTerminalReturn: false);
                                            return NormalizeImperativeFunctionText(functionBody.ToKnRECMAScript());
                                        }
                                        catch (RazorVueCompilationIssueException)
                                        {
                                            throw;
                                        }
                                        catch (Exception ex) when (ex is NotSupportedException or OperationTransformationException)
                                        {
                                            throw CreateUnsupportedImperativeRenderCompilerBoundaryException(
                                                imperative.Operations,
                                                ex);
                                        }
                                    })))))));
    }

    private T WithImperativeBuilderAlias<T>(string builderAlias, Func<T> action)
    {
        var previous = _imperativeBuilderAlias;
        _imperativeBuilderAlias = builderAlias;
        try
        {
            return action();
        }
        finally
        {
            _imperativeBuilderAlias = previous;
        }
    }

    private T WithImperativeRenderFragmentLocalInitializers<T>(
        IReadOnlyDictionary<ILocalSymbol, IOperation> initializers,
        Func<T> action)
    {
        var previous = _imperativeRenderFragmentLocalInitializers;
        var current = previous is null
            ? new Dictionary<ILocalSymbol, IOperation>(SymbolEqualityComparer.Default)
            : new Dictionary<ILocalSymbol, IOperation>(previous, SymbolEqualityComparer.Default);

        foreach (var pair in initializers)
            current[pair.Key] = pair.Value;

        _imperativeRenderFragmentLocalInitializers = current;
        try
        {
            return action();
        }
        finally
        {
            _imperativeRenderFragmentLocalInitializers = previous;
        }
    }

    private T WithImperativeStaticMarkupLocalInitializers<T>(
        IReadOnlyDictionary<ILocalSymbol, IOperation> initializers,
        Func<T> action)
    {
        var previous = _imperativeStaticMarkupLocalInitializers;
        var current = previous is null
            ? new Dictionary<ILocalSymbol, IOperation>(SymbolEqualityComparer.Default)
            : new Dictionary<ILocalSymbol, IOperation>(previous, SymbolEqualityComparer.Default);

        foreach (var pair in initializers)
            current[pair.Key] = pair.Value;

        _imperativeStaticMarkupLocalInitializers = current;
        try
        {
            return action();
        }
        finally
        {
            _imperativeStaticMarkupLocalInitializers = previous;
        }
    }

    private T WithImperativeMaterializedLocalRenderFragmentFactories<T>(
        IEnumerable<IMethodSymbol> localFunctions,
        Func<T> action)
    {
        var previous = _imperativeMaterializedLocalRenderFragmentFactories;
        var current = previous is null
            ? new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default)
            : new HashSet<IMethodSymbol>(previous, SymbolEqualityComparer.Default);

        foreach (var localFunction in localFunctions)
            current.Add(localFunction);

        _imperativeMaterializedLocalRenderFragmentFactories = current;
        try
        {
            return action();
        }
        finally
        {
            _imperativeMaterializedLocalRenderFragmentFactories = previous;
        }
    }

    private void EnsureSupportedImperativeOperation(IOperation operation)
    {
        if (TryGetUnsupportedImperativeGotoOperation(operation, out var unsupportedGotoOperation))
            throw CreateUnsupportedImperativeGotoLoweringException(unsupportedGotoOperation);

        if (TryGetUnsupportedImperativeAsyncOperation(operation, out var unsupportedOperation))
            throw CreateUnsupportedImperativeRenderLoweringException(unsupportedOperation);
    }

    private static bool TryGetUnsupportedImperativeGotoOperation(
        IOperation? operation,
        out IOperation unsupportedOperation)
    {
        unsupportedOperation = null!;
        if (operation is null)
            return false;

        foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(
                     operation,
                     includeLocalFunctionBodies: true))
        {
            if (current is IBranchOperation { BranchKind: BranchKind.GoTo })
            {
                unsupportedOperation = current;
                return true;
            }
        }

        return false;
    }

    private RazorVueCompilationIssueException CreateUnsupportedImperativeGotoLoweringException(IOperation operation)
        => CreateUnsupportedImperativeRenderLoweringException(
            operation,
            $"RazorVue imperative render lowering does not support 'goto' in component '{_snapshot.Descriptor.FullName}' because Jazor.Compiler does not provide an equivalent JavaScript lowering for arbitrary jump control flow.");

    private static bool TryGetUnsupportedImperativeAsyncOperation(
        IOperation? operation,
        out IOperation unsupportedOperation)
    {
        unsupportedOperation = null!;
        if (operation is null)
            return false;

        foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
        {
            if (current is IAnonymousFunctionOperation or ILocalFunctionOperation)
                continue;

            if (current is IUsingDeclarationOperation { IsAsynchronous: true })
            {
                unsupportedOperation = current;
                return true;
            }

            if (current is IUsingOperation { IsAsynchronous: true })
            {
                unsupportedOperation = current;
                return true;
            }

            if (current is IAwaitOperation)
            {
                unsupportedOperation = current;
                return true;
            }

            if (current is IForEachLoopOperation { IsAsynchronous: true })
            {
                unsupportedOperation = current;
                return true;
            }
        }

        return false;
    }

    private RazorVueCompilationIssueException CreateUnsupportedImperativeRenderLoweringException(IOperation operation)
    {
        var construct = DescribeUnsupportedImperativeOperation(operation);
        return CreateUnsupportedImperativeRenderLoweringException(
            operation,
            $"RazorVue imperative render lowering does not support '{construct}' in component '{_snapshot.Descriptor.FullName}' because the current `.mjs`/render-function `.vue` artifact contract is synchronous and cannot carry async render semantics.");
    }

    private RazorVueCompilationIssueException CreateUnsupportedImperativeRenderLoweringException(
        IOperation operation,
        string detail)
    {
        var origin = operation.Syntax is null
            ? _snapshot.Origins.FirstOrDefault()
            : RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), RazorVueOriginKind.Template);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedImperativeRenderLowering,
            RazorVueIssueSeverity.Error,
            detail,
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, _snapshot.Descriptor.FullName, origin);
    }

    private RazorVueCompilationIssueException CreateUnsupportedImperativeRenderCompilerBoundaryException(
        IReadOnlyList<IOperation> operations,
        Exception exception)
    {
        var originOperation = operations.FirstOrDefault(static operation => operation.Syntax is not null) ??
                              operations.FirstOrDefault();
        var detail = $"RazorVue imperative render lowering could not translate component '{_snapshot.Descriptor.FullName}' through Jazor.Compiler. The render-function `.vue` artifact contract only supports compiler-lowerable synchronous render statements.";
        if (!string.IsNullOrWhiteSpace(exception.Message))
            detail += " " + exception.Message;

        if (originOperation is not null)
            return CreateUnsupportedImperativeRenderLoweringException(originOperation, detail);

        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedImperativeRenderLowering,
            RazorVueIssueSeverity.Error,
            detail,
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(
            issue,
            _snapshot.Descriptor.FullName,
            _snapshot.Origins.FirstOrDefault());
    }

    private static string DescribeUnsupportedImperativeOperation(IOperation operation)
        => operation switch
        {
            IUsingOperation { IsAsynchronous: true } => "await using",
            IUsingDeclarationOperation { IsAsynchronous: true } => "await using",
            IAwaitOperation => "await",
            IForEachLoopOperation { IsAsynchronous: true } => "await foreach",
            IBranchOperation { BranchKind: BranchKind.GoTo } => "goto",
            _ => operation.Kind.ToString()
        };

    private T WithImperativeBuilderParameterTargets<T>(
        IReadOnlyDictionary<IParameterSymbol, string> targets,
        Func<T> action)
    {
        var previous = _imperativeBuilderParameterTargets;
        var current = previous is null
            ? new Dictionary<IParameterSymbol, string>(SymbolEqualityComparer.Default)
            : new Dictionary<IParameterSymbol, string>(previous, SymbolEqualityComparer.Default);

        foreach (var pair in targets)
            current[pair.Key] = pair.Value;

        _imperativeBuilderParameterTargets = current;
        try
        {
            return action();
        }
        finally
        {
            _imperativeBuilderParameterTargets = previous;
        }
    }

    private static FunctionBody NormalizeImperativeFunctionBody(
        FunctionBody functionBody,
        string builderAlias,
        bool appendTerminalReturn)
    {
        var rewriter = new ImperativeTopLevelReturnRewriter(builderAlias);
        var rewritten = (FunctionBody)(rewriter.Visit(functionBody) ?? functionBody);
        if (appendTerminalReturn && !rewritten.Body.Any(static statement => statement is ReturnStatement))
        {
            var statements = rewritten.Body.ToList();
            statements.Add(new ReturnStatement(CreateImperativeFinishCall(builderAlias)));
            rewritten = new FunctionBody(NodeList.From(statements), rewritten.Strict);
        }

        return rewritten;
    }

    private static string NormalizeImperativeFunctionText(string functionBodyText)
    {
        const string strictPrefix = "{\n\t\"use strict\";\n";
        if (functionBodyText.StartsWith(strictPrefix, System.StringComparison.Ordinal))
            functionBodyText = "{\n" + functionBodyText.Substring(strictPrefix.Length);
        else if (functionBodyText.StartsWith("{\n\"use strict\";\n", System.StringComparison.Ordinal))
            functionBodyText = "{\n" + functionBodyText.Substring("{\n\"use strict\";\n".Length);

        var normalized = Util.NormalizeLineEndingsToLf(functionBodyText).Trim();
        if (!normalized.StartsWith("{", System.StringComparison.Ordinal) ||
            !normalized.EndsWith("}", System.StringComparison.Ordinal))
        {
            return normalized;
        }

        var innerBody = normalized.Substring(1, normalized.Length - 2).Trim();
        if (innerBody.Length == 0)
            return string.Empty;

        return innerBody;
    }

    internal bool TryRewriteInstanceReference(IInstanceReferenceOperation operation, SenseArgument argument, out string expression)
    {
        _ = argument;
        expression = string.Empty;
        if (_imperativeBuilderAlias is null)
            return false;

        if (operation.ReferenceKind == InstanceReferenceKind.ContainingTypeInstance)
        {
            expression = "__jazorComponent";
            return true;
        }

        return false;
    }

    internal bool TryRewriteStaticMarkupStringConversion(IConversionOperation operation, out string expression)
    {
        expression = string.Empty;
        if (_imperativeBuilderAlias is null)
            return false;

        if (TryGetImperativeStaticMarkupString(operation) is not string staticMarkup)
            return false;

        expression = ToJavaScriptString(staticMarkup);
        return true;
    }

    internal bool TryRewriteStaticMarkupStringObjectCreation(IObjectCreationOperation operation, out string expression)
    {
        expression = string.Empty;
        if (_imperativeBuilderAlias is null)
            return false;

        if (TryGetImperativeStaticMarkupString(operation) is not string staticMarkup)
            return false;

        expression = ToJavaScriptString(staticMarkup);
        return true;
    }

    private bool TryRewriteImperativeBuilderInvocation(IInvocationOperation invocation, SenseArgument argument, out string expression)
    {
        expression = string.Empty;
        if (_imperativeBuilderAlias is null)
        {
            return false;
        }

        if (TryRewriteImperativeEventModifierInvocation(invocation, argument, out expression))
            return true;

        if (invocation.Instance is null)
            return false;

        var builderTarget = ResolveImperativeBuilderTarget(invocation.Instance);
        if (builderTarget is null)
            return false;

        switch (invocation.TargetMethod.Name)
        {
            case "OpenComponent":
                var componentReference = ResolveImperativeOpenComponentTarget(invocation);
                if (componentReference is null)
                    return false;

                var componentMetadataReference = ResolveImperativeComponentMetadataReference(invocation) ?? "null";
                expression = builderTarget + ".enterComponent(" + componentReference + ", " + componentMetadataReference + ")";
                return true;
            case "OpenElement":
                expression = builderTarget + ".enterElement(" + EmitImperativeArgument(invocation, argument, 1) + ")";
                return true;
            case "CloseElement":
                expression = builderTarget + ".leaveElement()";
                return true;
            case "CloseComponent":
                expression = builderTarget + ".leaveComponent()";
                return true;
            case "CloseRegion":
                expression = builderTarget + ".closeRegion()";
                return true;
            case "OpenRegion":
                expression = builderTarget + ".openRegion()";
                return true;
            case "AddContent":
                if (invocation.Arguments.Length >= 2 &&
                    RazorVueStaticMarkupValueHelper.IsMarkupStringType(invocation.TargetMethod.Parameters.ElementAtOrDefault(1)?.Type))
                {
                    if (TryResolveImperativeStaticMarkupString(invocation.Arguments[1].Value) is not { } staticMarkup)
                    {
                        throw CreateUnsupportedImperativeRenderLoweringException(
                            invocation,
                            $"RazorVue imperative render lowering only supports compile-time provable static MarkupString AddContent(...) in component '{_snapshot.Descriptor.FullName}'.");
                    }

                    expression = builderTarget + ".append(" + EmitStaticMarkupExpression(staticMarkup, argument, invocation) + ")";
                    return true;
                }

                expression = invocation.Arguments.Length >= 3
                    ? builderTarget + ".append(" + EmitImperativeContentValue(invocation, argument, 1) + ", " + EmitImperativeArgument(invocation, argument, 2) + ")"
                    : builderTarget + ".append(" + EmitImperativeContentValue(invocation, argument, 1) + ")";
                return true;
            case "AddMarkupContent":
                if (TryResolveImperativeStaticMarkupContent(invocation.Arguments[1].Value) is not { } markup)
                {
                    throw CreateUnsupportedImperativeRenderLoweringException(
                        invocation,
                        $"RazorVue imperative render lowering only supports compile-time provable static AddMarkupContent(...) in component '{_snapshot.Descriptor.FullName}'.");
                }

                expression = builderTarget + ".append(" + EmitStaticMarkupExpression(markup, argument, invocation) + ")";
                return true;
            case "AddAttribute":
                expression = builderTarget + ".setAttribute(" + EmitImperativeArgument(invocation, argument, 1) + ", " + EmitImperativeComponentParameterValue(invocation, argument, builderTarget, 2) + ")";
                return true;
            case "AddComponentParameter":
                expression = builderTarget + ".setComponentParameter(" + EmitImperativeArgument(invocation, argument, 1) + ", " + EmitImperativeComponentParameterValue(invocation, argument, builderTarget, 2) + ")";
                return true;
            case "AddMultipleAttributes":
                expression = builderTarget + ".mergeAttributes(" + EmitImperativeArgument(invocation, argument, 1) + ")";
                return true;
            case "SetKey":
                expression = builderTarget + ".setKey(" + EmitImperativeArgument(invocation, argument, 0) + ")";
                return true;
        }

        return false;
    }

    private bool TryRewriteImperativeEventModifierInvocation(
        IInvocationOperation invocation,
        SenseArgument argument,
        out string expression)
    {
        expression = string.Empty;
        if (!IsEventModifierInvocation(invocation.TargetMethod) ||
            invocation.Arguments.Length < 4)
        {
            return false;
        }

        var builderTarget = ResolveImperativeBuilderTarget(invocation.Arguments[0].Value);
        if (builderTarget is null)
            return false;

        var modifierPropertyName = invocation.TargetMethod.Name switch
        {
            "AddEventPreventDefaultAttribute" => "preventDefault",
            "AddEventStopPropagationAttribute" => "stopPropagation",
            _ => null
        };
        if (modifierPropertyName is null)
            return false;

        expression = builderTarget +
                     ".setEventModifier(" +
                     EmitImperativeArgument(invocation, argument, 2) +
                     ", " +
                     ToJavaScriptString(modifierPropertyName) +
                     ", " +
                     EmitImperativeArgument(invocation, argument, 3) +
                     ")";
        return true;
    }

    private string? TryGetImperativeStaticMarkupString(IOperation? operation)
        => RazorVueStaticMarkupValueHelper.TryGetStaticMarkupValue(
            operation,
            _snapshot.Compilation,
            TryGetImperativeLocalMarkupStringInitializer,
            TryGetImperativePropertyMarkupStringInitializer,
            TryGetImperativeFieldMarkupStringInitializer,
            TryGetImperativeStaticMarkupFactoryReturnedValue,
            IsSupportedImperativeStaticMarkupFactoryInvocation);

    private RazorVueStaticMarkupValueHelper.StaticMarkupResolution? TryResolveImperativeStaticMarkupString(IOperation? operation)
    {
        if (TryResolveEmittedImperativeStaticMarkupLocal(
                operation,
                TryGetImperativeLocalMarkupStringInitializer,
                TryGetImperativePropertyMarkupStringInitializer,
                TryGetImperativeFieldMarkupStringInitializer,
                out var localResolution))
        {
            return localResolution;
        }

        return RazorVueStaticMarkupValueHelper.TryResolveStaticMarkup(
            operation,
            _snapshot.Compilation,
            TryGetImperativeLocalMarkupStringInitializer,
            TryGetImperativePropertyMarkupStringInitializer,
            TryGetImperativeFieldMarkupStringInitializer,
            TryGetImperativeStaticMarkupFactoryReturnedValue,
            IsSupportedImperativeStaticMarkupFactoryInvocation);
    }

    private string? TryGetImperativeStaticMarkupContent(IOperation? operation)
        => RazorVueStaticMarkupValueHelper.TryGetStaticMarkupValue(
            operation,
            _snapshot.Compilation,
            TryGetImperativeLocalStaticMarkupInitializer,
            TryGetImperativePropertyStaticMarkupInitializer,
            TryGetImperativeFieldStaticMarkupInitializer,
            TryGetImperativeStaticMarkupFactoryReturnedValue,
            IsSupportedImperativeStaticMarkupFactoryInvocation);

    private RazorVueStaticMarkupValueHelper.StaticMarkupResolution? TryResolveImperativeStaticMarkupContent(IOperation? operation)
    {
        if (TryResolveEmittedImperativeStaticMarkupLocal(
                operation,
                TryGetImperativeLocalStaticMarkupInitializer,
                TryGetImperativePropertyStaticMarkupInitializer,
                TryGetImperativeFieldStaticMarkupInitializer,
                out var localResolution))
        {
            return localResolution;
        }

        return RazorVueStaticMarkupValueHelper.TryResolveStaticMarkup(
            operation,
            _snapshot.Compilation,
            TryGetImperativeLocalStaticMarkupInitializer,
            TryGetImperativePropertyStaticMarkupInitializer,
            TryGetImperativeFieldStaticMarkupInitializer,
            TryGetImperativeStaticMarkupFactoryReturnedValue,
            IsSupportedImperativeStaticMarkupFactoryInvocation);
    }

    private bool TryResolveEmittedImperativeStaticMarkupLocal(
        IOperation? operation,
        Func<ILocalSymbol, IOperation?> localInitializerResolver,
        Func<IPropertySymbol, IOperation?> propertyInitializerResolver,
        Func<IFieldSymbol, IOperation?> fieldInitializerResolver,
        out RazorVueStaticMarkupValueHelper.StaticMarkupResolution resolution)
    {
        resolution = default;
        if (RazorVueOperationNormalizer.Unwrap(operation) is not ILocalReferenceOperation localReference)
            return false;

        var initializer = localInitializerResolver(localReference.Local);
        if (initializer is null)
            return false;

        var resolved = RazorVueStaticMarkupValueHelper.TryResolveStaticMarkup(
            initializer,
            _snapshot.Compilation,
            localInitializerResolver,
            propertyInitializerResolver,
            fieldInitializerResolver,
            TryGetImperativeStaticMarkupFactoryReturnedValue,
            IsSupportedImperativeStaticMarkupFactoryInvocation);
        if (resolved is null)
            return false;

        // The local declarator/assignment is emitted by SemanticWalker and already
        // preserves factory argument evaluation. The AddContent/AddMarkupContent
        // consumption only needs the proven static vnode shape.
        resolution = RazorVueStaticMarkupValueHelper.StaticMarkupResolution.Create(resolved.Value.Markup);
        return true;
    }

    private IOperation? TryGetImperativeLocalMarkupStringInitializer(ILocalSymbol local)
    {
        if (_imperativeStaticMarkupLocalInitializers is not null &&
            _imperativeStaticMarkupLocalInitializers.TryGetValue(local, out var initializer))
        {
            return initializer;
        }

        return RazorVueSourceStableLocalInitializerHelper.TryGetSourceStableLocalInitializer(
            _snapshot.Compilation,
            local,
            RazorVueStaticMarkupValueHelper.IsMarkupStringType,
            out initializer)
            ? initializer
            : null;
    }

    private IOperation? TryGetImperativeLocalStaticMarkupInitializer(ILocalSymbol local)
    {
        if (_imperativeStaticMarkupLocalInitializers is not null &&
            _imperativeStaticMarkupLocalInitializers.TryGetValue(local, out var initializer))
        {
            return initializer;
        }

        return RazorVueSourceStableLocalInitializerHelper.TryGetSourceStableLocalInitializer(
            _snapshot.Compilation,
            local,
            RazorVueStaticMarkupValueHelper.IsStaticMarkupCarrierType,
            out initializer)
            ? initializer
            : null;
    }

    private IOperation? TryGetImperativePropertyMarkupStringInitializer(IPropertySymbol property)
    {
        foreach (var syntaxReference in property.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                continue;

            var semanticModel = _snapshot.Compilation.GetSemanticModel(declaration.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(
                    semanticModel,
                    declaration,
                    out var propertyOperation))
            {
                return propertyOperation;
            }
        }

        return null;
    }

    private IOperation? TryGetImperativePropertyStaticMarkupInitializer(IPropertySymbol property)
    {
        foreach (var syntaxReference in property.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                continue;

            var semanticModel = _snapshot.Compilation.GetSemanticModel(declaration.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(
                    semanticModel,
                    declaration,
                    out var propertyOperation))
            {
                return propertyOperation;
            }
        }

        return null;
    }

    private IOperation? TryGetImperativeFieldMarkupStringInitializer(IFieldSymbol field)
    {
        foreach (var syntaxReference in field.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not VariableDeclaratorSyntax declarator ||
                declarator.Initializer?.Value is null)
            {
                continue;
            }

            var semanticModel = _snapshot.Compilation.GetSemanticModel(declarator.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    declarator.Initializer.Value,
                    out var initializerOperation))
            {
                return initializerOperation;
            }
        }

        return null;
    }

    private IOperation? TryGetImperativeFieldStaticMarkupInitializer(IFieldSymbol field)
    {
        foreach (var syntaxReference in field.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not VariableDeclaratorSyntax declarator ||
                declarator.Initializer?.Value is null)
            {
                continue;
            }

            var semanticModel = _snapshot.Compilation.GetSemanticModel(declarator.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    declarator.Initializer.Value,
                    out var initializerOperation))
            {
                return initializerOperation;
            }
        }

        return null;
    }

    private bool IsSupportedImperativeStaticMarkupFactoryInvocation(IInvocationOperation invocation)
        => invocation is { Instance: null, TargetMethod.MethodKind: MethodKind.LocalFunction } ||
           IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance);

    private IOperation? TryGetImperativeStaticMarkupFactoryReturnedValue(IInvocationOperation invocation)
        => RazorVueImperativeRenderFragmentCarrierHelper.TryGetRenderFragmentFactoryReturnedValue(
            _snapshot.Compilation,
            invocation,
            out var returnedValue)
            ? returnedValue
            : null;

    private string EmitImperativeArgument(IInvocationOperation invocation, SenseArgument argument, int argumentIndex)
    {
        if (invocation.Arguments.Length <= argumentIndex)
            return "undefined";

        return EmitImperativeNestedExpression(invocation.Arguments[argumentIndex].Value, argument);
    }

    private string EmitImperativeContentValue(IInvocationOperation invocation, SenseArgument argument, int argumentIndex)
    {
        if (invocation.Arguments.Length <= argumentIndex)
            return "undefined";

        var value = invocation.Arguments[argumentIndex].Value;
        if (Unwrap(value) is ILocalReferenceOperation localReference &&
            IsImperativeRenderFragmentCarrierType(localReference.Local.Type))
        {
            return EmitImperativeNestedExpression(value, argument);
        }

        if (TryEmitImperativeRenderSlotFactory(value, out var renderSlotFactory))
            return renderSlotFactory;

        if (TryEmitImperativeContextualRenderSlotFactory(value, out var contextualRenderSlotFactory))
            return contextualRenderSlotFactory;

        if (TryEmitImperativeRenderFragmentFactoryInvocation(
                value,
                candidate => EmitImperativeNestedExpression(candidate, argument),
                out var factoryBackedRenderSlotFactory))
        {
            return factoryBackedRenderSlotFactory;
        }

        if (TryEmitImperativeStoredLocalRenderSlotValue(value, argument, out var storedRenderSlotValue))
            return storedRenderSlotValue;

        return EmitImperativeNestedExpression(value, argument);
    }

    private string EmitImperativeComponentParameterValue(IInvocationOperation invocation, SenseArgument argument, string builderTarget, int argumentIndex)
    {
        if (invocation.Arguments.Length <= argumentIndex)
            return "undefined";

        var value = invocation.Arguments[argumentIndex].Value;
        if (TryGetCurrentComponentSlotDescriptor(value, out var currentSlot))
        {
            return "__jazorCreateSlotReference(" +
                   EmitCurrentComponentSlotReference(currentSlot) + ", " +
                   (currentSlot.Parameters.IsDefaultOrEmpty ? "false" : "true") + ")";
        }

        if (Unwrap(value) is ILocalReferenceOperation localReference &&
            IsImperativeRenderFragmentCarrierType(localReference.Local.Type))
        {
            return EmitImperativeNestedExpression(value, argument);
        }

        if (TryEmitImperativeStoredLocalRenderSlotValue(value, argument, out var storedLocalRenderSlotValue))
            return storedLocalRenderSlotValue;

        if (TryEmitImperativeRenderSlotFactory(value, out var renderSlotFactory))
            return renderSlotFactory;

        if (TryEmitImperativeContextualRenderSlotFactory(value, out var contextualRenderSlotFactory))
            return contextualRenderSlotFactory;

        if (TryEmitImperativeRenderFragmentFactoryInvocation(
                value,
                candidate => EmitImperativeNestedExpression(candidate, argument),
                out var factoryBackedRenderSlotFactory))
        {
            return factoryBackedRenderSlotFactory;
        }

        if (IsImperativeUntypedRenderFragmentValue(value))
            return EmitImperativeNestedExpression(value, argument);

        if (IsImperativeTypedRenderFragmentValue(value))
            return EmitImperativeNestedExpression(value, argument);

        return EmitImperativeNestedExpression(value, argument);
    }

    private string EmitImperativeNestedExpression(IOperation operation, SenseArgument argument)
        => EmitExpression(operation, argument.WithNewScope());

    private bool TryEmitImperativeStoredLocalRenderSlotValue(
        IOperation operation,
        SenseArgument argument,
        out string expression)
    {
        expression = string.Empty;
        if (!TryGetImperativeRenderFragmentCarrierInitializer(operation, out var initializer))
            return false;

        if (initializer is null)
            return false;

        if (TryEmitImperativeRenderSlotFactory(initializer, out var renderSlotFactory))
        {
            expression = renderSlotFactory;
            return true;
        }

        if (TryEmitImperativeContextualRenderSlotFactory(initializer, out var contextualRenderSlotFactory))
        {
            expression = contextualRenderSlotFactory;
            return true;
        }

        if (TryEmitImperativeRenderFragmentFactoryInvocation(
                initializer,
                candidate => EmitImperativeNestedExpression(candidate, argument),
                out var factoryBackedRenderSlotFactory))
        {
            expression = factoryBackedRenderSlotFactory;
            return true;
        }

        if (IsImperativeUntypedRenderFragmentValue(initializer))
        {
            expression = EmitSetupExpression(initializer, argument.WithNewScope());
            return true;
        }

        if (IsImperativeTypedRenderFragmentValue(initializer))
        {
            expression = EmitSetupExpression(initializer, argument.WithNewScope());
            return true;
        }

        return false;
    }

    private bool TryGetImperativeRenderFragmentCarrierInitializer(
        IOperation operation,
        out IOperation? initializer)
    {
        initializer = null;
        var current = Unwrap(operation);
        switch (current)
        {
            case ILocalReferenceOperation localReference:
                if (_imperativeRenderFragmentLocalInitializers is not null &&
                    _imperativeRenderFragmentLocalInitializers.TryGetValue(localReference.Local, out var collectedInitializer))
                {
                    initializer = collectedInitializer;
                    return true;
                }

                initializer = TryGetImperativeLocalRenderFragmentInitializer(localReference.Local);
                return initializer is not null;

            case IPropertyReferenceOperation propertyReference
                when IsCurrentComponentMember(propertyReference.Property, propertyReference.Instance):
                initializer = TryGetImperativeCurrentComponentRenderFragmentMemberInitializer(propertyReference.Property);
                return initializer is not null;

            case IFieldReferenceOperation fieldReference
                when IsCurrentComponentMember(fieldReference.Field, fieldReference.Instance):
                initializer = TryGetImperativeCurrentComponentRenderFragmentMemberInitializer(fieldReference.Field);
                return initializer is not null;

            default:
                return false;
        }
    }

    internal bool TryRewriteVariableDeclarator(
        IVariableDeclaratorOperation operation,
        SenseArgument argument,
        out VariableDeclarator declarator)
    {
        declarator = default!;
        if (TryRewriteScopedLocalAliasVariableDeclarator(operation, argument, out declarator))
            return true;

        if (_imperativeBuilderAlias is null)
        {
            return false;
        }

        if (TryRewriteImperativeStaticMarkupVariableDeclarator(operation, argument, out var expression))
        {
            declarator = new VariableDeclarator(new Identifier(operation.Symbol.Name), ParseJavaScriptExpression(expression));
            return true;
        }

        if (TryRewriteImperativeComponentTypeVariableDeclarator(operation, out expression))
        {
            declarator = new VariableDeclarator(new Identifier(operation.Symbol.Name), ParseJavaScriptExpression(expression));
            return true;
        }

        if (!IsImperativeRenderFragmentCarrierType(operation.Symbol.Type))
            return false;

        if (RazorVueImperativeRenderFragmentCarrierHelper.IsSourceStableLocalRenderFragmentInitializerInvalidatedByLaterWrites(
                _snapshot.Compilation,
                operation.Symbol))
        {
            throw CreateImperativeRenderFragmentLocalCarrierInvalidatedException(operation);
        }

        var current = TryGetNormalizedImperativeVariableInitializer(operation) ??
                      operation.Initializer?.Value;
        if (current is null)
            return false;

        current = Unwrap(current) ?? current;
        if (TryEmitImperativeRenderSlotFactory(current, out var renderSlotFactory))
        {
            declarator = new VariableDeclarator(new Identifier(operation.Symbol.Name), ParseJavaScriptExpression(renderSlotFactory));
            return true;
        }

        if (TryEmitImperativeContextualRenderSlotFactory(current, out var contextualRenderSlotFactory))
        {
            declarator = new VariableDeclarator(new Identifier(operation.Symbol.Name), ParseJavaScriptExpression(contextualRenderSlotFactory));
            return true;
        }

        if (TryEmitImperativeRenderFragmentFactoryInvocation(
                current,
                candidate => EmitImperativeNestedExpression(candidate, argument),
                out var factoryBackedRenderSlotFactory))
        {
            declarator = new VariableDeclarator(new Identifier(operation.Symbol.Name), ParseJavaScriptExpression(factoryBackedRenderSlotFactory));
            return true;
        }

        return false;
    }

    private bool TryRewriteScopedLocalAliasVariableDeclarator(
        IVariableDeclaratorOperation operation,
        SenseArgument argument,
        out VariableDeclarator declarator)
    {
        declarator = default!;
        if (_scopedLocalAliases is null ||
            !_scopedLocalAliases.TryGetValue(operation.Symbol, out var alias))
        {
            return false;
        }

        var initializer = operation.Initializer?.Value is { } value
            ? _isShouldRenderStatementRewriteScopeActive &&
              TryEmitShouldRenderMethodGroupDelegateInitializer(value, out var methodGroupExpression)
                ? ParseJavaScriptExpression(methodGroupExpression)
                : ParseJavaScriptExpression(EmitSetupExpression(value, argument))
            : null;
        declarator = new VariableDeclarator(new Identifier(alias), initializer);
        return true;
    }

    private bool TryEmitShouldRenderMethodGroupDelegateInitializer(
        IOperation operation,
        out string expression)
    {
        expression = string.Empty;
        if (!TryGetMethodGroupReference(operation, out var methodReference))
            return false;

        if (methodReference.Method.MethodKind == MethodKind.LocalFunction)
        {
            expression = methodReference.Method.Name;
            return true;
        }

        if (!IsCurrentComponentMember(methodReference.Method, methodReference.Instance))
            return false;

        RecordRequiredSetupMethod(methodReference.Method);
        expression = ToLowerCamelCase(methodReference.Method.Name);
        return true;
    }

    private static bool TryGetMethodGroupReference(
        IOperation? operation,
        out IMethodReferenceOperation methodReference)
    {
        switch (RazorVueOperationNormalizer.Unwrap(operation))
        {
            case IMethodReferenceOperation directMethodReference:
                methodReference = directMethodReference;
                return true;
            case IDelegateCreationOperation delegateCreation:
                return TryGetMethodGroupReference(delegateCreation.Target, out methodReference);
            case IConversionOperation conversion:
                return TryGetMethodGroupReference(conversion.Operand, out methodReference);
            default:
                methodReference = default!;
                return false;
        }
    }

    internal bool TryRewriteSimpleAssignment(
        ISimpleAssignmentOperation operation,
        SenseArgument argument,
        out string expression)
    {
        expression = string.Empty;
        if (_isShouldRenderStatementRewriteScopeActive &&
            _scopedLocalAliases is not null &&
            RazorVueOperationNormalizer.Unwrap(operation.Target) is ILocalReferenceOperation shouldRenderLocalReference &&
            _scopedLocalAliases.TryGetValue(shouldRenderLocalReference.Local, out var shouldRenderLocalAlias) &&
            TryEmitShouldRenderMethodGroupDelegateInitializer(operation.Value, out var methodGroupExpression))
        {
            _ = argument;
            expression = shouldRenderLocalAlias + " = " + methodGroupExpression;
            return true;
        }

        if (_imperativeBuilderAlias is null ||
            operation.Target is not ILocalReferenceOperation localReference)
        {
            return false;
        }

        if (TryRewriteImperativeStaticMarkupLocalAssignment(operation, localReference, argument, out expression))
            return true;

        if (TryRewriteImperativeComponentTypeLocalAssignment(operation, localReference, out expression))
            return true;

        if (!IsImperativeRenderFragmentCarrierType(localReference.Local.Type))
            return false;

        if (RazorVueImperativeRenderFragmentCarrierHelper.IsSourceStableLocalRenderFragmentInitializerInvalidatedByLaterWrites(
                _snapshot.Compilation,
                localReference.Local))
        {
            throw CreateImperativeRenderFragmentLocalCarrierInvalidatedException(operation, localReference.Local);
        }

        var current = Unwrap(operation.Value) ?? operation.Value;
        if (TryEmitImperativeRenderSlotFactory(current, out var renderSlotFactory))
        {
            expression = localReference.Local.Name + " = " + renderSlotFactory;
            return true;
        }

        if (TryEmitImperativeContextualRenderSlotFactory(current, out var contextualRenderSlotFactory))
        {
            expression = localReference.Local.Name + " = " + contextualRenderSlotFactory;
            return true;
        }

        if (TryEmitImperativeRenderFragmentFactoryInvocation(
                current,
                candidate => EmitImperativeNestedExpression(candidate, argument),
                out var factoryBackedRenderSlotFactory))
        {
            expression = localReference.Local.Name + " = " + factoryBackedRenderSlotFactory;
            return true;
        }

        _ = argument;
        return false;
    }

    private bool TryRewriteImperativeStaticMarkupVariableDeclarator(
        IVariableDeclaratorOperation operation,
        SenseArgument argument,
        out string expression)
    {
        expression = string.Empty;
        if (!RazorVueStaticMarkupValueHelper.IsMarkupStringType(operation.Symbol.Type))
            return false;

        if (RazorVueSourceStableLocalInitializerHelper.IsSourceStableLocalInitializerInvalidatedByLaterWrites(
                _snapshot.Compilation,
                operation.Symbol,
                RazorVueStaticMarkupValueHelper.IsMarkupStringType))
        {
            throw CreateUnsupportedImperativeStaticMarkupLocalCarrierInvalidatedException(
                operation,
                operation.Symbol,
                "MarkupString AddContent(...)");
        }

        var current = TryGetNormalizedImperativeVariableInitializer(operation) ??
                      operation.Initializer?.Value;
        if (current is null)
            return false;

        if (TryResolveImperativeStaticMarkupCarrierValue(current) is not { } resolution)
            return false;

        expression = EmitStaticMarkupCarrierValueExpression(resolution, argument);
        return true;
    }

    private bool TryRewriteImperativeStaticMarkupLocalAssignment(
        ISimpleAssignmentOperation operation,
        ILocalReferenceOperation localReference,
        SenseArgument argument,
        out string expression)
    {
        expression = string.Empty;
        if (!RazorVueStaticMarkupValueHelper.IsMarkupStringType(localReference.Local.Type))
            return false;

        if (RazorVueSourceStableLocalInitializerHelper.IsSourceStableLocalInitializerInvalidatedByLaterWrites(
                _snapshot.Compilation,
                localReference.Local,
                RazorVueStaticMarkupValueHelper.IsMarkupStringType))
        {
            throw CreateUnsupportedImperativeStaticMarkupLocalCarrierInvalidatedException(
                operation,
                localReference.Local,
                "MarkupString AddContent(...)");
        }

        if (TryResolveImperativeStaticMarkupCarrierValue(operation.Value) is not { } resolution)
            return false;

        expression = localReference.Local.Name + " = " + EmitStaticMarkupCarrierValueExpression(resolution, argument);
        return true;
    }

    private bool TryRewriteImperativeComponentTypeVariableDeclarator(
        IVariableDeclaratorOperation operation,
        out string expression)
    {
        expression = string.Empty;
        if (!RazorVueComponentTypeCarrierHelper.IsSystemType(operation.Symbol.Type))
            return false;

        if (RazorVueSourceStableLocalInitializerHelper.IsSourceStableLocalInitializerInvalidatedByLaterWrites(
                _snapshot.Compilation,
                operation.Symbol,
                RazorVueComponentTypeCarrierHelper.IsSystemType))
        {
            throw CreateImperativeComponentTypeCarrierInvalidatedException(operation, operation.Symbol);
        }

        var current = TryGetNormalizedImperativeVariableInitializer(operation) ??
                      operation.Initializer?.Value;
        if (current is null)
            return false;

        if (!RazorVueComponentTypeCarrierHelper.TryResolveComponentType(
                _snapshot.Compilation,
                _snapshot.ComponentSymbol,
                current,
                out var componentType,
                out _))
        {
            return false;
        }

        if (TryResolveImperativeComponentReference(componentType) is not { } componentReference)
            return false;

        expression = componentReference;
        return true;
    }

    private bool TryRewriteImperativeComponentTypeLocalAssignment(
        ISimpleAssignmentOperation operation,
        ILocalReferenceOperation localReference,
        out string expression)
    {
        expression = string.Empty;
        if (!RazorVueComponentTypeCarrierHelper.IsSystemType(localReference.Local.Type))
            return false;

        if (RazorVueSourceStableLocalInitializerHelper.IsSourceStableLocalInitializerInvalidatedByLaterWrites(
                _snapshot.Compilation,
                localReference.Local,
                RazorVueComponentTypeCarrierHelper.IsSystemType))
        {
            throw CreateImperativeComponentTypeCarrierInvalidatedException(operation, localReference.Local);
        }

        if (!RazorVueComponentTypeCarrierHelper.TryResolveComponentType(
                _snapshot.Compilation,
                _snapshot.ComponentSymbol,
                operation.Value,
                out var componentType,
                out _))
        {
            return false;
        }

        if (TryResolveImperativeComponentReference(componentType) is not { } componentReference)
            return false;

        expression = localReference.Local.Name + " = " + componentReference;
        return true;
    }

    private void ThrowIfInvalidatedImperativeComponentTypeMemberCarrier(IOperation operation)
    {
        if (RazorVueComponentTypeCarrierHelper.TryGetInvalidatedSourceStableComponentTypeMember(
                _snapshot.Compilation,
                _snapshot.ComponentSymbol,
                operation,
                out var memberCarrier))
        {
            throw CreateImperativeComponentTypeMemberCarrierInvalidatedException(operation, memberCarrier);
        }
    }

    private RazorVueStaticMarkupValueHelper.StaticMarkupResolution? TryResolveImperativeStaticMarkupCarrierValue(IOperation? operation)
        => RazorVueStaticMarkupValueHelper.TryResolveStaticMarkup(
            operation,
            _snapshot.Compilation,
            TryGetImperativeLocalStaticMarkupInitializer,
            TryGetImperativePropertyStaticMarkupInitializer,
            TryGetImperativeFieldStaticMarkupInitializer,
            TryGetImperativeStaticMarkupFactoryReturnedValue,
            IsSupportedImperativeStaticMarkupFactoryInvocation);

    private IOperation? TryGetNormalizedImperativeVariableInitializer(IVariableDeclaratorOperation operation)
    {
        foreach (var syntaxReference in operation.Symbol.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not VariableDeclaratorSyntax declarator ||
                declarator.Initializer?.Value is null)
            {
                continue;
            }

            var semanticModel = _snapshot.Compilation.GetSemanticModel(declarator.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    declarator.Initializer.Value,
                    out var initializerOperation))
            {
                return initializerOperation;
            }
        }

        return null;
    }

    private IOperation? TryGetImperativeLocalRenderFragmentInitializer(ILocalSymbol local)
        => RazorVueImperativeRenderFragmentCarrierHelper.TryGetSourceStableLocalRenderFragmentInitializer(
            _snapshot.Compilation,
            local,
            out var initializer)
            ? initializer
            : null;

    private RazorVueCompilationIssueException CreateImperativeRenderFragmentLocalCarrierInvalidatedException(
        IVariableDeclaratorOperation operation)
        => CreateImperativeRenderFragmentLocalCarrierInvalidatedException(operation, operation.Symbol);

    private RazorVueCompilationIssueException CreateImperativeRenderFragmentLocalCarrierInvalidatedException(
        IOperation operation,
        ILocalSymbol local)
    {
        var origin = operation.Syntax is null
            ? _snapshot.Origins.FirstOrDefault()
            : RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), RazorVueOriginKind.Template);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.CanonicalizationFailed,
            RazorVueIssueSeverity.Error,
            $"RazorVue RenderFragment local '{local.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. Declaration-initialized local carriers must remain source-stable.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, _snapshot.Descriptor.FullName, origin);
    }

    private RazorVueCompilationIssueException CreateUnsupportedImperativeStaticMarkupLocalCarrierInvalidatedException(
        IOperation operation,
        ILocalSymbol local,
        string api)
    {
        var carrierKind = RazorVueStaticMarkupValueHelper.IsMarkupStringType(local.Type)
            ? "MarkupString"
            : "static markup";
        return CreateUnsupportedImperativeRenderLoweringException(
            operation,
            $"RazorVue imperative render lowering only supports compile-time provable static {api} in component '{_snapshot.Descriptor.FullName}'. RazorVue {carrierKind} local '{local.Name}' cannot be observed through later writes. Local carriers must remain source-stable.");
    }

    private RazorVueCompilationIssueException CreateImperativeComponentTypeCarrierInvalidatedException(
        IOperation operation,
        ILocalSymbol local)
    {
        var origin = operation.Syntax is null
            ? _snapshot.Origins.FirstOrDefault()
            : RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), RazorVueOriginKind.Template);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.CanonicalizationFailed,
            RazorVueIssueSeverity.Error,
            $"RazorVue System.Type local '{local.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. OpenComponent(Type) carriers must remain source-stable.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, _snapshot.Descriptor.FullName, origin);
    }

    private RazorVueCompilationIssueException CreateImperativeComponentTypeMemberCarrierInvalidatedException(
        IOperation operation,
        ISymbol member)
    {
        var origin = operation.Syntax is null
            ? _snapshot.Origins.FirstOrDefault()
            : RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), RazorVueOriginKind.Template);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.CanonicalizationFailed,
            RazorVueIssueSeverity.Error,
            $"RazorVue System.Type member '{member.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be observed through later writes. OpenComponent(Type) carriers must remain source-stable.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, _snapshot.Descriptor.FullName, origin);
    }

    private RazorVueCompilationIssueException CreateImperativeComponentTypeCarrierUnresolvedException(
        IOperation operation,
        INamedTypeSymbol componentType)
    {
        var origin = operation.Syntax is null
            ? _snapshot.Origins.FirstOrDefault()
            : RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), RazorVueOriginKind.Template);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.CanonicalizationFailed,
            RazorVueIssueSeverity.Error,
            $"RazorVue OpenComponent(Type) in component '{_snapshot.Descriptor.FullName}' requires a source-stable typeof(...) value that resolves to a visible RazorVue component. Type '{componentType.ToDisplayString()}' is not resolved as a component in this artifact.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, _snapshot.Descriptor.FullName, origin);
    }

    private RazorVueCompilationIssueException CreateImperativeComponentTypeCarrierRuntimeValueException(
        IOperation operation,
        ILocalSymbol local)
    {
        var origin = operation.Syntax is null
            ? _snapshot.Origins.FirstOrDefault()
            : RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), RazorVueOriginKind.Template);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.CanonicalizationFailed,
            RazorVueIssueSeverity.Error,
            $"RazorVue System.Type local '{local.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be used as a runtime render value. System.Type locals are only supported as source-stable OpenComponent(Type) carriers.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, _snapshot.Descriptor.FullName, origin);
    }

    private RazorVueCompilationIssueException CreateImperativeComponentTypeMemberCarrierRuntimeValueException(
        IOperation operation,
        ISymbol member)
    {
        var origin = operation.Syntax is null
            ? _snapshot.Origins.FirstOrDefault()
            : RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), RazorVueOriginKind.Template);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.CanonicalizationFailed,
            RazorVueIssueSeverity.Error,
            $"RazorVue System.Type member '{member.Name}' in component '{_snapshot.Descriptor.FullName}' cannot be used as a runtime render value. System.Type members are only supported as source-stable OpenComponent(Type) carriers.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, _snapshot.Descriptor.FullName, origin);
    }

    private bool TryEmitImperativeRenderSlotFactory(IOperation operation, out string expression)
    {
        expression = string.Empty;
        if (!TryGetAnonymousFunction(operation, out var anonymousFunction) ||
            !TryGetSingleBuilderParameter(anonymousFunction, out var builderParameter))
        {
            return false;
        }

        expression = EmitImperativeBuilderLambdaFactory(
            anonymousFunction,
            builderParameter,
            prefixParameterNames: null,
            capturedParameterAliases: null);
        return true;
    }

    private bool TryEmitImperativeContextualRenderSlotFactory(IOperation operation, out string expression)
    {
        expression = string.Empty;
        if (!TryGetTypedBuilderTemplate(operation, out var outerAnonymousFunction, out var builderAnonymousFunction) ||
            !TryGetSingleBuilderParameter(builderAnonymousFunction, out var builderParameter))
        {
            return false;
        }

        var templateParameterNames = outerAnonymousFunction.Symbol.Parameters
            .Select(static parameter => parameter.Name)
            .ToImmutableArray();
        expression = EmitImperativeBuilderLambdaFactory(
            builderAnonymousFunction,
            builderParameter,
            templateParameterNames,
            capturedParameterAliases: null);
        return true;
    }

    private bool TryEmitImperativeRenderFragmentFactoryInvocation(
        IOperation operation,
        Func<IOperation, string> emitCapturedInitializer,
        out string expression)
    {
        expression = string.Empty;
        if (Unwrap(operation) is not IInvocationOperation invocation ||
            !IsSupportedImperativeRenderFragmentFactoryInvocation(invocation) ||
            !IsImperativeRenderFragmentCarrierType(invocation.TargetMethod.ReturnType))
        {
            return false;
        }

        if (!TryGetImperativeRenderFragmentFactoryReturnedValue(invocation, out var returnedValue))
            return false;

        if (TryEmitImperativeContextualRenderSlotFactory(returnedValue, out var directContextualRenderSlotFactory))
        {
            if (!TryGetTypedBuilderTemplate(returnedValue, out var outerAnonymousFunction, out var builderAnonymousFunction) ||
                !TryGetSingleBuilderParameter(builderAnonymousFunction, out var builderParameter))
            {
                return false;
            }

            expression = EmitImperativeBuilderLambdaFactory(
                builderAnonymousFunction,
                builderParameter,
                prefixParameterNames: outerAnonymousFunction.Symbol.Parameters
                    .Select(static parameter => parameter.Name)
                    .ToImmutableArray(),
                capturedParameterAliases: null);
            expression = WrapImperativeFactoryCapturedArguments(invocation, expression, emitCapturedInitializer);
            return true;
        }

        if (TryEmitImperativeRenderSlotFactory(returnedValue, out var directRenderSlotFactory))
        {
            if (!TryGetAnonymousFunction(returnedValue, out var anonymousFunction) ||
                !TryGetSingleBuilderParameter(anonymousFunction, out var builderParameter))
            {
                return false;
            }

            expression = EmitImperativeBuilderLambdaFactory(
                anonymousFunction,
                builderParameter,
                prefixParameterNames: null,
                capturedParameterAliases: null);
            expression = WrapImperativeFactoryCapturedArguments(invocation, expression, emitCapturedInitializer);
            return true;
        }

        return false;
    }

    internal bool ShouldSkipImperativeLocalFunctionDeclaration(ILocalFunctionOperation operation)
        => _imperativeBuilderAlias is not null &&
           _imperativeMaterializedLocalRenderFragmentFactories is not null &&
           _imperativeMaterializedLocalRenderFragmentFactories.Contains(operation.Symbol);

    private HashSet<IMethodSymbol> CollectMaterializedImperativeLocalRenderFragmentFactories(
        IReadOnlyList<IOperation> operations)
    {
        var declaredCandidates = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var materializedInvocations = new HashSet<IInvocationOperation>();
        var materializedInvocationSyntaxes = new HashSet<SyntaxNode>();
        var materializedMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var nonMaterializedMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

        foreach (var operation in operations)
        {
            foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
            {
                if (current is ILocalFunctionOperation localFunction &&
                    IsSupportedImperativeLocalRenderFragmentFactory(localFunction.Symbol))
                {
                    declaredCandidates.Add(localFunction.Symbol);
                    continue;
                }

                CollectMaterializedImperativeLocalRenderFragmentFactoryInvocations(
                    current,
                    materializedInvocations,
                    materializedInvocationSyntaxes,
                    materializedMethods);
            }
        }

        foreach (var operation in operations)
        {
            foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
            {
                switch (current)
                {
                    case IInvocationOperation invocation
                        when IsSupportedImperativeLocalRenderFragmentFactoryInvocation(invocation) &&
                             !materializedInvocations.Contains(invocation) &&
                             (invocation.Syntax is null || !materializedInvocationSyntaxes.Contains(invocation.Syntax)):
                        nonMaterializedMethods.Add(invocation.TargetMethod);
                        break;

                    case IMethodReferenceOperation methodReference
                        when methodReference.Method.MethodKind == MethodKind.LocalFunction &&
                             IsImperativeRenderFragmentCarrierType(methodReference.Method.ReturnType):
                        nonMaterializedMethods.Add(methodReference.Method);
                        break;
                }
            }
        }

        var result = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        foreach (var method in declaredCandidates)
        {
            if (materializedMethods.Contains(method) && !nonMaterializedMethods.Contains(method))
                result.Add(method);
        }

        return result;
    }

    private void CollectMaterializedImperativeLocalRenderFragmentFactoryInvocations(
        IOperation operation,
        HashSet<IInvocationOperation> materializedInvocations,
        HashSet<SyntaxNode> materializedInvocationSyntaxes,
        HashSet<IMethodSymbol> materializedMethods)
    {
        switch (operation)
        {
            case IInvocationOperation invocation when ResolveImperativeBuilderTarget(invocation.Instance) is not null:
                if (string.Equals(invocation.TargetMethod.Name, "AddContent", System.StringComparison.Ordinal) &&
                    invocation.Arguments.Length >= 2)
                {
                    CollectMaterializedImperativeLocalRenderFragmentFactoryInvocation(
                        invocation.Arguments[1].Value,
                        materializedInvocations,
                        materializedInvocationSyntaxes,
                        materializedMethods);
                }
                else if ((string.Equals(invocation.TargetMethod.Name, "AddAttribute", System.StringComparison.Ordinal) ||
                          string.Equals(invocation.TargetMethod.Name, "AddComponentParameter", System.StringComparison.Ordinal)) &&
                         invocation.Arguments.Length >= 3)
                {
                    CollectMaterializedImperativeLocalRenderFragmentFactoryInvocation(
                        invocation.Arguments[2].Value,
                        materializedInvocations,
                        materializedInvocationSyntaxes,
                        materializedMethods);
                }

                break;

            case IVariableDeclaratorOperation declarator
                when IsImperativeRenderFragmentCarrierType(declarator.Symbol.Type):
                var initializer = TryGetNormalizedImperativeVariableInitializer(declarator) ??
                                  declarator.Initializer?.Value;
                CollectMaterializedImperativeLocalRenderFragmentFactoryInvocation(
                    initializer,
                    materializedInvocations,
                    materializedInvocationSyntaxes,
                    materializedMethods);
                break;

            case ISimpleAssignmentOperation assignment
                when assignment.Target is ILocalReferenceOperation localReference &&
                     IsImperativeRenderFragmentCarrierType(localReference.Local.Type):
                CollectMaterializedImperativeLocalRenderFragmentFactoryInvocation(
                    assignment.Value,
                    materializedInvocations,
                    materializedInvocationSyntaxes,
                    materializedMethods);
                break;
        }
    }

    private void CollectMaterializedImperativeLocalRenderFragmentFactoryInvocation(
        IOperation? operation,
        HashSet<IInvocationOperation> materializedInvocations,
        HashSet<SyntaxNode> materializedInvocationSyntaxes,
        HashSet<IMethodSymbol> materializedMethods)
    {
        if (Unwrap(operation) is not IInvocationOperation invocation ||
            !IsSupportedImperativeLocalRenderFragmentFactoryInvocation(invocation))
        {
            return;
        }

        materializedInvocations.Add(invocation);
        if (invocation.Syntax is not null)
            materializedInvocationSyntaxes.Add(invocation.Syntax);

        materializedMethods.Add(invocation.TargetMethod);
    }

    private bool IsSupportedImperativeRenderFragmentFactoryInvocation(IInvocationOperation invocation)
        => IsSupportedImperativeLocalRenderFragmentFactoryInvocation(invocation) ||
           IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance);

    private bool IsSupportedImperativeLocalRenderFragmentFactoryInvocation(IInvocationOperation invocation)
        => invocation is { Instance: null, TargetMethod.MethodKind: MethodKind.LocalFunction } &&
           IsSupportedImperativeLocalRenderFragmentFactory(invocation.TargetMethod);

    private bool IsSupportedImperativeLocalRenderFragmentFactory(IMethodSymbol method)
        => method.MethodKind == MethodKind.LocalFunction &&
           IsImperativeRenderFragmentCarrierType(method.ReturnType) &&
           CanMaterializeImperativeRenderFragmentFactoryMethod(method);

    private bool CanMaterializeImperativeRenderFragmentFactoryMethod(IMethodSymbol method)
    {
        if (!RazorVueImperativeRenderFragmentCarrierHelper.TryGetRenderFragmentFactoryReturnedValue(
                _snapshot.Compilation,
                method,
                out var returnedValue))
        {
            return false;
        }

        if (TryGetTypedBuilderTemplate(returnedValue, out _, out var builderAnonymousFunction))
            return TryGetSingleBuilderParameter(builderAnonymousFunction, out _);

        return TryGetAnonymousFunction(returnedValue, out var anonymousFunction) &&
               TryGetSingleBuilderParameter(anonymousFunction, out _);
    }

    private string? TryEmitImperativeRenderFragmentLocalDeclarationInitializer(
        IOperation initializer,
        ImmutableHashSet<ILocalSymbol> currentLocalScope,
        ImmutableHashSet<IParameterSymbol> currentParameterScope)
    {
        var current = Unwrap(initializer) ?? initializer;

        if (TryEmitImperativeRenderSlotFactory(current, out var renderSlotFactory))
            return renderSlotFactory;

        if (TryEmitImperativeContextualRenderSlotFactory(current, out var contextualRenderSlotFactory))
            return contextualRenderSlotFactory;

        if (TryEmitImperativeRenderFragmentFactoryInvocation(
                current,
                candidate => EmitScopedExpression(candidate, currentLocalScope, currentParameterScope),
                out var factoryBackedRenderSlotFactory))
        {
            return factoryBackedRenderSlotFactory;
        }

        _ = currentLocalScope;
        _ = currentParameterScope;
        return null;
    }

    private string EmitImperativeBuilderLambdaFactory(
        IAnonymousFunctionOperation builderAnonymousFunction,
        IParameterSymbol builderParameter,
        ImmutableArray<string>? prefixParameterNames,
        IReadOnlyDictionary<IParameterSymbol, string>? capturedParameterAliases)
    {
        var renderContextParameterAlias = AllocateImperativeScratchBuilderAlias();
        var builderArgument = new SenseArgument(Sense.FunctionBody, UseImportAliases: true);
        var parameterAliases = new Dictionary<IParameterSymbol, string>(SymbolEqualityComparer.Default)
        {
            [builderParameter] = renderContextParameterAlias
        };
        if (capturedParameterAliases is not null)
        {
            foreach (var pair in capturedParameterAliases)
                parameterAliases[pair.Key] = pair.Value;
        }

        var functionBody = WithImperativeBuilderAlias(
            renderContextParameterAlias,
            () => WithImperativeBuilderParameterTargets(
                parameterAliases,
                () => WithScopedParameterAliases(
                    parameterAliases,
                    () => WithScopedParameterAliases(
                        builderAnonymousFunction.Symbol.Parameters,
                        builderAnonymousFunction.Symbol.Parameters
                            .Select(parameter => parameterAliases.TryGetValue(parameter, out var alias) ? alias : parameter.Name)
                            .ToArray(),
                        () =>
                        {
                            var walker = new SemanticWalker(
                                _snapshot.ComponentSymbol,
                                _compilerModuleContext.DeclaredNames)
                            {
                                Host = new RazorVueCompilerHost(this)
                            };
                            var statements = walker.TranslateStatementSequence(
                                builderAnonymousFunction.Body.Operations,
                                builderArgument);
                            return NormalizeImperativeFunctionBody(
                                new FunctionBody(NodeList.From(statements), strict: true),
                                renderContextParameterAlias,
                                appendTerminalReturn: false);
                        }))));
        var body = NormalizeImperativeFunctionText(functionBody.ToKnRECMAScript());
        var endsWithTopLevelReturn = functionBody.Body.LastOrDefault() is ReturnStatement;

        var builder = new System.Text.StringBuilder();
        if (prefixParameterNames.HasValue)
        {
            builder.Append("(")
                .Append(string.Join(", ", prefixParameterNames.Value))
                .Append(") => {\n");
        }
        else
        {
            builder.Append("() => {\n");
        }

        builder.Append("const ")
            .Append(renderContextParameterAlias)
            .Append(" = __jazorCreateRenderContext(h);\n")
            .Append(body);

        if (body.Length > 0 && !body.EndsWith("\n", System.StringComparison.Ordinal))
            builder.Append('\n');

        if (!endsWithTopLevelReturn)
        {
            builder.Append("return ")
                .Append(renderContextParameterAlias)
                .Append(".finish();\n");
        }

        builder.Append('}');
        return builder.ToString();
    }

    private string WrapImperativeFactoryCapturedArguments(
        IInvocationOperation invocation,
        string factoryExpression,
        Func<IOperation, string> emitCapturedInitializer)
    {
        if (invocation.TargetMethod.Parameters.Length == 0)
            return factoryExpression;

        var binding = BindInvocationArguments(
            invocation.TargetMethod,
            invocation.Arguments,
            emitCapturedInitializer,
            new NotSupportedException(
                $"RazorVue imperative render fragment factory invocation does not support signature '{invocation.TargetMethod.ToDisplayString()}' because captured arguments must be normal by-value parameters."));
        return ComposeImperativeFactoryCaptureExpression(factoryExpression, binding);
    }

    private static string ComposeImperativeFactoryCaptureExpression(
        string factoryExpression,
        BoundInvocationArguments<string> binding)
    {
        if (ArgumentsAreAlreadyInDeclarationOrder(binding))
        {
            return "((" +
                   string.Join(", ", binding.ParametersByOrdinal.Select(static parameter => parameter.Name)) +
                   ") => " +
                   factoryExpression +
                   ")(" +
                   string.Join(", ", binding.ArgumentsByParameterOrdinal) +
                   ")";
        }

        var expression = factoryExpression;
        for (var index = binding.ArgumentsInSourceOrder.Length - 1; index >= 0; index--)
        {
            var argument = binding.ArgumentsInSourceOrder[index];
            expression = "((" + argument.Parameter.Name + ") => " + expression + ")(" + argument.Emitted + ")";
        }

        return expression;
    }

    private static Expression CreateImperativeFinishCall(string builderAlias)
        => new CallExpression(
            new MemberExpression(
                new Identifier(builderAlias),
                new Identifier("finish"),
                computed: false,
                optional: false),
            NodeList.Empty<Expression>(),
            optional: false);

    private sealed class ImperativeTopLevelReturnRewriter(string builderAlias) : AstRewriter
    {
        protected override object? VisitReturnStatement(ReturnStatement node)
        {
            if (node.Argument is not null)
                return node;

            return new ReturnStatement(CreateImperativeFinishCall(builderAlias));
        }

        protected override object VisitFunctionExpression(FunctionExpression node) => node;
        protected override object VisitArrowFunctionExpression(ArrowFunctionExpression node) => node;
    }

    private string AllocateImperativeScratchBuilderAlias()
        => "__jazorImperativeRenderContext0";

    private IOperation? TryGetImperativeCurrentComponentRenderFragmentMemberInitializer(ISymbol member)
        => RazorVueImperativeRenderFragmentCarrierHelper.TryGetCurrentComponentRenderFragmentMemberInitializer(
            _snapshot.Compilation,
            _snapshot.ComponentSymbol,
            member,
            Unwrap,
            IsSourceStableMutableCarrierMember,
            out var initializer)
            ? initializer
            : null;

    private bool IsSupportedImperativeCurrentComponentRenderFragmentCarrierMember(ISymbol member)
    {
        switch (member)
        {
            case IPropertySymbol propertySymbol:
                if (!IsImperativeRenderFragmentCarrierType(propertySymbol.Type))
                    return false;

                if (propertySymbol.SetMethod is null)
                    return true;

                if (!RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(propertySymbol))
                    return false;

                return !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(
                    _snapshot.Compilation,
                    propertySymbol);

            case IFieldSymbol fieldSymbol:
                if (!IsImperativeRenderFragmentCarrierType(fieldSymbol.Type))
                    return false;

                if (fieldSymbol.IsReadOnly)
                    return true;

                if (!RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(fieldSymbol))
                    return false;

                return !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(
                    _snapshot.Compilation,
                    fieldSymbol);

            default:
                return false;
        }
    }

    private bool IsSourceStableMutableCarrierMember(Compilation compilation, ISymbol member)
        => RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(member) &&
           !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(compilation, member);

    private IOperation? TryGetImperativePropertyRenderFragmentInitializer(IPropertySymbol property)
    {
        foreach (var syntaxReference in property.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                continue;

            var semanticModel = _snapshot.Compilation.GetSemanticModel(declaration.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(
                    semanticModel,
                    declaration,
                    out var propertyOperation))
            {
                return propertyOperation;
            }
        }

        return null;
    }

    private IOperation? TryGetImperativeFieldRenderFragmentInitializer(IFieldSymbol field)
    {
        foreach (var syntaxReference in field.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not VariableDeclaratorSyntax declarator ||
                declarator.Initializer?.Value is null)
            {
                continue;
            }

            var semanticModel = _snapshot.Compilation.GetSemanticModel(declarator.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    declarator.Initializer.Value,
                    out var initializerOperation))
            {
                return initializerOperation;
            }
        }

        return null;
    }

    private static bool IsImperativeRenderFragmentCarrierType(ITypeSymbol? typeSymbol)
        => RazorVueImperativeRenderFragmentCarrierHelper.IsRenderFragmentCarrierType(typeSymbol);

    private static bool IsRenderFragmentDelegateType(INamedTypeSymbol namedType)
        => string.Equals(
            namedType.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat),
            RazorVueRenderFragmentTypeHelper.RenderFragmentMetadataName,
            StringComparison.Ordinal) ||
           string.Equals(
            namedType.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat),
            RazorVueRenderFragmentTypeHelper.ParameterizedRenderFragmentMetadataName,
            StringComparison.Ordinal);

    private static bool TryGetTypedBuilderTemplate(
        IOperation? operation,
        out IAnonymousFunctionOperation outerAnonymousFunction,
        out IAnonymousFunctionOperation builderAnonymousFunction)
        => RazorVueImperativeRenderFragmentCarrierHelper.TryGetTypedBuilderTemplate(
            operation,
            out outerAnonymousFunction,
            out builderAnonymousFunction);

    private static bool TryGetReturnedAnonymousFunction(
        IOperation? body,
        out IAnonymousFunctionOperation anonymousFunction)
    {
        anonymousFunction = default!;
        switch (Unwrap(body))
        {
            case IAnonymousFunctionOperation direct:
                anonymousFunction = direct;
                return true;
            case IDelegateCreationOperation delegateCreation:
                return TryGetAnonymousFunction(delegateCreation.Target, out anonymousFunction);
            case IBlockOperation block when TryGetSingleReturnedValue(block, out var returnValue):
                return TryGetAnonymousFunction(returnValue, out anonymousFunction);
            case IReturnOperation returnOperation when returnOperation.ReturnedValue is not null:
                return TryGetAnonymousFunction(returnOperation.ReturnedValue, out anonymousFunction);
            default:
                return false;
        }
    }

    private string EmitStaticMarkupExpression(string markup)
        => EmitStaticMarkupExpression(
            RazorVueStaticMarkupValueHelper.StaticMarkupResolution.Create(markup),
            _compilerArgument,
            originOperation: null);

    private string EmitStaticMarkupExpression(
        RazorVueStaticMarkupValueHelper.StaticMarkupResolution resolution,
        SenseArgument compilerArgument,
        IOperation? originOperation)
    {
        ImmutableArray<RazorVueRenderNode> nodes;
        try
        {
            nodes = RazorVueStaticMarkupParser.Parse(
                resolution.Markup,
                ImmutableArray<RazorVueSourceOrigin>.Empty,
                new RazorVueStaticMarkupParser.Dependencies(
                    CreateImperativeLiteralStringOperation,
                    detail => new NotSupportedException(detail)));
        }
        catch (NotSupportedException exception)
        {
            throw CreateUnsupportedImperativeStaticMarkupException(
                originOperation ?? resolution.CapturedBindings.FirstOrDefault().Initializer,
                $"RazorVue imperative render lowering could not parse compile-time provable static raw markup in component '{_snapshot.Descriptor.FullName}': {exception.Message}");
        }

        var fragmentExpression = EmitStaticMarkupFragment(nodes);
        if (resolution.CapturedBindings.IsDefaultOrEmpty)
            return fragmentExpression;

        var currentExpression = fragmentExpression;
        for (var index = resolution.CapturedBindings.Length - 1; index >= 0; index--)
        {
            var binding = resolution.CapturedBindings[index];
            currentExpression =
                "((" + binding.ParameterSymbol.Name + ") => " +
                currentExpression +
                ")(" + EmitStaticMarkupCapturedBindingInitializer(binding.Initializer, compilerArgument) + ")";
        }

        return currentExpression;
    }

    private string EmitStaticMarkupCarrierValueExpression(
        RazorVueStaticMarkupValueHelper.StaticMarkupResolution resolution,
        SenseArgument compilerArgument)
    {
        var currentExpression = ToJavaScriptString(resolution.Markup);
        if (resolution.CapturedBindings.IsDefaultOrEmpty)
            return currentExpression;

        for (var index = resolution.CapturedBindings.Length - 1; index >= 0; index--)
        {
            var binding = resolution.CapturedBindings[index];
            currentExpression =
                "((" + binding.ParameterSymbol.Name + ") => " +
                currentExpression +
                ")(" + EmitStaticMarkupCapturedBindingInitializer(binding.Initializer, compilerArgument) + ")";
        }

        return currentExpression;
    }

    private RazorVueCompilationIssueException CreateUnsupportedImperativeStaticMarkupException(
        IOperation? operation,
        string detail)
    {
        if (operation is not null)
            return CreateUnsupportedImperativeRenderLoweringException(operation, detail);

        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedImperativeRenderLowering,
            RazorVueIssueSeverity.Error,
            detail,
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(
            issue,
            _snapshot.Descriptor.FullName,
            _snapshot.Origins.FirstOrDefault());
    }

    private string EmitStaticMarkupCapturedBindingInitializer(IOperation initializer, SenseArgument compilerArgument)
    {
        var current = RazorVueOperationNormalizer.Unwrap(initializer);
        if (current is null)
            return "undefined";

        return current switch
        {
            ILiteralOperation literal => EmitLiteral(literal),
            IDefaultValueOperation defaultValue when IsNullDefaultValue(defaultValue) => "null",
            _ => EmitExpression(current, compilerArgument)
        };
    }

    private string EmitStaticMarkupFragment(ImmutableArray<RazorVueRenderNode> nodes)
    {
        if (nodes.IsDefaultOrEmpty)
            return "null";

        if (nodes.Length == 1)
            return EmitStaticMarkupNode(nodes[0]);

        return "[" + string.Join(", ", nodes.Select(EmitStaticMarkupNode)) + "]";
    }

    private string EmitStaticMarkupNode(RazorVueRenderNode node)
        => node switch
        {
            RazorVueTextNode text => ToJavaScriptString(text.Text),
            RazorVueElementNode element => EmitVNodeCall(
                ToJavaScriptString(element.TagName),
                EmitStaticMarkupAttributes(element.Attributes),
                new OptionalJsArgument(EmitStaticMarkupFragment(element.Children.Children), true)),
            _ => throw new NotSupportedException(
                $"RazorVue imperative static markup lowering encountered unsupported node '{node.GetType().Name}' in component '{_snapshot.Descriptor.FullName}'.")
        };

    private OptionalJsArgument EmitStaticMarkupAttributes(ImmutableArray<RazorVueAttributeEntry> attributes)
    {
        if (attributes.IsDefaultOrEmpty)
            return OptionalJsArgument.Missing;

        var entries = new List<string>(attributes.Length);
        foreach (var attributeEntry in attributes)
        {
            if (attributeEntry is not RazorVueAttributeNode attribute)
            {
                throw new NotSupportedException(
                    $"RazorVue imperative static markup lowering does not support attribute spread in component '{_snapshot.Descriptor.FullName}'.");
            }

            entries.Add(
                ToJavaScriptString(attribute.Name) + ": " +
                EmitStaticMarkupAttributeValue(attribute.Value));
        }

        return new OptionalJsArgument("{ " + string.Join(", ", entries) + " }", true);
    }

    private static string EmitStaticMarkupAttributeValue(IOperation? value)
    {
        if (value is null)
            return "true";

        var current = RazorVueOperationNormalizer.Unwrap(value);
        if (current?.ConstantValue.HasValue != true)
        {
            throw new NotSupportedException("RazorVue imperative static markup lowering requires constant attribute values.");
        }

        return current.ConstantValue.Value switch
        {
            null => "true",
            string text => ToJavaScriptString(text),
            bool boolValue => boolValue ? "true" : "false",
            char ch => ToJavaScriptString(ch.ToString()),
            _ => System.Convert.ToString(current.ConstantValue.Value, System.Globalization.CultureInfo.InvariantCulture) ?? "null"
        };
    }

    private static string? TryGetImperativeConstantString(IOperation? operation)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current?.ConstantValue.HasValue == true &&
            current.ConstantValue.Value is string text)
        {
            return text;
        }

        return null;
    }

    private IOperation CreateImperativeLiteralStringOperation(string value)
    {
        var parseOptions = _snapshot.Compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
                           ?? CSharpParseOptions.Default;
        var source = "file static class __RazorVueImperativeLiteralHolder { internal static object Value => "
                     + SymbolDisplay.FormatLiteral(value, quote: true)
                     + "; }";
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var compilation = CSharpCompilation.Create(
            "__RazorVueImperativeLiteralHolder",
            [syntaxTree],
            _snapshot.Compilation.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var literal = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<LiteralExpressionSyntax>()
            .Single();
        return compilation.GetSemanticModel(syntaxTree).GetOperation(literal)
               ?? throw new InvalidOperationException("Could not materialize a Roslyn literal operation for imperative static markup lowering.");
    }

    private string? ResolveImperativeOpenComponentTarget(IInvocationOperation invocation)
    {
        if (invocation.TargetMethod.TypeArguments.Length == 1 &&
            invocation.TargetMethod.TypeArguments[0] is INamedTypeSymbol genericComponentType)
        {
            return TryResolveImperativeComponentReference(genericComponentType);
        }

        if (invocation.Arguments.Length < 2)
            return null;

        ThrowIfInvalidatedImperativeComponentTypeMemberCarrier(invocation.Arguments[1].Value);
        if (RazorVueComponentTypeCarrierHelper.TryResolveComponentType(
            _snapshot.Compilation,
            _snapshot.ComponentSymbol,
            invocation.Arguments[1].Value,
            out var explicitComponentType,
            out _))
        {
            return TryResolveImperativeComponentReference(explicitComponentType)
                   ?? throw CreateImperativeComponentTypeCarrierUnresolvedException(invocation, explicitComponentType);
        }

        return null;
    }

    private string? ResolveImperativeComponentMetadataReference(IInvocationOperation invocation)
    {
        if (invocation.TargetMethod.TypeArguments.Length == 1 &&
            invocation.TargetMethod.TypeArguments[0] is INamedTypeSymbol genericComponentType)
        {
            return TryResolveImperativeComponentMetadataReference(genericComponentType);
        }

        if (invocation.Arguments.Length < 2)
            return null;

        ThrowIfInvalidatedImperativeComponentTypeMemberCarrier(invocation.Arguments[1].Value);
        if (RazorVueComponentTypeCarrierHelper.TryResolveComponentType(
            _snapshot.Compilation,
            _snapshot.ComponentSymbol,
            invocation.Arguments[1].Value,
            out var explicitComponentType,
            out _))
        {
            return TryResolveImperativeComponentMetadataReference(explicitComponentType)
                   ?? throw CreateImperativeComponentTypeCarrierUnresolvedException(invocation, explicitComponentType);
        }

        return null;
    }

    private string? TryResolveImperativeComponentReference(INamedTypeSymbol componentType)
    {
        foreach (var pair in _resolvedComponents)
        {
            if (!string.Equals(pair.Value.FullName, componentType.ToDisplayString(), System.StringComparison.Ordinal))
                continue;

            return _componentReferences.TryGetValue(pair.Key, out var reference)
                ? reference
                : null;
        }

        return null;
    }

    private string? TryResolveImperativeComponentMetadataReference(INamedTypeSymbol componentType)
    {
        foreach (var pair in _resolvedComponents)
        {
            if (!string.Equals(pair.Value.FullName, componentType.ToDisplayString(), System.StringComparison.Ordinal))
                continue;

            return RazorVueArtifactFactory.CreateImperativeComponentMetadataAlias(pair.Key);
        }

        return null;
    }

    private string? ResolveImperativeBuilderTarget(IOperation? instance)
    {
        switch (Unwrap(instance))
        {
            case IParameterReferenceOperation parameterReference when IsRenderTreeBuilderType(parameterReference.Parameter.Type):
                return TryResolveImperativeBuilderParameterTarget(parameterReference.Parameter, out var parameterTarget)
                    ? parameterTarget
                    : parameterReference.Parameter.Name;
            case ILocalReferenceOperation localReference when IsRenderTreeBuilderType(localReference.Local.Type):
                return _scopedLocalAliases is not null &&
                       _scopedLocalAliases.TryGetValue(localReference.Local, out var localAlias)
                    ? localAlias
                    : localReference.Local.Name;
            default:
                return null;
        }
    }

    private bool TryResolveImperativeBuilderParameterTarget(IParameterSymbol parameter, out string target)
    {
        if (_imperativeBuilderParameterTargets is not null &&
            _imperativeBuilderParameterTargets.TryGetValue(parameter, out target))
        {
            return true;
        }

        target = string.Empty;
        return false;
    }

    private static bool IsRenderTreeBuilderType(ITypeSymbol? typeSymbol)
        => string.Equals(
            typeSymbol?.ToDisplayString(),
            "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder",
            System.StringComparison.Ordinal);

    private static bool IsEventModifierInvocation(IMethodSymbol method)
        => (string.Equals(method.Name, "AddEventPreventDefaultAttribute", System.StringComparison.Ordinal) ||
            string.Equals(method.Name, "AddEventStopPropagationAttribute", System.StringComparison.Ordinal)) &&
           string.Equals(
               method.ContainingType?.ToDisplayString(),
               "Microsoft.AspNetCore.Components.Web.WebRenderTreeBuilderExtensions",
               System.StringComparison.Ordinal);

    private static bool IsImperativeUntypedRenderFragmentValue(IOperation operation)
    {
        if (TryGetAnonymousFunction(operation, out var anonymousFunction))
            return TryGetSingleBuilderParameter(anonymousFunction, out _);

        return RazorVueRenderFragmentTypeHelper.IsUntypedRenderFragmentType(Unwrap(operation)?.Type);
    }

    private static bool IsImperativeTypedRenderFragmentValue(IOperation operation)
    {
        if (TryGetAnonymousFunction(operation, out var anonymousFunction))
            return TryGetTypedBuilderTemplateSignature(anonymousFunction);

        return RazorVueRenderFragmentTypeHelper.IsParameterizedRenderFragmentType(Unwrap(operation)?.Type);
    }

    private static bool TryGetSingleBuilderParameter(
        IAnonymousFunctionOperation anonymousFunction,
        out IParameterSymbol builderParameter)
        => RazorVueImperativeRenderFragmentCarrierHelper.TryGetSingleBuilderParameter(
            anonymousFunction,
            out builderParameter);

    private static bool TryGetTypedBuilderTemplateSignature(IAnonymousFunctionOperation anonymousFunction)
    {
        if (anonymousFunction.Symbol.Parameters.Length != 1)
            return false;

        var body = anonymousFunction.Body;
        if (body is null)
            return false;

        IOperation? returnedBuilderFactory = null;
        switch (Unwrap(body))
        {
            case IAnonymousFunctionOperation nestedAnonymousFunction:
                returnedBuilderFactory = nestedAnonymousFunction;
                break;
            case IDelegateCreationOperation delegateCreation:
                returnedBuilderFactory = delegateCreation;
                break;
            case IBlockOperation block when TryGetSingleReturnedValue(block, out var returnValue):
                returnedBuilderFactory = returnValue;
                break;
            case IReturnOperation returnOperation when returnOperation.ReturnedValue is not null:
                returnedBuilderFactory = returnOperation.ReturnedValue;
                break;
        }

        return TryGetAnonymousFunction(returnedBuilderFactory, out var builderAnonymousFunction) &&
               TryGetSingleBuilderParameter(builderAnonymousFunction, out _);
    }

    private static bool TryGetSingleReturnedValue(IBlockOperation block, out IOperation? returnedValue)
        => RazorVueImperativeRenderFragmentCarrierHelper.TryGetSingleReturnedValue(block, out returnedValue);

    private bool TryGetImperativeRenderFragmentFactoryReturnedValue(
        IInvocationOperation invocation,
        out IOperation returnedValue)
        => RazorVueImperativeRenderFragmentCarrierHelper.TryGetRenderFragmentFactoryReturnedValue(
            _snapshot.Compilation,
            invocation,
            out returnedValue);
}
