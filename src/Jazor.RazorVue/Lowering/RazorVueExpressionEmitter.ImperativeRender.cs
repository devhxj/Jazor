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

    private string EmitImperativeBlockBody(
        RazorVueImperativeBlockNode imperative,
        string builderAlias)
    {
        foreach (var operation in imperative.Operations)
            EnsureSupportedImperativeOperation(operation);

        var bodyArgument = new SenseArgument(Sense.FunctionBody, UseImportAliases: true);
        var visibleLocalAliases = new Dictionary<ILocalSymbol, string>(SymbolEqualityComparer.Default);
        var imperativeBuilderTargets = new Dictionary<IParameterSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var local in imperative.VisibleLocals)
            visibleLocalAliases[local] = local.Name;
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

        return WithImperativeBuilderAlias(
            builderAlias,
            () => WithImperativeBuilderParameterTargets(
                imperativeBuilderTargets,
                () => WithImperativeRenderFragmentLocalInitializers(
                    imperativeRenderFragmentLocalInitializers,
                    () => WithImperativeStaticMarkupLocalInitializers(
                        imperativeStaticMarkupLocalInitializers,
                        () => WithScopedLocalAliases(
                            visibleLocalAliases,
                            () => WithScopedParameterAliases(
                                imperative.VisibleParameters,
                                imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(),
                                () =>
                                {
                                    var statements = _semanticWalker.TranslateStatementSequence(imperative.Operations, bodyArgument);
                                    var functionBody = NormalizeImperativeFunctionBody(
                                        new FunctionBody(NodeList.From(statements), strict: true),
                                        builderAlias,
                                        appendTerminalReturn: false);
                                    return NormalizeImperativeFunctionText(functionBody.ToKnRECMAScript());
                                }))))));
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

    private void EnsureSupportedImperativeOperation(IOperation operation)
    {
        if (TryGetUnsupportedImperativeAsyncOperation(operation, out var unsupportedOperation))
            throw CreateUnsupportedImperativeRenderLoweringException(unsupportedOperation);
    }

    private static bool TryGetUnsupportedImperativeAsyncOperation(IOperation? operation, out IOperation unsupportedOperation)
    {
        unsupportedOperation = null!;
        if (operation is null)
            return false;

        foreach (var current in EnumerateImperativeOperationAndDescendants(operation))
        {
            if (current is IAnonymousFunctionOperation or ILocalFunctionOperation)
                continue;

            if (current is IUsingOperation { IsAsynchronous: true } or IUsingDeclarationOperation { IsAsynchronous: true })
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

    private static IEnumerable<IOperation> EnumerateImperativeOperationAndDescendants(IOperation root)
    {
        yield return root;
        if (root is IAnonymousFunctionOperation or ILocalFunctionOperation)
            yield break;

        foreach (var child in root.ChildOperations)
        {
            if (child is null)
                continue;

            foreach (var nested in EnumerateImperativeOperationAndDescendants(child))
                yield return nested;
        }
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

    private static string DescribeUnsupportedImperativeOperation(IOperation operation)
        => operation switch
        {
            IUsingOperation { IsAsynchronous: true } => "await using",
            IUsingDeclarationOperation { IsAsynchronous: true } => "await using",
            IAwaitOperation => "await",
            IForEachLoopOperation { IsAsynchronous: true } => "await foreach",
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
        if (_imperativeBuilderAlias is null ||
            invocation.Instance is null)
        {
            return false;
        }

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

                    expression = builderTarget + ".append(" + EmitStaticMarkupExpression(staticMarkup, argument) + ")";
                    return true;
                }

                expression = invocation.Arguments.Length >= 3
                    ? builderTarget + ".append(" + EmitImperativeArgument(invocation, argument, 1) + ", " + EmitImperativeArgument(invocation, argument, 2) + ")"
                    : builderTarget + ".append(" + EmitImperativeArgument(invocation, argument, 1) + ")";
                return true;
            case "AddMarkupContent":
                if (TryResolveImperativeStaticMarkupContent(invocation.Arguments[1].Value) is not { } markup)
                {
                    throw CreateUnsupportedImperativeRenderLoweringException(
                        invocation,
                        $"RazorVue imperative render lowering only supports compile-time provable static AddMarkupContent(...) in component '{_snapshot.Descriptor.FullName}'.");
                }

                expression = builderTarget + ".append(" + EmitStaticMarkupExpression(markup, argument) + ")";
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
        => RazorVueStaticMarkupValueHelper.TryResolveStaticMarkup(
            operation,
            _snapshot.Compilation,
            TryGetImperativeLocalMarkupStringInitializer,
            TryGetImperativePropertyMarkupStringInitializer,
            TryGetImperativeFieldMarkupStringInitializer,
            TryGetImperativeStaticMarkupFactoryReturnedValue,
            IsSupportedImperativeStaticMarkupFactoryInvocation);

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
        => RazorVueStaticMarkupValueHelper.TryResolveStaticMarkup(
            operation,
            _snapshot.Compilation,
            TryGetImperativeLocalStaticMarkupInitializer,
            TryGetImperativePropertyStaticMarkupInitializer,
            TryGetImperativeFieldStaticMarkupInitializer,
            TryGetImperativeStaticMarkupFactoryReturnedValue,
            IsSupportedImperativeStaticMarkupFactoryInvocation);

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
        => IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance);

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

        return EmitExpression(invocation.Arguments[argumentIndex].Value, argument);
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

        if (TryEmitImperativeStoredLocalRenderSlotValue(value, argument, out var storedLocalRenderSlotValue))
            return storedLocalRenderSlotValue;

        if (TryEmitImperativeRenderSlotFactory(value, out var renderSlotFactory))
            return renderSlotFactory;

        if (TryEmitImperativeContextualRenderSlotFactory(value, out var contextualRenderSlotFactory))
            return contextualRenderSlotFactory;

        if (IsImperativeUntypedRenderFragmentValue(value))
            return EmitExpression(value, argument);

        if (IsImperativeTypedRenderFragmentValue(value))
            return EmitExpression(value, argument);

        return EmitExpression(value, argument);
    }

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

        if (TryEmitImperativeRenderFragmentFactoryInvocation(initializer, out var factoryBackedRenderSlotFactory))
        {
            expression = factoryBackedRenderSlotFactory;
            return true;
        }

        if (IsImperativeUntypedRenderFragmentValue(initializer))
        {
            expression = EmitSetupExpression(initializer, argument);
            return true;
        }

        if (IsImperativeTypedRenderFragmentValue(initializer))
        {
            expression = EmitSetupExpression(initializer, argument);
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
        out string expression)
    {
        expression = string.Empty;
        if (_imperativeBuilderAlias is null ||
            !IsImperativeRenderFragmentCarrierType(operation.Symbol.Type))
        {
            return false;
        }

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
            expression = renderSlotFactory;
            return true;
        }

        if (TryEmitImperativeContextualRenderSlotFactory(current, out var contextualRenderSlotFactory))
        {
            expression = contextualRenderSlotFactory;
            return true;
        }

        if (TryEmitImperativeRenderFragmentFactoryInvocation(current, out var factoryBackedRenderSlotFactory))
        {
            expression = factoryBackedRenderSlotFactory;
            return true;
        }

        return false;
    }

    internal bool TryRewriteSimpleAssignment(
        ISimpleAssignmentOperation operation,
        SenseArgument argument,
        out string expression)
    {
        expression = string.Empty;
        if (_imperativeBuilderAlias is null ||
            operation.Target is not ILocalReferenceOperation localReference ||
            !IsImperativeRenderFragmentCarrierType(localReference.Local.Type))
        {
            return false;
        }

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

        if (TryEmitImperativeRenderFragmentFactoryInvocation(current, out var factoryBackedRenderSlotFactory))
        {
            expression = localReference.Local.Name + " = " + factoryBackedRenderSlotFactory;
            return true;
        }

        _ = argument;
        return false;
    }

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

    private bool TryEmitImperativeRenderFragmentFactoryInvocation(IOperation operation, out string expression)
    {
        expression = string.Empty;
        if (Unwrap(operation) is not IInvocationOperation invocation ||
            !IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance) ||
            !IsImperativeRenderFragmentCarrierType(invocation.TargetMethod.ReturnType))
        {
            return false;
        }

        if (!TryGetImperativeRenderFragmentFactoryReturnedValue(invocation, out var returnedValue))
            return false;

        var capturedParameterAliases = CreateImperativeFactoryCapturedParameterAliases(invocation);
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
                capturedParameterAliases);
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
                capturedParameterAliases);
            return true;
        }

        return false;
    }

    private string? TryEmitImperativeRenderFragmentLocalDeclarationInitializer(
        IOperation initializer,
        ImmutableHashSet<IParameterSymbol> currentParameterScope)
    {
        var current = Unwrap(initializer) ?? initializer;

        if (TryEmitImperativeRenderSlotFactory(current, out var renderSlotFactory))
            return renderSlotFactory;

        if (TryEmitImperativeContextualRenderSlotFactory(current, out var contextualRenderSlotFactory))
            return contextualRenderSlotFactory;

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
                                moduleDeclaredNames: new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default))
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

    private Dictionary<IParameterSymbol, string> CreateImperativeFactoryCapturedParameterAliases(
        IInvocationOperation invocation)
    {
        var aliases = new Dictionary<IParameterSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter is null)
                continue;

            var parameter = RazorVueMethodSymbolNormalizer.NormalizeParameter(invocation.TargetMethod, argument.Parameter);
            aliases[parameter] = EmitSetupExpression(argument.Value);
        }

        return aliases;
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
            _compilerArgument);

    private string EmitStaticMarkupExpression(
        RazorVueStaticMarkupValueHelper.StaticMarkupResolution resolution,
        SenseArgument compilerArgument)
    {
        var nodes = RazorVueStaticMarkupParser.Parse(
            resolution.Markup,
            ImmutableArray<RazorVueSourceOrigin>.Empty,
            new RazorVueStaticMarkupParser.Dependencies(
                CreateImperativeLiteralStringOperation,
                detail => new NotSupportedException(
                    $"RazorVue imperative render lowering could not parse static markup block '{resolution.Markup}' in component '{_snapshot.Descriptor.FullName}': {detail}")));
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

        if (invocation.Arguments.Length >= 2 &&
            Unwrap(invocation.Arguments[1].Value) is ITypeOfOperation { TypeOperand: INamedTypeSymbol explicitComponentType })
        {
            return TryResolveImperativeComponentReference(explicitComponentType);
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

        if (invocation.Arguments.Length >= 2 &&
            Unwrap(invocation.Arguments[1].Value) is ITypeOfOperation { TypeOperand: INamedTypeSymbol explicitComponentType })
        {
            return TryResolveImperativeComponentMetadataReference(explicitComponentType);
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
