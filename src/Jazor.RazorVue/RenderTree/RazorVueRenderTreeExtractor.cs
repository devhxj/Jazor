using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RenderTree;

/// <summary>
/// Extracts a framework-agnostic RazorVue render tree from BuildRenderTree operations.
/// </summary>
internal sealed class RazorVueRenderTreeExtractor
{
    /// <summary>
    /// Converts BuildRenderTree syntax/operations into a <see cref="RazorVueRenderFragment"/>.
    /// </summary>
    public RazorVueRenderFragment Extract(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        var method = snapshot.BuildRenderTreeMethod;
        if (method is null)
            return RazorVueRenderFragment.Empty;

        var builderParameters = method.Parameters
            .Where(static parameter => string.Equals(parameter.Name, "builder", StringComparison.Ordinal) ||
                                       string.Equals(parameter.Type.Name, "RenderTreeBuilder", StringComparison.Ordinal))
            .ToImmutableHashSet<IParameterSymbol>(SymbolEqualityComparer.Default);

        foreach (var reference in method.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
                continue;

            var model = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            var operation = methodSyntax.Body is not null
                ? model.GetOperation(methodSyntax.Body)
                : methodSyntax.ExpressionBody is not null
                    ? model.GetOperation(methodSyntax.ExpressionBody.Expression)
                    : null;

            if (operation is IBlockOperation block)
                return new Parser(snapshot, context.Compilation, context.Symbols, builderParameters).Parse(block.Operations);

            if (operation is not null)
                return new Parser(snapshot, context.Compilation, context.Symbols, builderParameters).Parse([operation]);
        }

        return RazorVueRenderFragment.Empty;
    }

    private sealed class Parser
    {
        private readonly RazorVueSemanticSnapshot _snapshot;
        private readonly Compilation _compilation;
        private readonly RazorVueCompilationSymbols _symbols;
        private ImmutableHashSet<IParameterSymbol> _builderParameters;
        private readonly HashSet<ILocalSymbol> _builderAliases;
        private readonly HashSet<IMethodSymbol> _activeRenderHelperMethods;
        private readonly List<RazorVueRenderNode> _rootChildren = [];
        private readonly Stack<OpenFrame> _openFrames = new();

        public Parser(
            RazorVueSemanticSnapshot snapshot,
            Compilation compilation,
            RazorVueCompilationSymbols symbols,
            ImmutableHashSet<IParameterSymbol> builderParameters,
            IEnumerable<ILocalSymbol>? builderAliases = null,
            IEnumerable<IMethodSymbol>? activeRenderHelperMethods = null)
        {
            _snapshot = snapshot;
            _compilation = compilation;
            _symbols = symbols;
            _builderParameters = builderParameters;
            _builderAliases = builderAliases is null
                ? new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default)
                : new HashSet<ILocalSymbol>(builderAliases, SymbolEqualityComparer.Default);
            _activeRenderHelperMethods = activeRenderHelperMethods is null
                ? new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default)
                : new HashSet<IMethodSymbol>(activeRenderHelperMethods, SymbolEqualityComparer.Default);
        }

        public RazorVueRenderFragment Parse(IEnumerable<IOperation> operations)
        {
            foreach (var operation in operations)
                ParseOperation(operation);

            if (_openFrames.Count > 0)
                throw CreateStructuralIssueForUnclosedFrames();

            return new RazorVueRenderFragment(_rootChildren.ToImmutableArray());
        }

        private void ParseOperation(IOperation? operation)
        {
            var current = Unwrap(operation);
            if (current is null)
                return;

            switch (current)
            {
                case IExpressionStatementOperation expressionStatement:
                    ParseExpressionStatement(expressionStatement);
                    break;
                case IConditionalOperation conditional:
                    AddNode(new RazorVueConditionalNode(
                        conditional.Condition,
                        ParseNestedBranch(conditional.WhenTrue),
                        ParseNestedBranch(conditional.WhenFalse),
                        CreateOrigins(current, RazorVueOriginKind.Template)));
                    break;
                case IForEachLoopOperation foreachLoop:
                    AddNode(new RazorVueForEachNode(
                        foreachLoop.Locals.Length > 0 ? foreachLoop.Locals[0].Name : "item",
                        foreachLoop.Locals.Length > 0 ? foreachLoop.Locals[0] : null,
                        foreachLoop.Collection,
                        ParseNestedBranch(foreachLoop.Body),
                        CreateOrigins(current, RazorVueOriginKind.Template)));
                    break;
                case IForLoopOperation forLoop:
                    AddNode(CreateForNode(forLoop));
                    break;
                case IBlockOperation block:
                    foreach (var child in block.Operations)
                        ParseOperation(child);
                    break;
                case IVariableDeclarationGroupOperation variableDeclarationGroup:
                    RegisterBuilderAliases(variableDeclarationGroup);
                    break;
                default:
                    break;
            }
        }

        private void ParseExpressionStatement(IExpressionStatementOperation expressionStatement)
        {
            var statementOperation = Unwrap(expressionStatement.Operation);
            if (statementOperation is ISimpleAssignmentOperation assignment)
            {
                RegisterBuilderAliasAssignment(assignment);
                return;
            }

            if (statementOperation is not IInvocationOperation invocation)
                return;

            if (TryParseCurrentComponentRenderHelperInvocation(invocation))
                return;

            if (!IsRenderTreeBuilderInvocation(invocation))
            {
                if (IsRenderTreeBuilderMethod(invocation.TargetMethod))
                {
                    throw CreateUnsupportedBuilderCall(
                        invocation,
                        $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' uses a RenderTreeBuilder receiver that RazorVue cannot track in component '{_snapshot.Descriptor.FullName}'. " +
                        "Use the active builder parameter or a direct local alias of that parameter.");
                }

                return;
            }

            switch (invocation.TargetMethod.Name)
            {
                case "OpenElement":
                    OpenElement(invocation);
                    break;
                case "CloseElement":
                    CloseCurrentNode(invocation, expectedComponent: false);
                    break;
                case "OpenComponent":
                    OpenComponent(invocation);
                    break;
                case "CloseComponent":
                    CloseCurrentNode(invocation, expectedComponent: true);
                    break;
                case "AddAttribute":
                    AddAttribute(invocation);
                    break;
                case "AddComponentParameter":
                    AddComponentParameter(invocation);
                    break;
                case "AddMultipleAttributes":
                    AddMultipleAttributes(invocation);
                    break;
                case "OpenRegion":
                    OpenRegion(invocation);
                    break;
                case "CloseRegion":
                    CloseRegion(invocation);
                    break;
                case "AddContent":
                    AddContent(invocation);
                    break;
                case "AddMarkupContent":
                    AddMarkupContent(invocation);
                    break;
                default:
                    throw CreateUnsupportedBuilderCall(
                        invocation,
                        $"RazorVue BuildRenderTree frontend does not support builder call '{GetBuilderCallDisplayName(invocation)}' in component '{_snapshot.Descriptor.FullName}'.");
            }
        }

        private void OpenElement(IInvocationOperation invocation)
        {
            var tagName = GetConstantStringArgument(invocation, 1);
            if (string.IsNullOrWhiteSpace(tagName))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' requires a constant element name in component '{_snapshot.Descriptor.FullName}'.");
            }

            _openFrames.Push(new ElementBuilder(tagName!, CreateOrigins(invocation, RazorVueOriginKind.Template)));
        }

        private void OpenComponent(IInvocationOperation invocation)
        {
            if (!TryResolveOpenComponent(invocation, out var componentType, out var resolutionName))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' must specify a concrete component type that RazorVue can resolve in component '{_snapshot.Descriptor.FullName}'.");
            }

            _openFrames.Push(new ComponentBuilder(
                componentType.Name,
                componentType.ToDisplayString(),
                resolutionName,
                CreateOrigins(invocation, RazorVueOriginKind.Template)));
        }

        private static bool TryResolveOpenComponent(
            IInvocationOperation invocation,
            out INamedTypeSymbol componentType,
            out string resolutionName)
        {
            componentType = default!;
            resolutionName = string.Empty;

            if (invocation.TargetMethod.TypeArguments.Length == 1 &&
                invocation.TargetMethod.TypeArguments[0] is INamedTypeSymbol genericComponentType)
            {
                componentType = genericComponentType;
                resolutionName = GetGenericComponentResolutionName(invocation, componentType.ToDisplayString());
                return true;
            }

            if (GetInvocationArgument(invocation, 1) is ITypeOfOperation { TypeOperand: INamedTypeSymbol explicitComponentType } typeOfOperation)
            {
                componentType = explicitComponentType;
                resolutionName = GetTypeOfComponentResolutionName(typeOfOperation, componentType.ToDisplayString());
                return true;
            }

            return false;
        }

        private static string GetGenericComponentResolutionName(IInvocationOperation invocation, string fallback)
        {
            if (invocation.Syntax is not InvocationExpressionSyntax invocationSyntax)
                return fallback;

            if (invocationSyntax.Expression is not MemberAccessExpressionSyntax { Name: GenericNameSyntax genericName })
                return fallback;

            if (genericName.TypeArgumentList.Arguments.Count != 1)
                return fallback;

            return genericName.TypeArgumentList.Arguments[0].ToString();
        }

        private static string GetTypeOfComponentResolutionName(ITypeOfOperation typeOfOperation, string fallback)
            => typeOfOperation.Syntax is TypeOfExpressionSyntax { Type: { } typeSyntax }
                ? typeSyntax.ToString()
                : fallback;

        private void OpenRegion(IInvocationOperation invocation)
            => _openFrames.Push(new RegionScope(CreateOrigins(invocation, RazorVueOriginKind.Template)));

        private void CloseRegion(IInvocationOperation invocation)
        {
            if (_openFrames.Count == 0)
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered 'CloseRegion' without a matching open region in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (_openFrames.Peek() is not RegionScope)
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered 'CloseRegion' while the current open frame is {_openFrames.Peek().Describe()} in component '{_snapshot.Descriptor.FullName}'.");
            }

            _openFrames.Pop();
        }

        private void CloseCurrentNode(IInvocationOperation invocation, bool expectedComponent)
        {
            if (_openFrames.Count == 0)
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered '{invocation.TargetMethod.Name}' without a matching open frame in component '{_snapshot.Descriptor.FullName}'.");

            if (_openFrames.Peek() is not OpenNodeBuilder current)
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered '{invocation.TargetMethod.Name}' while the current open frame is {_openFrames.Peek().Describe()} in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (current is ComponentBuilder != expectedComponent)
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered '{invocation.TargetMethod.Name}' while the current open node is {current.Describe()} in component '{_snapshot.Descriptor.FullName}'.");
            }

            _openFrames.Pop();
            AddNode(current.Build());
        }

        private void AddAttribute(IInvocationOperation invocation)
        {
            var name = GetConstantStringArgument(invocation, 1);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' requires a constant attribute name in component '{_snapshot.Descriptor.FullName}'.");
            }

            var value = GetInvocationArgument(invocation, 2);
            var currentNode = GetRequiredOpenNodeBuilder(invocation);
            if (TryHandleComponentSlotValue(currentNode, name!, value, invocation))
                return;

            if (string.Equals(name, "ChildContent", StringComparison.Ordinal) &&
                TryParseChildContent(value, out var childContent))
            {
                foreach (var child in childContent.Children)
                    currentNode.AddChild(child);
                return;
            }

            if (ShouldOmitElementAttribute(currentNode, value))
                return;

            currentNode.AddAttribute(new RazorVueAttributeNode(
                name!,
                value,
                CreateOrigins(invocation, RazorVueOriginKind.Template)));
        }

        private void AddComponentParameter(IInvocationOperation invocation)
        {
            var name = GetConstantStringArgument(invocation, 1);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' requires a constant parameter name in component '{_snapshot.Descriptor.FullName}'.");
            }

            var value = GetInvocationArgument(invocation, 2);
            var currentNode = GetRequiredOpenComponentBuilder(invocation);
            if (TryHandleComponentSlotValue(currentNode, name!, value, invocation))
                return;

            if (string.Equals(name, "ChildContent", StringComparison.Ordinal) &&
                TryParseChildContent(value, out var childContent))
            {
                foreach (var child in childContent.Children)
                    currentNode.AddChild(child);
                return;
            }

            currentNode.AddAttribute(new RazorVueAttributeNode(
                name!,
                value,
                CreateOrigins(invocation, RazorVueOriginKind.Template)));
        }

        private void AddMultipleAttributes(IInvocationOperation invocation)
        {
            var value = GetInvocationArgument(invocation, 1);
            if (value is null || IsConstantNull(value))
                return;

            var currentNode = GetRequiredOpenNodeBuilder(invocation);
            currentNode.AddAttribute(new RazorVueAttributeSpreadNode(
                value,
                CreateOrigins(invocation, RazorVueOriginKind.Template)));
        }

        private void AddContent(IInvocationOperation invocation)
        {
            var value = GetInvocationArgument(invocation, 1);
            if (value is null || IsConstantNull(value))
                return;

            var origins = CreateOrigins(invocation, RazorVueOriginKind.Template);
            if (TryResolveSlotOutlet(value, out var slotName))
            {
                AddNode(new RazorVueSlotOutletNode(
                    slotName,
                    GetInvocationArgument(invocation, 2),
                    origins));
                return;
            }

            if (IsMarkupStringAddContent(invocation))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' emits raw markup that RazorVue cannot safely canonicalize in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (TryParseAddContentRenderFragment(invocation, value, out var fragment))
            {
                foreach (var child in fragment.Children)
                    AddNode(child);
                return;
            }

            if (IsRenderFragmentAddContent(invocation))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' uses a RenderFragment shape that RazorVue cannot canonicalize in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (TryGetConstantString(value) is string text)
            {
                AddNode(new RazorVueTextNode(text, origins));
                return;
            }

            AddNode(new RazorVueExpressionNode(value, origins));
        }

        private void AddMarkupContent(IInvocationOperation invocation)
        {
            if (TryGetConstantString(GetInvocationArgument(invocation, 1)) is not string markup ||
                string.IsNullOrEmpty(markup))
                return;

            throw CreateUnsupportedBuilderCall(
                invocation,
                $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' emits raw markup that RazorVue cannot safely canonicalize in component '{_snapshot.Descriptor.FullName}'.");
        }

        private RazorVueRenderFragment ParseNestedBranch(IOperation? operation)
        {
            var current = Unwrap(operation);
            if (current is null)
                return RazorVueRenderFragment.Empty;

            return current switch
            {
                IBlockOperation block => new Parser(_snapshot, _compilation, _symbols, _builderParameters, _builderAliases, _activeRenderHelperMethods).Parse(block.Operations),
                _ => new Parser(_snapshot, _compilation, _symbols, _builderParameters, _builderAliases, _activeRenderHelperMethods).Parse([current])
            };
        }

        private RazorVueForNode CreateForNode(IForLoopOperation loop)
        {
            var analyzedLoop = RazorVueForLoopAnalyzer.AnalyzeRequired(
                loop,
                Unwrap,
                _snapshot.Descriptor.FullName);

            return new RazorVueForNode(
                analyzedLoop.VariableName,
                loop.Locals.Length > 0 ? loop.Locals[0] : null,
                analyzedLoop.InitialValue,
                analyzedLoop.ConditionKind,
                analyzedLoop.LimitValue,
                analyzedLoop.StepKind,
                analyzedLoop.StepValue,
                ParseNestedBranch(loop.Body),
                CreateOrigins(loop, RazorVueOriginKind.Template));
        }

        private void AddNode(RazorVueRenderNode node)
        {
            if (TryGetNearestOpenNodeBuilder(out var currentNode))
                currentNode.AddChild(node);
            else
                _rootChildren.Add(node);
        }

        private bool IsRenderTreeBuilderInvocation(IInvocationOperation invocation)
        {
            if (_builderParameters.Count == 0)
                return false;

            return IsKnownBuilderReference(invocation.Instance);
        }

        private bool TryParseCurrentComponentRenderHelperInvocation(IInvocationOperation invocation)
        {
            if (!IsCurrentComponentRenderHelperCandidate(invocation.TargetMethod, invocation.Instance))
                return false;

            if (!TryGetSupportedRenderHelperBuilderParameter(invocation.TargetMethod, out var builderParameter, out var failureMessage))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    failureMessage);
            }

            if (invocation.Arguments.Length != 1)
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must be invoked with exactly one RenderTreeBuilder argument in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (!IsKnownBuilderReference(invocation.Arguments[0].Value))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must receive the active RenderTreeBuilder parameter or a direct local alias in component '{_snapshot.Descriptor.FullName}'.");
            }

            ParseRenderHelperBody(invocation, builderParameter);
            return true;
        }

        private bool IsKnownBuilderReference(IOperation? operation)
        {
            return Unwrap(operation) switch
            {
                IParameterReferenceOperation parameterReference => _builderParameters.Contains(parameterReference.Parameter),
                ILocalReferenceOperation localReference => _builderAliases.Contains(localReference.Local),
                _ => false
            };
        }

        private void RegisterBuilderAliases(IVariableDeclarationGroupOperation declarationGroup)
        {
            foreach (var declaration in declarationGroup.Declarations)
            {
                foreach (var declarator in declaration.Declarators)
                    SetBuilderAlias(declarator.Symbol, declarator.Initializer?.Value);
            }
        }

        private void RegisterBuilderAliasAssignment(ISimpleAssignmentOperation assignment)
        {
            if (assignment.Target is not ILocalReferenceOperation localReference)
                return;

            SetBuilderAlias(localReference.Local, assignment.Value);
        }

        private void SetBuilderAlias(ILocalSymbol localSymbol, IOperation? value)
        {
            if (IsRenderTreeBuilderType(localSymbol.Type) && IsKnownBuilderReference(value))
            {
                _builderAliases.Add(localSymbol);
                return;
            }

            _builderAliases.Remove(localSymbol);
        }

        private bool IsCurrentComponentRenderHelperCandidate(IMethodSymbol method, IOperation? instance)
        {
            if (!ContainsRenderTreeBuilderParameter(method))
                return false;

            return IsCurrentComponentMethod(method, instance);
        }

        private bool TryGetSupportedRenderHelperBuilderParameter(
            IMethodSymbol method,
            out IParameterSymbol builderParameter,
            out string failureMessage)
        {
            builderParameter = default!;
            failureMessage = string.Empty;

            if (!method.ReturnsVoid)
            {
                failureMessage =
                    $"BuildRenderTree helper method '{method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}' must return void in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            if (method.IsGenericMethod)
            {
                failureMessage =
                    $"BuildRenderTree helper method '{method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}' must not be generic in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            var builderParameters = method.Parameters
                .Where(static parameter => IsRenderTreeBuilderType(parameter.Type))
                .ToArray();

            if (builderParameters.Length != 1)
            {
                failureMessage =
                    $"BuildRenderTree helper method '{method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}' must declare exactly one RenderTreeBuilder parameter in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            if (method.Parameters.Length != 1)
            {
                failureMessage =
                    $"BuildRenderTree helper method '{method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}' must not declare extra non-RenderTreeBuilder parameters in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            builderParameter = builderParameters[0];
            return true;
        }

        private void ParseRenderHelperBody(IInvocationOperation invocation, IParameterSymbol builderParameter)
        {
            if (!_activeRenderHelperMethods.Add(invocation.TargetMethod))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' is recursive; RazorVue does not support recursive render helpers in component '{_snapshot.Descriptor.FullName}'.");
            }

            try
            {
                var operations = GetRenderHelperOperations(invocation);
                ExecuteWithBuilderScope(
                    ImmutableHashSet.Create<IParameterSymbol>(SymbolEqualityComparer.Default, builderParameter),
                    () =>
                    {
                        foreach (var operation in operations)
                            ParseOperation(operation);
                    });
            }
            finally
            {
                _activeRenderHelperMethods.Remove(invocation.TargetMethod);
            }
        }

        private ImmutableArray<IOperation> GetRenderHelperOperations(IInvocationOperation invocation)
        {
            foreach (var syntaxReference in invocation.TargetMethod.DeclaringSyntaxReferences)
            {
                var syntax = syntaxReference.GetSyntax();
                var semanticModel = _compilation.GetSemanticModel(syntax.SyntaxTree);
                var operation = syntax switch
                {
                    MethodDeclarationSyntax methodDeclaration => methodDeclaration.Body is not null
                        ? semanticModel.GetOperation(methodDeclaration.Body)
                        : methodDeclaration.ExpressionBody is not null
                            ? semanticModel.GetOperation(methodDeclaration.ExpressionBody.Expression)
                            : null,
                    LocalFunctionStatementSyntax localFunction => localFunction.Body is not null
                        ? semanticModel.GetOperation(localFunction.Body)
                        : localFunction.ExpressionBody is not null
                            ? semanticModel.GetOperation(localFunction.ExpressionBody.Expression)
                            : null,
                    _ => null
                };

                if (operation is IBlockOperation block)
                    return block.Operations;

                if (operation is not null)
                    return ImmutableArray.Create(operation);
            }

            throw CreateUnsupportedBuilderCall(
                invocation,
                $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must be source-authored with an analyzable body in component '{_snapshot.Descriptor.FullName}'.");
        }

        private void ExecuteWithBuilderScope(
            ImmutableHashSet<IParameterSymbol> builderParameters,
            Action action)
        {
            var previousBuilderParameters = _builderParameters;
            var previousBuilderAliases = _builderAliases.ToArray();

            _builderParameters = builderParameters;
            _builderAliases.Clear();

            try
            {
                action();
            }
            finally
            {
                _builderParameters = previousBuilderParameters;
                _builderAliases.Clear();
                foreach (var alias in previousBuilderAliases)
                    _builderAliases.Add(alias);
            }
        }

        private bool TryResolveSlotOutlet(IOperation operation, out string slotName)
        {
            slotName = string.Empty;
            if (Unwrap(operation) is not IPropertyReferenceOperation propertyReference)
                return false;

            if (!IsCurrentComponentMember(propertyReference.Property, propertyReference.Instance))
                return false;

            if (!IsRenderFragment(propertyReference.Property.Type))
                return false;

            slotName = string.Equals(propertyReference.Property.Name, "ChildContent", StringComparison.Ordinal)
                ? "default"
                : ToLowerCamelCase(propertyReference.Property.Name);
            return true;
        }

        private bool IsRenderFragment(ITypeSymbol typeSymbol)
            => typeSymbol is INamedTypeSymbol namedType &&
               ((_symbols.RenderFragment is not null && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _symbols.RenderFragment)) ||
                (_symbols.RenderFragmentOfT is not null && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _symbols.RenderFragmentOfT)));

        private bool IsCurrentComponentMethod(IMethodSymbol method, IOperation? instance)
        {
            for (var current = _snapshot.ComponentSymbol; current is not null; current = current.BaseType)
            {
                if (SymbolEqualityComparer.Default.Equals(method.ContainingType, current))
                    return instance is null || Unwrap(instance) is IInstanceReferenceOperation;
            }

            return false;
        }

        private static bool ContainsRenderTreeBuilderParameter(IMethodSymbol method)
            => method.Parameters.Any(static parameter => IsRenderTreeBuilderType(parameter.Type));

        private static bool IsRenderTreeBuilderMethod(IMethodSymbol method)
            => IsRenderTreeBuilderType(method.ContainingType);

        private static bool IsRenderTreeBuilderType(ITypeSymbol? typeSymbol)
            => string.Equals(
                typeSymbol?.ToDisplayString(),
                "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder",
                StringComparison.Ordinal);

        private bool IsCurrentComponentMember(ISymbol symbol, IOperation? instance)
        {
            for (var current = _snapshot.ComponentSymbol; current is not null; current = current.BaseType)
            {
                if (SymbolEqualityComparer.Default.Equals(symbol.ContainingType, current))
                    return instance is null || Unwrap(instance) is IInstanceReferenceOperation;
            }

            return false;
        }

        private OpenNodeBuilder GetRequiredOpenNodeBuilder(IInvocationOperation invocation)
        {
            if (_openFrames.Count == 0)
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered '{invocation.TargetMethod.Name}' without an open element or component frame in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (_openFrames.Peek() is not OpenNodeBuilder currentNode)
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree encountered '{invocation.TargetMethod.Name}' while the current open frame is {_openFrames.Peek().Describe()} in component '{_snapshot.Descriptor.FullName}'.");
            }

            return currentNode;
        }

        private ComponentBuilder GetRequiredOpenComponentBuilder(IInvocationOperation invocation)
        {
            var currentNode = GetRequiredOpenNodeBuilder(invocation);
            if (currentNode is ComponentBuilder componentBuilder)
                return componentBuilder;

            throw CreateStructuralIssue(
                invocation,
                $"BuildRenderTree encountered '{invocation.TargetMethod.Name}' while the current open node is {currentNode.Describe()} in component '{_snapshot.Descriptor.FullName}'.");
        }

        private bool TryGetNearestOpenNodeBuilder(out OpenNodeBuilder currentNode)
        {
            foreach (var frame in _openFrames)
            {
                if (frame is OpenNodeBuilder nodeBuilder)
                {
                    currentNode = nodeBuilder;
                    return true;
                }
            }

            currentNode = default!;
            return false;
        }

        private static IOperation? GetInvocationArgument(IInvocationOperation invocation, int index)
        {
            if (invocation.Arguments.Length <= index)
                return null;

            return Unwrap(invocation.Arguments[index].Value);
        }

        private static string? GetConstantStringArgument(IInvocationOperation invocation, int index)
            => TryGetConstantString(GetInvocationArgument(invocation, index));

        private static string? TryGetConstantString(IOperation? operation)
        {
            var current = Unwrap(operation);
            if (current?.ConstantValue.HasValue == true &&
                current.ConstantValue.Value is string text)
                return text;

            return null;
        }

        private static bool IsConstantNull(IOperation? operation)
        {
            var current = Unwrap(operation);
            return current?.ConstantValue.HasValue == true &&
                   current.ConstantValue.Value is null;
        }

        private static IOperation? Unwrap(IOperation? operation)
            => RazorVueOperationNormalizer.Unwrap(operation);

        private bool ShouldOmitElementAttribute(OpenNodeBuilder currentNode, IOperation? value)
        {
            if (currentNode is not ElementBuilder)
                return false;

            if (value is null)
                return false;

            var current = Unwrap(value);
            if (current is null)
                return false;

            if (IsConstantNull(current))
                return true;

            return current.ConstantValue.HasValue &&
                   current.ConstantValue.Value is bool boolValue &&
                   !boolValue;
        }

        private bool TryHandleComponentSlotValue(
            OpenNodeBuilder currentNode,
            string name,
            IOperation? value,
            IInvocationOperation invocation)
        {
            if (currentNode is not ComponentBuilder)
                return false;

            if (!TryParseSlotTemplate(value, out var slotTemplate))
                return false;

            if (string.Equals(name, "ChildContent", StringComparison.Ordinal) &&
                string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
            {
                foreach (var child in slotTemplate.Children.Children)
                    currentNode.AddChild(child);
                return true;
            }

            currentNode.AddSlotTemplate(new RazorVueComponentSlotTemplateNode(
                PublicName: name,
                SlotName: string.Equals(name, "ChildContent", StringComparison.Ordinal)
                    ? "default"
                    : ToLowerCamelCase(name),
                ParameterName: slotTemplate.ParameterName,
                ParameterSymbol: slotTemplate.ParameterSymbol,
                Children: slotTemplate.Children,
                Origins: CreateOrigins(invocation, RazorVueOriginKind.Template)));
            return true;
        }

        private bool TryParseChildContent(IOperation? operation, out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (!TryParseSlotTemplate(operation, out var slotTemplate))
                return false;

            if (!string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
                return false;

            fragment = slotTemplate.Children;
            return true;
        }

        private bool TryParseAddContentRenderFragment(
            IInvocationOperation invocation,
            IOperation value,
            out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (!IsRenderFragmentAddContent(invocation))
                return false;

            if (invocation.Arguments.Length != 2)
                return false;

            return TryParseChildContent(value, out fragment);
        }

        private bool TryParseSlotTemplate(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (!TryGetAnonymousFunction(operation, out var anonymousFunction))
                return false;

            if (TryParseUntypedSlotTemplate(anonymousFunction, out slotTemplate))
                return true;

            if (TryParseTypedSlotTemplate(anonymousFunction, out slotTemplate))
                return true;

            return false;
        }

        private bool TryParseUntypedSlotTemplate(
            IAnonymousFunctionOperation anonymousFunction,
            out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (!TryGetSingleBuilderParameter(anonymousFunction, out _))
                return false;

            slotTemplate = new ParsedSlotTemplate(
                ParameterName: null,
                ParameterSymbol: null,
                Children: ParseAnonymousFunctionBody(anonymousFunction));
            return true;
        }

        private bool TryParseTypedSlotTemplate(
            IAnonymousFunctionOperation anonymousFunction,
            out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (anonymousFunction.Symbol.Parameters.Length != 1)
                return false;

            var slotContextParameter = anonymousFunction.Symbol.Parameters[0];
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

            if (!TryGetAnonymousFunction(returnedBuilderFactory, out var builderAnonymousFunction))
                return false;

            if (!TryGetSingleBuilderParameter(builderAnonymousFunction, out _))
                return false;

            slotTemplate = new ParsedSlotTemplate(
                ParameterName: slotContextParameter.Name,
                ParameterSymbol: slotContextParameter,
                Children: ParseAnonymousFunctionBody(builderAnonymousFunction));
            return true;
        }

        private RazorVueRenderFragment ParseAnonymousFunctionBody(IAnonymousFunctionOperation anonymousFunction)
        {
            if (!TryGetBuilderParameters(anonymousFunction, out var builderParameters))
                return RazorVueRenderFragment.Empty;

            var body = anonymousFunction.Body;
            if (body is null)
                return RazorVueRenderFragment.Empty;

            if (body is IBlockOperation block)
                return new Parser(_snapshot, _compilation, _symbols, builderParameters, activeRenderHelperMethods: _activeRenderHelperMethods).Parse(block.Operations);

            return new Parser(_snapshot, _compilation, _symbols, builderParameters, activeRenderHelperMethods: _activeRenderHelperMethods).Parse([body]);
        }

        private static bool TryGetAnonymousFunction(
            IOperation? operation,
            out IAnonymousFunctionOperation anonymousFunction)
        {
            anonymousFunction = default!;
            var current = UnwrapDelegateCarrier(operation);
            switch (current)
            {
                case IAnonymousFunctionOperation directAnonymousFunction:
                    anonymousFunction = directAnonymousFunction;
                    return true;
                case IDelegateCreationOperation delegateCreation when UnwrapDelegateCarrier(delegateCreation.Target) is IAnonymousFunctionOperation targetAnonymousFunction:
                    anonymousFunction = targetAnonymousFunction;
                    return true;
                default:
                    return false;
            }
        }

        private static IOperation? UnwrapDelegateCarrier(IOperation? operation)
        {
            var current = Unwrap(operation);
            while (current is IConversionOperation conversion)
                current = Unwrap(conversion.Operand);
            return current;
        }

        private static bool TryGetBuilderParameters(
            IAnonymousFunctionOperation anonymousFunction,
            out ImmutableHashSet<IParameterSymbol> builderParameters)
        {
            builderParameters = anonymousFunction.Symbol.Parameters
                .Where(static parameter =>
                    string.Equals(parameter.Name, "builder", StringComparison.Ordinal) ||
                    string.Equals(parameter.Type.Name, "RenderTreeBuilder", StringComparison.Ordinal))
                .ToImmutableHashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
            return builderParameters.Count > 0;
        }

        private static bool TryGetSingleBuilderParameter(
            IAnonymousFunctionOperation anonymousFunction,
            out IParameterSymbol builderParameter)
        {
            builderParameter = default!;
            if (!TryGetBuilderParameters(anonymousFunction, out var builderParameters) ||
                builderParameters.Count != 1)
            {
                return false;
            }

            builderParameter = builderParameters.Single();
            return true;
        }

        private static bool TryGetSingleReturnedValue(IBlockOperation block, out IOperation? returnedValue)
        {
            returnedValue = null;
            if (block.Operations.Length != 1 ||
                block.Operations[0] is not IReturnOperation returnOperation)
            {
                return false;
            }

            returnedValue = returnOperation.ReturnedValue;
            return returnedValue is not null;
        }

        private bool IsRenderFragmentAddContent(IInvocationOperation invocation)
            => invocation.Arguments.Length >= 2 &&
               IsRenderFragmentType(invocation.TargetMethod.Parameters[1].Type);

        private bool IsMarkupStringAddContent(IInvocationOperation invocation)
            => invocation.Arguments.Length >= 2 &&
               IsMarkupStringType(invocation.TargetMethod.Parameters[1].Type);

        private bool IsRenderFragmentType(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null)
                return false;

            if (typeSymbol is INamedTypeSymbol namedType &&
                namedType.IsGenericType &&
                namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                typeSymbol = namedType.TypeArguments[0];
            }

            return IsRenderFragment(typeSymbol);
        }

        private static bool IsMarkupStringType(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null)
                return false;

            if (typeSymbol is INamedTypeSymbol namedType &&
                namedType.IsGenericType &&
                namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                typeSymbol = namedType.TypeArguments[0];
            }

            return string.Equals(
                typeSymbol.ToDisplayString(),
                "Microsoft.AspNetCore.Components.MarkupString",
                StringComparison.Ordinal);
        }

        private RazorVueCompilationIssueException CreateStructuralIssue(
            IOperation operation,
            string message)
            => CreateStructuralIssue(
                operation.Syntax is null
                    ? _snapshot.Origins.FirstOrDefault() is { } origin ? ImmutableArray.Create(origin) : ImmutableArray<RazorVueSourceOrigin>.Empty
                    : CreateOrigins(operation, RazorVueOriginKind.Template),
                message);

        private RazorVueCompilationIssueException CreateStructuralIssueForUnclosedFrames()
        {
            var current = _openFrames.Peek();
            return CreateStructuralIssue(
                current.Origins,
                $"BuildRenderTree ended with {_openFrames.Count} unclosed frame(s); innermost open frame is {current.Describe()} in component '{_snapshot.Descriptor.FullName}'.");
        }

        private RazorVueCompilationIssueException CreateUnsupportedBuilderCall(
            IInvocationOperation invocation,
            string message)
            => CreateStructuralIssue(invocation, message);

        private static string GetBuilderCallDisplayName(IInvocationOperation invocation)
            => invocation.TargetMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        private sealed class RegionScope(ImmutableArray<RazorVueSourceOrigin> origins)
            : OpenFrame(origins)
        {
            public override string Describe()
                => "region";
        }

        private RazorVueCompilationIssueException CreateStructuralIssue(
            ImmutableArray<RazorVueSourceOrigin> origins,
            string message)
        {
            var issue = new RazorVueCompilationIssue(
                RazorVueIssueCode.CanonicalizationFailed,
                RazorVueIssueSeverity.Error,
                message,
                ImmutableArray<string>.Empty);
            return new RazorVueCompilationIssueException(
                issue,
                _snapshot.Descriptor.FullName,
                origins.IsDefaultOrEmpty ? _snapshot.Origins.FirstOrDefault() : origins[0]);
        }

        private static ImmutableArray<RazorVueSourceOrigin> CreateOrigins(IOperation operation, RazorVueOriginKind originKind)
            => operation.Syntax is null
                ? ImmutableArray<RazorVueSourceOrigin>.Empty
                : ImmutableArray.Create(RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), originKind));

        private static string ToLowerCamelCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length == 1)
                return char.ToLowerInvariant(value[0]).ToString();

            if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
                return value;

            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        private readonly record struct ParsedSlotTemplate(
            string? ParameterName,
            IParameterSymbol? ParameterSymbol,
            RazorVueRenderFragment Children);

    }

    private abstract class OpenFrame
    {
        protected OpenFrame(ImmutableArray<RazorVueSourceOrigin> origins)
        {
            Origins = origins;
        }

        public ImmutableArray<RazorVueSourceOrigin> Origins { get; }

        public abstract string Describe();
    }

    private abstract class OpenNodeBuilder : OpenFrame
    {
        private readonly List<RazorVueAttributeEntry> _attributes = [];
        private readonly List<RazorVueComponentSlotTemplateNode> _slotTemplates = [];
        private readonly List<RazorVueRenderNode> _children = [];

        protected OpenNodeBuilder(ImmutableArray<RazorVueSourceOrigin> origins)
            : base(origins)
        {
        }

        public void AddAttribute(RazorVueAttributeEntry attribute)
            => _attributes.Add(attribute);

        public void AddSlotTemplate(RazorVueComponentSlotTemplateNode slotTemplate)
            => _slotTemplates.Add(slotTemplate);

        public void AddChild(RazorVueRenderNode child)
            => _children.Add(child);

        protected ImmutableArray<RazorVueAttributeEntry> BuildAttributes()
            => _attributes.ToImmutableArray();

        protected ImmutableArray<RazorVueComponentSlotTemplateNode> BuildSlotTemplates()
            => _slotTemplates.ToImmutableArray();

        protected RazorVueRenderFragment BuildChildren()
            => new(_children.ToImmutableArray());

        public abstract override string Describe();

        public abstract RazorVueRenderNode Build();
    }

    private sealed class ElementBuilder(string tagName, ImmutableArray<RazorVueSourceOrigin> origins)
        : OpenNodeBuilder(origins)
    {
        public override string Describe()
            => $"element <{tagName}>";

        public override RazorVueRenderNode Build()
            => new RazorVueElementNode(tagName, BuildAttributes(), BuildChildren(), Origins);
    }

    private sealed class ComponentBuilder(string componentName, string componentFullName, string resolutionName, ImmutableArray<RazorVueSourceOrigin> origins)
        : OpenNodeBuilder(origins)
    {
        public override string Describe()
            => $"component '{componentFullName}'";

        public override RazorVueRenderNode Build()
            => new RazorVueComponentNode(componentName, componentFullName, resolutionName, BuildAttributes(), BuildSlotTemplates(), BuildChildren(), Origins);
    }
}
