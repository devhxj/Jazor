using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

        return WithImperativeBuilderAlias(
            builderAlias,
            () => WithImperativeBuilderParameterTargets(
                imperativeBuilderTargets,
                () => WithScopedLocalAliases(
                    visibleLocalAliases,
                    () => WithScopedParameterAliases(
                        imperative.VisibleParameters,
                        imperative.VisibleParameters.Select(static parameter => parameter.Name).ToArray(),
                        () =>
                        {
                            var statements = _semanticWalker.TranslateStatementSequence(imperative.Operations, bodyArgument);
                            var functionBody = new FunctionBody(NodeList.From(statements), strict: true);
                            return NormalizeImperativeFunctionBody(functionBody.ToKnRECMAScript(), builderAlias, appendTerminalReturn: false);
                        }))));
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
        var origin = operation.Syntax is null
            ? _snapshot.Origins.FirstOrDefault()
            : RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), RazorVueOriginKind.Template);
        var construct = DescribeUnsupportedImperativeOperation(operation);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedImperativeRenderLowering,
            RazorVueIssueSeverity.Error,
            $"RazorVue imperative render lowering does not support '{construct}' in component '{_snapshot.Descriptor.FullName}' because the current `.mjs`/render-function `.vue` artifact contract is synchronous and cannot carry async render semantics.",
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

    private string NormalizeImperativeFunctionBody(
        string functionBodyText,
        string builderAlias,
        bool appendTerminalReturn)
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
            return appendTerminalReturn
                ? "return " + builderAlias + ".complete();"
                : string.Empty;

        return RewriteTerminalReturns(innerBody, builderAlias, appendTerminalReturn);
    }

    private static string RewriteTerminalReturns(
        string bodyText,
        string builderAlias,
        bool appendTerminalReturn)
    {
        var lines = bodyText.Split('\n').ToList();
        for (var index = 0; index < lines.Count; index++)
        {
            var trimmed = lines[index].Trim();
            if (!trimmed.StartsWith("return", System.StringComparison.Ordinal))
                continue;

            if (trimmed == "return;" || trimmed == "return")
            {
                lines[index] = lines[index].Replace(trimmed, "return " + builderAlias + ".complete();");
                continue;
            }
        }

        if (appendTerminalReturn &&
            !lines.Any(static line => line.TrimStart().StartsWith("return ", System.StringComparison.Ordinal)))
        {
            lines.Add("return " + builderAlias + ".complete();");
        }

        return string.Join("\n", lines);
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
                expression = builderTarget + ".OpenComponent(" + componentReference + ", " + componentMetadataReference + ")";
                return true;
            case "OpenElement":
                expression = builderTarget + ".OpenElement(" + EmitImperativeArgument(invocation, argument, 1) + ")";
                return true;
            case "CloseElement":
            case "CloseComponent":
            case "CloseRegion":
                expression = builderTarget + "." + invocation.TargetMethod.Name + "()";
                return true;
            case "OpenRegion":
                expression = builderTarget + ".OpenRegion()";
                return true;
            case "AddContent":
                expression = invocation.Arguments.Length >= 3
                    ? builderTarget + ".AddContent(" + EmitImperativeArgument(invocation, argument, 1) + ", " + EmitImperativeArgument(invocation, argument, 2) + ")"
                    : builderTarget + ".AddContent(" + EmitImperativeArgument(invocation, argument, 1) + ")";
                return true;
            case "AddMarkupContent":
                if (TryGetImperativeConstantString(invocation.Arguments[1].Value) is not string markup)
                {
                    throw new NotSupportedException(
                        $"RazorVue imperative render lowering only supports constant AddMarkupContent(...) in component '{_snapshot.Descriptor.FullName}'.");
                }

                expression = builderTarget + ".AddContent(" + EmitStaticMarkupExpression(markup) + ")";
                return true;
            case "AddAttribute":
                expression = builderTarget + "." + invocation.TargetMethod.Name + "(" + EmitImperativeArgument(invocation, argument, 1) + ", " + EmitImperativeArgument(invocation, argument, 2) + ")";
                return true;
            case "AddComponentParameter":
                expression = builderTarget + ".AddComponentParameter(" + EmitImperativeArgument(invocation, argument, 1) + ", " + EmitImperativeComponentParameterValue(invocation, argument, builderTarget, 2) + ")";
                return true;
            case "AddMultipleAttributes":
                expression = builderTarget + ".AddMultipleAttributes(" + EmitImperativeArgument(invocation, argument, 1) + ")";
                return true;
            case "SetKey":
                expression = builderTarget + ".SetKey(" + EmitImperativeArgument(invocation, argument, 0) + ")";
                return true;
        }

        return false;
    }

    private string EmitImperativeArgument(IInvocationOperation invocation, SenseArgument argument, int argumentIndex)
    {
        if (invocation.Arguments.Length <= argumentIndex)
            return "undefined";

        return EmitSetupExpression(invocation.Arguments[argumentIndex].Value, argument);
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

        if (IsImperativeUntypedRenderFragmentValue(value))
            return "__jazorCreateBuilderSlot(" + EmitSetupExpression(value, argument) + ")";

        if (IsImperativeTypedRenderFragmentValue(value))
            return "__jazorCreateContextualBuilderSlot(" + EmitSetupExpression(value, argument) + ")";

        return EmitSetupExpression(value, argument);
    }

    private string EmitStaticMarkupExpression(string markup)
    {
        var nodes = RazorVueStaticMarkupParser.Parse(
            markup,
            ImmutableArray<RazorVueSourceOrigin>.Empty,
            new RazorVueStaticMarkupParser.Dependencies(
                CreateImperativeLiteralStringOperation,
                detail => new NotSupportedException(
                    $"RazorVue imperative render lowering could not parse static markup block '{markup}' in component '{_snapshot.Descriptor.FullName}': {detail}")));
        return EmitStaticMarkupFragment(nodes);
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

        if (Unwrap(operation)?.Type is not INamedTypeSymbol namedType)
            return false;

        return string.Equals(
            namedType.OriginalDefinition.ToDisplayString(),
            "Microsoft.AspNetCore.Components.RenderFragment",
            System.StringComparison.Ordinal);
    }

    private static bool IsImperativeTypedRenderFragmentValue(IOperation operation)
    {
        if (TryGetAnonymousFunction(operation, out var anonymousFunction))
            return TryGetTypedBuilderTemplateSignature(anonymousFunction);

        if (Unwrap(operation)?.Type is not INamedTypeSymbol namedType)
            return false;

        return string.Equals(
            namedType.OriginalDefinition.ToDisplayString(),
            "Microsoft.AspNetCore.Components.RenderFragment<T>",
            System.StringComparison.Ordinal);
    }

    private static bool TryGetSingleBuilderParameter(
        IAnonymousFunctionOperation anonymousFunction,
        out IParameterSymbol builderParameter)
    {
        builderParameter = anonymousFunction.Symbol.Parameters.FirstOrDefault(
            static parameter => IsRenderTreeBuilderType(parameter.Type))!;
        return builderParameter is not null && anonymousFunction.Symbol.Parameters.Length == 1;
    }

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
    {
        returnedValue = null;
        if (block.Operations.Length != 1 ||
            block.Operations[0] is not IReturnOperation returnOperation)
        {
            return false;
        }

        returnedValue = Unwrap(returnOperation.ReturnedValue);
        return returnedValue is not null;
    }
}
