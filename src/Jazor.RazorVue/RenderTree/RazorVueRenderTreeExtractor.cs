using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

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
            {
                if (RazorVueImperativeRenderSegmentationPlanner.TryPlanLocalSegments(block.Operations, out var segments))
                {
                    return new Parser(snapshot, context.Compilation, context.Symbols, builderParameters, allowTemplateScopedLocals: true)
                        .ParseWithImperativeSegments(block.Operations, segments);
                }

                if (RazorVueImperativeRenderPromotionAnalyzer.ShouldPromoteBody(block.Operations))
                {
                    return CreateImperativeBodyFragment(
                        block.Operations,
                        RazorVueImperativeRenderPromotionAnalyzer.ClassifyBodyKind(block.Operations),
                        builderParameters);
                }

                return new Parser(snapshot, context.Compilation, context.Symbols, builderParameters, allowTemplateScopedLocals: true).Parse(block.Operations);
            }

            if (operation is not null)
            {
                if (RazorVueImperativeRenderPromotionAnalyzer.ShouldPromoteBody([operation]))
                {
                    return CreateImperativeBodyFragment(
                        [operation],
                        RazorVueImperativeRenderPromotionAnalyzer.ClassifyBodyKind([operation]),
                        builderParameters);
                }

                return new Parser(snapshot, context.Compilation, context.Symbols, builderParameters, allowTemplateScopedLocals: true).Parse([operation]);
            }
        }

        return RazorVueRenderFragment.Empty;
    }

    private static RazorVueRenderFragment CreateImperativeBodyFragment(
        IReadOnlyList<IOperation> operations,
        RazorVueImperativeBlockKind kind,
        ImmutableHashSet<IParameterSymbol> builderParameters)
    {
        var visibleLocals = CollectVisibleLocals(operations);
        var visibleParameters = CollectVisibleParameters(operations, builderParameters);

        return new RazorVueRenderFragment(
        [
            new RazorVueImperativeBlockNode(
                [.. operations],
                kind,
                visibleLocals,
                visibleParameters,
                CreateOriginsStatic(operations, RazorVueOriginKind.Template))
        ]);
    }

    private static ImmutableArray<ILocalSymbol> CollectVisibleLocals(IEnumerable<IOperation> operations)
    {
        var builder = ImmutableArray.CreateBuilder<ILocalSymbol>();
        var seen = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);

        foreach (var operation in operations)
        {
            foreach (var candidate in EnumerateOperationAndDescendants(operation))
            {
                switch (candidate)
                {
                    case IVariableDeclarationGroupOperation declarationGroup:
                        foreach (var declaration in declarationGroup.Declarations)
                        {
                            foreach (var declarator in declaration.Declarators)
                            {
                                if (seen.Add(declarator.Symbol))
                                    builder.Add(declarator.Symbol);
                            }
                        }

                        break;
                    case IForEachLoopOperation foreachLoop:
                        foreach (var local in foreachLoop.Locals)
                        {
                            if (seen.Add(local))
                                builder.Add(local);
                        }

                        break;
                    case IForLoopOperation forLoop:
                        foreach (var local in forLoop.Locals)
                        {
                            if (seen.Add(local))
                                builder.Add(local);
                        }

                        break;
                    case IUsingDeclarationOperation usingDeclaration:
                        if (usingDeclaration.DeclarationGroup is null)
                            break;

                        foreach (var declaration in usingDeclaration.DeclarationGroup.Declarations)
                        {
                            foreach (var declarator in declaration.Declarators)
                            {
                                if (seen.Add(declarator.Symbol))
                                    builder.Add(declarator.Symbol);
                            }
                        }

                        break;
                    case ILocalReferenceOperation localReference:
                        if (seen.Add(localReference.Local))
                            builder.Add(localReference.Local);

                        break;
                }
            }
        }

        return builder.ToImmutable();
    }

    private static IEnumerable<IOperation> EnumerateOperationAndDescendants(IOperation operation)
    {
        yield return operation;
        foreach (var child in operation.ChildOperations)
        {
            if (child is null)
                continue;

            foreach (var nested in EnumerateOperationAndDescendants(child))
                yield return nested;
        }
    }

    private static ImmutableArray<IParameterSymbol> CollectVisibleParameters(
        IEnumerable<IOperation> operations,
        ImmutableHashSet<IParameterSymbol> fallbackParameters)
    {
        var builder = ImmutableArray.CreateBuilder<IParameterSymbol>();
        var seen = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);

        foreach (var operation in operations)
        {
            foreach (var parameterReference in EnumerateOperationAndDescendants(operation).OfType<IParameterReferenceOperation>())
            {
                if (seen.Add(parameterReference.Parameter))
                    builder.Add(parameterReference.Parameter);
            }
        }

        if (builder.Count > 0)
            return builder.ToImmutable();

        return fallbackParameters.ToImmutableArray();
    }

    private static ImmutableArray<RazorVueSourceOrigin> CreateOriginsStatic(
        IEnumerable<IOperation> operations,
        RazorVueOriginKind originKind)
    {
        var builder = ImmutableArray.CreateBuilder<RazorVueSourceOrigin>();
        foreach (var operation in operations)
        {
            if (operation.Syntax is null)
                continue;

            builder.Add(RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), originKind));
        }

        return builder.ToImmutable();
    }

    private readonly record struct ParsedSlotTemplate(
        string? ParameterName,
        IParameterSymbol? ParameterSymbol,
        RazorVueRenderFragment Children,
        ImmutableArray<RenderHelperValueBinding> CapturedBindings)
    {
        public static ParsedSlotTemplate Create(
            string? parameterName,
            IParameterSymbol? parameterSymbol,
            RazorVueRenderFragment children)
            => new(
                parameterName,
                parameterSymbol,
                children,
                ImmutableArray<RenderHelperValueBinding>.Empty);

        public ParsedSlotTemplate PrependCapturedBindings(ImmutableArray<RenderHelperValueBinding> capturedBindings)
        {
            if (capturedBindings.IsDefaultOrEmpty)
                return this;

            if (CapturedBindings.IsDefaultOrEmpty)
                return new ParsedSlotTemplate(ParameterName, ParameterSymbol, Children, capturedBindings);

            var builder = ImmutableArray.CreateBuilder<RenderHelperValueBinding>(capturedBindings.Length + CapturedBindings.Length);
            builder.AddRange(capturedBindings);
            builder.AddRange(CapturedBindings);
            return new ParsedSlotTemplate(ParameterName, ParameterSymbol, Children, builder.MoveToImmutable());
        }
    }

    private readonly record struct RenderFragmentLocalCarrier(
        ILocalSymbol LocalSymbol,
        ParsedSlotTemplate Template);

    private readonly record struct RenderFragmentMemberCarrier(
        ISymbol MemberSymbol,
        ParsedSlotTemplate Template);

    private readonly record struct RenderFragmentFactoryCarrier(
        IMethodSymbol MethodSymbol,
        ParsedSlotTemplate Template);

    private readonly record struct RenderHelperValueBinding(
        IParameterSymbol ParameterSymbol,
        IOperation Initializer);

    private sealed class Parser(
        RazorVueSemanticSnapshot snapshot,
        Compilation compilation,
        RazorVueCompilationSymbols symbols,
        ImmutableHashSet<IParameterSymbol> builderParameters,
        IEnumerable<ILocalSymbol>? builderAliases = null,
        IEnumerable<IMethodSymbol>? activeRenderHelperMethods = null,
        IEnumerable<ISymbol>? activeRenderFragmentMembers = null,
        IEnumerable<IMethodSymbol>? activeRenderFragmentFactories = null,
        IEnumerable<RenderFragmentLocalCarrier>? localRenderFragmentCarriers = null,
        IEnumerable<RenderFragmentMemberCarrier>? memberRenderFragmentCarriers = null,
        IEnumerable<RenderFragmentFactoryCarrier>? factoryRenderFragmentCarriers = null,
        IEnumerable<ILocalSymbol>? accessibleTemplateLocals = null,
        IEnumerable<IParameterSymbol>? accessibleTemplateParameters = null,
        bool allowTemplateScopedLocals = false)
    {
        private readonly RazorVueSemanticSnapshot _snapshot = snapshot;
        private readonly Compilation _compilation = compilation;
        private readonly RazorVueCompilationSymbols _symbols = symbols;
        private ImmutableHashSet<IParameterSymbol> _builderParameters = builderParameters;
        private readonly HashSet<ILocalSymbol> _builderAliases = builderAliases is null
                ? [with(SymbolEqualityComparer.Default)]
                : new HashSet<ILocalSymbol>(builderAliases, SymbolEqualityComparer.Default);
        private readonly HashSet<IMethodSymbol> _activeRenderHelperMethods = activeRenderHelperMethods is null
                ? [with(SymbolEqualityComparer.Default)]
                : new HashSet<IMethodSymbol>(activeRenderHelperMethods, SymbolEqualityComparer.Default);
        private readonly HashSet<ISymbol> _activeRenderFragmentMembers = activeRenderFragmentMembers is null
                ? [with(SymbolEqualityComparer.Default)]
                : new HashSet<ISymbol>(activeRenderFragmentMembers, SymbolEqualityComparer.Default);
        private readonly HashSet<IMethodSymbol> _activeRenderFragmentFactories = activeRenderFragmentFactories is null
                ? [with(SymbolEqualityComparer.Default)]
                : new HashSet<IMethodSymbol>(activeRenderFragmentFactories, SymbolEqualityComparer.Default);
        private readonly Dictionary<ILocalSymbol, ParsedSlotTemplate> _localRenderFragmentCarriers = localRenderFragmentCarriers is null
                ? new Dictionary<ILocalSymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default)
                : CreateLocalRenderFragmentCarrierDictionary(localRenderFragmentCarriers);
        private readonly Dictionary<ILocalSymbol, IOperation> _localStaticMarkupCarriers =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<ISymbol, ParsedSlotTemplate> _memberRenderFragmentCarriers = memberRenderFragmentCarriers is null
                ? new Dictionary<ISymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default)
                : CreateMemberRenderFragmentCarrierDictionary(memberRenderFragmentCarriers);
        private readonly Dictionary<IMethodSymbol, ParsedSlotTemplate> _factoryRenderFragmentCarriers = factoryRenderFragmentCarriers is null
                ? new Dictionary<IMethodSymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default)
                : CreateFactoryRenderFragmentCarrierDictionary(factoryRenderFragmentCarriers);
        private readonly HashSet<ILocalSymbol> _accessibleTemplateLocals = accessibleTemplateLocals is null
                ? [with(SymbolEqualityComparer.Default)]
                : new HashSet<ILocalSymbol>(accessibleTemplateLocals, SymbolEqualityComparer.Default);
        private readonly HashSet<IParameterSymbol> _accessibleTemplateParameters = accessibleTemplateParameters is null
                ? [with(SymbolEqualityComparer.Default)]
                : new HashSet<IParameterSymbol>(accessibleTemplateParameters, SymbolEqualityComparer.Default);
        private readonly Dictionary<ILocalSymbol, PendingRenderFragmentLocalCarrierDeclaration> _pendingRenderFragmentLocalCarriers =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<ILocalSymbol, PendingTemplateScopedDeclaration> _pendingTemplateScopedDeclarations =
            new(SymbolEqualityComparer.Default);
        private readonly Dictionary<string, IOperation> _literalStringOperationCache = new(StringComparer.Ordinal);
        private readonly List<RazorVueRenderNode> _rootChildren = [];
        private readonly Stack<OpenFrame> _openFrames = new();
        private readonly bool _allowTemplateScopedLocals = allowTemplateScopedLocals;

        public RazorVueRenderFragment Parse(IEnumerable<IOperation> operations)
        {
            foreach (var operation in operations)
                ParseOperation(operation);

            EnsureNoPendingImmediateAssignmentDeclarations();

            if (_openFrames.Count > 0)
                throw CreateStructuralIssueForUnclosedFrames();

            return new RazorVueRenderFragment(_rootChildren.ToImmutableArray());
        }

        public RazorVueRenderFragment ParseWithImperativeSegments(
            IReadOnlyList<IOperation> operations,
            ImmutableArray<RazorVueImperativeRenderSegmentationPlanner.PlannedSegment> segments)
        {
            var nextOperationIndex = 0;
            foreach (var segment in segments)
            {
                for (; nextOperationIndex < segment.StartIndex; nextOperationIndex++)
                    ParseOperation(operations[nextOperationIndex]);

                AddImperativeSegment(operations, segment);
                nextOperationIndex = segment.EndExclusive;
            }

            for (; nextOperationIndex < operations.Count; nextOperationIndex++)
                ParseOperation(operations[nextOperationIndex]);

            EnsureNoPendingImmediateAssignmentDeclarations();

            if (_openFrames.Count > 0)
                throw CreateStructuralIssueForUnclosedFrames();

            return new RazorVueRenderFragment(_rootChildren.ToImmutableArray());
        }

        private void ParseOperation(IOperation? operation)
        {
            var current = Unwrap(operation);
            if (current is null)
                return;

            if (HasPendingImmediateAssignmentDeclarations() &&
                !IsPendingImmediateAssignment(current))
            {
                ThrowPendingImmediateAssignmentRequiresImmediateAssignment(current);
            }

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
                        ParseNestedBranch(foreachLoop.Body, foreachLoop.Locals),
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
                    ParseVariableDeclarationGroup(variableDeclarationGroup);
                    break;
                case IInvocationOperation invocation:
                    ParseOperationExpression(invocation);
                    break;
                case ILocalFunctionOperation:
                case IEmptyOperation:
                    break;
                case IReturnOperation { IsImplicit: true }:
                    break;
                case IReturnOperation returnOperation:
                    throw CreateStructuralIssue(
                        returnOperation,
                        $"BuildRenderTree does not support 'return' statements during RazorVue template extraction in component '{_snapshot.Descriptor.FullName}'. Move this control flow outside the render body or use the Razor IR frontend.");
                case ILoopOperation loop:
                    throw CreateStructuralIssue(
                        loop,
                        $"BuildRenderTree does not support loop statement '{GetOperationDisplay(loop)}' in component '{_snapshot.Descriptor.FullName}'. Only canonicalizable 'for' and 'foreach' loops are supported.");
                default:
                    throw CreateStructuralIssue(
                        current,
                        $"BuildRenderTree does not support statement '{GetOperationDisplay(current)}' ({current.Kind}) in component '{_snapshot.Descriptor.FullName}'.");
            }
        }

        private void AddImperativeSegment(
            IReadOnlyList<IOperation> operations,
            RazorVueImperativeRenderSegmentationPlanner.PlannedSegment segment)
        {
            var segmentOperations = operations
                .Skip(segment.StartIndex)
                .Take(segment.EndExclusive - segment.StartIndex)
                .ToImmutableArray();

            AddNode(new RazorVueImperativeBlockNode(
                segmentOperations,
                segment.Kind,
                CollectVisibleLocals(segmentOperations),
                CollectVisibleParameters(segmentOperations, _builderParameters),
                CreateOriginsStatic(segmentOperations, RazorVueOriginKind.Template)));
        }

        private void ParseExpressionStatement(IExpressionStatementOperation expressionStatement)
        {
            var statementOperation = Unwrap(expressionStatement.Operation);
            if (statementOperation is ISimpleAssignmentOperation assignment)
            {
                if (TryRegisterBuilderAliasAssignment(assignment))
                    return;

                if (TryCompletePendingRenderFragmentLocalCarrier(assignment))
                    return;

                if (TryCompletePendingTemplateScopedDeclaration(assignment))
                    return;

                throw CreateStructuralIssue(
                    assignment,
                    $"BuildRenderTree does not support assignment statement '{GetOperationDisplay(assignment)}' in component '{_snapshot.Descriptor.FullName}'. Only direct RenderTreeBuilder local alias assignments and the supported immediate-assignment local declaration patterns are allowed.");
            }

            if (statementOperation is not IInvocationOperation invocation)
            {
                throw CreateStructuralIssue(
                    statementOperation ?? expressionStatement,
                    $"BuildRenderTree does not support statement '{GetOperationDisplay(statementOperation ?? expressionStatement)}' in component '{_snapshot.Descriptor.FullName}'.");
            }

            ParseOperationExpression(invocation);
        }

        private void ParseOperationExpression(IInvocationOperation invocation)
        {
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

                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree does not support standalone invocation '{GetBuilderCallDisplayName(invocation)}' in component '{_snapshot.Descriptor.FullName}'. Only RenderTreeBuilder calls and supported render helpers may participate in RazorVue template extraction.");
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
                case "SetKey":
                    SetKey(invocation);
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
                componentType,
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
                currentNode.AddImplicitDefaultSlotAssignment(new RazorVueImplicitDefaultSlotAssignmentNode(
                    childContent,
                    CreateOrigins(invocation, RazorVueOriginKind.Template)));
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
                currentNode.AddImplicitDefaultSlotAssignment(new RazorVueImplicitDefaultSlotAssignmentNode(
                    childContent,
                    CreateOrigins(invocation, RazorVueOriginKind.Template)));
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

        private void SetKey(IInvocationOperation invocation)
        {
            var currentNode = GetRequiredOpenNodeBuilder(invocation);
            var key = GetInvocationArgument(invocation, 0);
            currentNode.SetKey(key, CreateOrigins(invocation, RazorVueOriginKind.Template));
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

            if (TryParseAddContentFragmentFactory(invocation, value, out var factoryFragment))
            {
                foreach (var child in factoryFragment.Children)
                    AddNode(child);
                return;
            }

            if (TryParseTypedAddContentTemplate(invocation, value, out var typedFragment))
            {
                foreach (var child in typedFragment.Children)
                    AddNode(child);
                return;
            }

            if (IsMarkupStringAddContent(invocation))
            {
                if (TryGetStaticMarkupString(value) is string staticMarkup)
                {
                    AddStaticMarkupContent(invocation, staticMarkup);
                    return;
                }

                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' uses MarkupString content that is not compile-time provable static markup in component '{_snapshot.Descriptor.FullName}'. RazorVue only supports static MarkupString literals that can be canonicalized into a safe render subtree.");
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
            var value = GetInvocationArgument(invocation, 1);
            if (value is null || IsConstantNull(value))
                return;

            if (TryGetStaticMarkupString(value) is not string markup)
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' uses AddMarkupContent(...) content that is not compile-time provable static markup in component '{_snapshot.Descriptor.FullName}'. RazorVue only supports static markup literals/carriers that can be canonicalized into a safe render subtree.");
            }

            if (string.IsNullOrEmpty(markup))
                return;

            AddStaticMarkupContent(invocation, markup);
        }

        private void AddStaticMarkupContent(IInvocationOperation invocation, string markup)
        {
            if (string.IsNullOrEmpty(markup))
                return;

            var nodes = RazorVueStaticMarkupParser.Parse(
                markup,
                CreateOrigins(invocation, RazorVueOriginKind.Template),
                new RazorVueStaticMarkupParser.Dependencies(
                    CreateLiteralStringOperation,
                    detail => CreateUnsupportedBuilderCall(
                        invocation,
                        $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' {detail} in component '{_snapshot.Descriptor.FullName}'.")));
            foreach (var node in nodes)
                AddNode(node);
        }

        private string? TryGetStaticMarkupString(IOperation? operation)
            => RazorVueStaticMarkupValueHelper.TryGetStaticMarkupValue(
                operation,
                _compilation,
                TryGetLocalMarkupStringInitializer,
                TryGetPropertyMarkupStringInitializer,
                TryGetFieldMarkupStringInitializer);

        private RazorVueRenderFragment ParseNestedBranch(IOperation? operation)
        {
            var current = Unwrap(operation);
            if (current is null)
                return RazorVueRenderFragment.Empty;

            return current switch
            {
                IBlockOperation block => new Parser(
                    _snapshot,
                    _compilation,
                    _symbols,
                    _builderParameters,
                    _builderAliases,
                    _activeRenderHelperMethods,
                    _activeRenderFragmentMembers,
                    _activeRenderFragmentFactories,
                    GetLocalRenderFragmentCarrierSnapshot(),
                    GetMemberRenderFragmentCarrierSnapshot(),
                    GetFactoryRenderFragmentCarrierSnapshot(),
                    _accessibleTemplateLocals,
                    _accessibleTemplateParameters,
                    _allowTemplateScopedLocals).Parse(block.Operations),
                _ => new Parser(
                    _snapshot,
                    _compilation,
                    _symbols,
                    _builderParameters,
                    _builderAliases,
                    _activeRenderHelperMethods,
                    _activeRenderFragmentMembers,
                    _activeRenderFragmentFactories,
                    GetLocalRenderFragmentCarrierSnapshot(),
                    GetMemberRenderFragmentCarrierSnapshot(),
                    GetFactoryRenderFragmentCarrierSnapshot(),
                    _accessibleTemplateLocals,
                    _accessibleTemplateParameters,
                    _allowTemplateScopedLocals).Parse([current])
            };
        }

        private RazorVueRenderFragment ParseNestedBranch(
            IOperation? operation,
            IEnumerable<ILocalSymbol> additionalTemplateLocals)
        {
            var current = Unwrap(operation);
            if (current is null)
                return RazorVueRenderFragment.Empty;

            var mergedTemplateLocals = new HashSet<ILocalSymbol>(_accessibleTemplateLocals, SymbolEqualityComparer.Default);
            foreach (var local in additionalTemplateLocals)
                mergedTemplateLocals.Add(local);

            return current switch
            {
                IBlockOperation block => new Parser(
                    _snapshot,
                    _compilation,
                    _symbols,
                    _builderParameters,
                    _builderAliases,
                    _activeRenderHelperMethods,
                    _activeRenderFragmentMembers,
                    _activeRenderFragmentFactories,
                    GetLocalRenderFragmentCarrierSnapshot(),
                    GetMemberRenderFragmentCarrierSnapshot(),
                    GetFactoryRenderFragmentCarrierSnapshot(),
                    mergedTemplateLocals,
                    _accessibleTemplateParameters,
                    _allowTemplateScopedLocals).Parse(block.Operations),
                _ => new Parser(
                    _snapshot,
                    _compilation,
                    _symbols,
                    _builderParameters,
                    _builderAliases,
                    _activeRenderHelperMethods,
                    _activeRenderFragmentMembers,
                    _activeRenderFragmentFactories,
                    GetLocalRenderFragmentCarrierSnapshot(),
                    GetMemberRenderFragmentCarrierSnapshot(),
                    GetFactoryRenderFragmentCarrierSnapshot(),
                    mergedTemplateLocals,
                    _accessibleTemplateParameters,
                    _allowTemplateScopedLocals).Parse([current])
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
                ParseNestedBranch(loop.Body, loop.Locals),
                CreateOrigins(loop, RazorVueOriginKind.Template));
        }

        private void AddNode(RazorVueRenderNode node)
        {
            if (TryGetNearestOpenNodeBuilder(out var currentNode))
            {
                if (currentNode is ComponentBuilder)
                    currentNode.AddAmbientDefaultSlotChild(node);
                currentNode.AddChild(node);
            }
            else
            {
                _rootChildren.Add(node);
            }
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

            if (!TryGetSupportedRenderHelperSignature(
                    invocation.TargetMethod,
                    out var builderParameter,
                    out var extraParameters,
                    out var failureMessage))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    failureMessage);
            }

            if (!TryGetRenderHelperInvocationBindings(
                    invocation,
                    builderParameter,
                    out var builderArgument,
                    out var extraArgumentBindings,
                    out failureMessage))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    failureMessage);
            }

            if (!IsKnownBuilderReference(builderArgument.Value))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must receive the active RenderTreeBuilder parameter or a direct local alias in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (extraArgumentBindings.IsDefaultOrEmpty)
            {
                ParseRenderHelperBody(invocation, builderParameter);
                return true;
            }

            var fragment = ParseRenderHelperBodyAsScopedFragment(invocation, builderParameter, extraParameters, extraArgumentBindings);
            foreach (var child in fragment.Children)
                AddNode(child);

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

        private void ParseVariableDeclarationGroup(IVariableDeclarationGroupOperation declarationGroup)
        {
            foreach (var declaration in declarationGroup.Declarations)
            {
                foreach (var declarator in declaration.Declarators)
                {
                    if (TryRegisterBuilderAliasDeclaration(declarator, out var failureMessage))
                        continue;

                    if (TryRegisterRenderFragmentLocalCarrier(declarator, out failureMessage))
                        continue;

                    if (TryRegisterStaticMarkupLocalCarrier(declarator, out failureMessage))
                        continue;

                    if (TryRegisterTemplateScopedDeclaration(declarator, out failureMessage))
                        continue;

                    throw CreateStructuralIssue(
                        declarator,
                        failureMessage);
                }
            }
        }

        private bool TryRegisterBuilderAliasDeclaration(
            IVariableDeclaratorOperation declarator,
            out string failureMessage)
        {
            failureMessage =
                $"BuildRenderTree does not support local variable declaration '{GetOperationDisplay(declarator)}' in component '{_snapshot.Descriptor.FullName}'. Only direct RenderTreeBuilder local alias declarations are supported.";

            if (!IsRenderTreeBuilderType(declarator.Symbol.Type))
                return false;

            var value = declarator.Initializer?.Value;
            if (!IsKnownBuilderReference(value))
            {
                failureMessage =
                    $"BuildRenderTree local alias '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be initialized from the active RenderTreeBuilder parameter or a direct local alias. Other RenderTreeBuilder receivers cannot be tracked safely.";
                return false;
            }

            _builderAliases.Add(declarator.Symbol);
            return true;
        }

        private bool TryRegisterBuilderAliasAssignment(ISimpleAssignmentOperation assignment)
        {
            if (assignment.Target is not ILocalReferenceOperation localReference)
                return false;

            if (!IsRenderTreeBuilderType(localReference.Local.Type))
                return false;

            if (!IsKnownBuilderReference(assignment.Value))
                return false;

            _builderAliases.Add(localReference.Local);
            return true;
        }

        private bool TryRegisterRenderFragmentLocalCarrier(
            IVariableDeclaratorOperation declarator,
            out string failureMessage)
        {
            failureMessage =
                $"BuildRenderTree does not support local variable declaration '{GetOperationDisplay(declarator)}' in component '{_snapshot.Descriptor.FullName}'.";

            if (!IsRenderFragmentType(declarator.Symbol.Type))
                return false;

            if (declarator.Initializer?.Value is not { } initializer)
            {
                if (!_allowTemplateScopedLocals)
                {
                    failureMessage =
                        $"RazorVue RenderFragment local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' requires an analyzable initializer.";
                    return false;
                }

                _pendingRenderFragmentLocalCarriers[declarator.Symbol] =
                    new PendingRenderFragmentLocalCarrierDeclaration(declarator);
                return true;
            }

            if (!TryParseSlotTemplate(initializer, out var slotTemplate))
            {
                failureMessage =
                    $"RazorVue RenderFragment local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be initialized from an analyzable inline template, current-component RenderFragment member, or supported fragment factory.";
                return false;
            }

            _localRenderFragmentCarriers[declarator.Symbol] = slotTemplate;
            return true;
        }

        private bool TryRegisterStaticMarkupLocalCarrier(
            IVariableDeclaratorOperation declarator,
            out string failureMessage)
        {
            failureMessage =
                $"BuildRenderTree does not support local variable declaration '{GetOperationDisplay(declarator)}' in component '{_snapshot.Descriptor.FullName}'.";

            if (!RazorVueStaticMarkupValueHelper.IsMarkupStringType(declarator.Symbol.Type))
                return false;

            if (declarator.Initializer?.Value is not { } initializer)
            {
                failureMessage =
                    $"RazorVue MarkupString local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' requires a compile-time provable static markup initializer.";
                return true;
            }

            if (TryGetStaticMarkupString(initializer) is null)
            {
                failureMessage =
                    $"RazorVue MarkupString local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be initialized from compile-time provable static markup or a previously analyzable static MarkupString carrier.";
                return true;
            }

            _localStaticMarkupCarriers[declarator.Symbol] = initializer;
            return true;
        }

        private bool TryRegisterTemplateScopedDeclaration(
            IVariableDeclaratorOperation declarator,
            out string failureMessage)
        {
            failureMessage =
                $"BuildRenderTree does not support local variable declaration '{GetOperationDisplay(declarator)}' in component '{_snapshot.Descriptor.FullName}'.";

            if (!_allowTemplateScopedLocals)
                return false;

            if (IsRenderTreeBuilderType(declarator.Symbol.Type))
                return false;

            if (declarator.Initializer?.Value is not { } initializer)
            {
                if (IsRenderFragmentType(declarator.Symbol.Type))
                {
                    failureMessage =
                        $"RazorVue RenderFragment local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' requires an analyzable initializer.";
                    return false;
                }

                _pendingTemplateScopedDeclarations[declarator.Symbol] = new PendingTemplateScopedDeclaration(declarator);
                return true;
            }

            CommitTemplateScopedDeclaration(declarator, initializer);
            return true;
        }

        private bool TryCompletePendingTemplateScopedDeclaration(ISimpleAssignmentOperation assignment)
        {
            if (assignment.Target is not ILocalReferenceOperation localReference)
                return false;

            if (!_pendingTemplateScopedDeclarations.TryGetValue(localReference.Local, out var pendingDeclaration))
                return false;

            _pendingTemplateScopedDeclarations.Remove(localReference.Local);
            CommitTemplateScopedDeclaration(pendingDeclaration.Declarator, assignment.Value);
            return true;
        }

        private bool TryCompletePendingRenderFragmentLocalCarrier(ISimpleAssignmentOperation assignment)
        {
            if (assignment.Target is not ILocalReferenceOperation localReference)
                return false;

            if (!_pendingRenderFragmentLocalCarriers.TryGetValue(localReference.Local, out var pendingDeclaration))
                return false;

            _pendingRenderFragmentLocalCarriers.Remove(localReference.Local);
            if (!TryParseSlotTemplate(assignment.Value, out var slotTemplate))
            {
                throw CreateStructuralIssue(
                    assignment,
                    $"RazorVue RenderFragment local '{pendingDeclaration.Declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be initialized from an analyzable inline template, current-component RenderFragment member, or supported fragment factory.");
            }

            _localRenderFragmentCarriers[pendingDeclaration.Declarator.Symbol] = slotTemplate;
            return true;
        }

        private void CommitTemplateScopedDeclaration(
            IVariableDeclaratorOperation declarator,
            IOperation initializer)
        {
            ValidateTemplateScopedInitializer(declarator, initializer);
            _accessibleTemplateLocals.Add(declarator.Symbol);
            AddNode(new RazorVueLocalDeclarationNode(
                declarator.Symbol,
                initializer,
                CreateOrigins(declarator, RazorVueOriginKind.Template)));
        }

        private void ValidateTemplateScopedInitializer(
            IVariableDeclaratorOperation declarator,
            IOperation initializer)
        {
            foreach (var operation in EnumerateSelfAndDescendants(initializer))
            {
                switch (Unwrap(operation))
                {
                    case null:
                        continue;
                    case ILocalReferenceOperation localReference when !_accessibleTemplateLocals.Contains(localReference.Local):
                        throw CreateStructuralIssue(
                            declarator,
                            $"RazorVue template-scoped local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' cannot capture unsupported local '{localReference.Local.Name}'. Only previously declared template locals and active slot/loop parameters are allowed.");
                    case IParameterReferenceOperation parameterReference when
                        !_builderParameters.Contains(parameterReference.Parameter) &&
                        !_accessibleTemplateParameters.Contains(parameterReference.Parameter) &&
                        !IsAnonymousFunctionParameter(parameterReference.Parameter):
                        throw CreateStructuralIssue(
                            declarator,
                            $"RazorVue template-scoped local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' cannot capture unsupported parameter '{parameterReference.Parameter.Name}'.");
                    case IAnonymousFunctionOperation:
                    case IDelegateCreationOperation:
                    case IAssignmentOperation:
                    case IIncrementOrDecrementOperation:
                        throw CreateStructuralIssue(
                            declarator,
                            $"RazorVue template-scoped local '{declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be an immutable value/cache initializer without nested write or callable template state.");
                }
            }
        }

        private bool IsCurrentComponentRenderHelperCandidate(IMethodSymbol method, IOperation? instance)
        {
            if (!ContainsRenderTreeBuilderParameter(method))
                return false;

            return IsCurrentComponentMethod(method, instance);
        }

        private bool TryGetSupportedRenderHelperSignature(
            IMethodSymbol method,
            out IParameterSymbol builderParameter,
            out ImmutableArray<IParameterSymbol> extraParameters,
            out string failureMessage)
        {
            builderParameter = default!;
            extraParameters = ImmutableArray<IParameterSymbol>.Empty;
            failureMessage = string.Empty;
            var helperDisplayName = method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            if (!method.ReturnsVoid)
            {
                failureMessage =
                    $"BuildRenderTree helper method '{helperDisplayName}' must return void in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            var builderParameters = method.Parameters
                .Where(static parameter => IsRenderTreeBuilderType(parameter.Type))
                .ToArray();

            if (builderParameters.Length != 1)
            {
                failureMessage =
                    $"BuildRenderTree helper method '{helperDisplayName}' must declare exactly one RenderTreeBuilder parameter in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            foreach (var parameter in method.Parameters)
            {
                if (parameter.RefKind != RefKind.None)
                {
                    var modifier = parameter.RefKind switch
                    {
                        RefKind.Ref => "ref",
                        RefKind.Out => "out",
                        RefKind.In => "in",
                        _ => parameter.RefKind.ToString().ToLowerInvariant()
                    };
                    failureMessage =
                        $"BuildRenderTree helper method '{helperDisplayName}' cannot declare '{modifier}' parameter '{parameter.Name}' in component '{_snapshot.Descriptor.FullName}'. Only ordinary by-value parameters are supported.";
                    return false;
                }
            }

            var selectedBuilderParameter = builderParameters[0];
            builderParameter = RazorVueMethodSymbolNormalizer.NormalizeParameter(method, selectedBuilderParameter);
            extraParameters = method.Parameters
                .Where(parameter => !SymbolEqualityComparer.Default.Equals(parameter, selectedBuilderParameter))
                .Select(parameter => RazorVueMethodSymbolNormalizer.NormalizeParameter(method, parameter))
                .ToImmutableArray();
            return true;
        }

        private bool TryGetRenderHelperInvocationBindings(
            IInvocationOperation invocation,
            IParameterSymbol builderParameter,
            out IArgumentOperation builderArgument,
            out ImmutableArray<RenderHelperValueBinding> extraArgumentBindings,
            out string failureMessage)
        {
            builderArgument = default!;
            extraArgumentBindings = ImmutableArray<RenderHelperValueBinding>.Empty;
            failureMessage = string.Empty;

            if (invocation.Arguments.Length != invocation.TargetMethod.Parameters.Length)
            {
                failureMessage =
                    $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must be invoked with arguments matching its declared signature in component '{_snapshot.Descriptor.FullName}'. Omitted optional parameters and argument reshaping are not supported.";
                return false;
            }

            var boundParameters = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
            var extraBindingsBuilder = ImmutableArray.CreateBuilder<RenderHelperValueBinding>(Math.Max(invocation.Arguments.Length - 1, 0));
            IArgumentOperation? matchedBuilderArgument = null;
            foreach (var argument in invocation.Arguments)
            {
                if (argument.Parameter is not { } rawParameter)
                {
                    failureMessage =
                        $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must use direct one-to-one argument binding in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                var parameter = RazorVueMethodSymbolNormalizer.NormalizeParameter(invocation.TargetMethod, rawParameter);
                if (!boundParameters.Add(parameter))
                {
                    failureMessage =
                        $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must use direct one-to-one argument binding in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                if (SymbolEqualityComparer.Default.Equals(parameter, builderParameter))
                {
                    matchedBuilderArgument = argument;
                    continue;
                }

                var initializer = Unwrap(argument.Value);
                if (initializer is null)
                {
                    failureMessage =
                        $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' contains an unsupported argument value for parameter '{parameter.Name}' in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                extraBindingsBuilder.Add(new RenderHelperValueBinding(parameter, initializer));
            }

            if (matchedBuilderArgument is null ||
                boundParameters.Count != invocation.TargetMethod.Parameters.Length)
            {
                failureMessage =
                    $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' must be invoked with arguments matching its declared signature in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            builderArgument = matchedBuilderArgument;
            extraArgumentBindings = extraBindingsBuilder.ToImmutable();
            return true;
        }

        private void ParseRenderHelperBody(IInvocationOperation invocation, IParameterSymbol builderParameter)
        {
            ExecuteRenderHelperBody(
                invocation,
                operations => ExecuteWithBuilderScope(
                    ImmutableHashSet.Create<IParameterSymbol>(SymbolEqualityComparer.Default, builderParameter),
                    () =>
                    {
                        foreach (var operation in operations)
                            ParseOperation(operation);
                    }));
        }

        private RazorVueRenderFragment ParseRenderHelperBodyAsScopedFragment(
            IInvocationOperation invocation,
            IParameterSymbol builderParameter,
            ImmutableArray<IParameterSymbol> extraParameters,
            ImmutableArray<RenderHelperValueBinding> extraArgumentBindings)
        {
            var fragment = ExecuteRenderHelperBody(
                invocation,
                operations =>
                {
                    var accessibleTemplateParameters = new HashSet<IParameterSymbol>(_accessibleTemplateParameters, SymbolEqualityComparer.Default);
                    foreach (var parameter in extraParameters)
                        accessibleTemplateParameters.Add(parameter);

                    try
                    {
                        return new Parser(
                            _snapshot,
                            _compilation,
                            _symbols,
                            ImmutableHashSet.Create<IParameterSymbol>(SymbolEqualityComparer.Default, builderParameter),
                            activeRenderHelperMethods: _activeRenderHelperMethods,
                            activeRenderFragmentMembers: _activeRenderFragmentMembers,
                            activeRenderFragmentFactories: _activeRenderFragmentFactories,
                            localRenderFragmentCarriers: GetLocalRenderFragmentCarrierSnapshot(),
                            memberRenderFragmentCarriers: GetMemberRenderFragmentCarrierSnapshot(),
                            factoryRenderFragmentCarriers: GetFactoryRenderFragmentCarrierSnapshot(),
                            accessibleTemplateLocals: _accessibleTemplateLocals,
                            accessibleTemplateParameters: accessibleTemplateParameters,
                            allowTemplateScopedLocals: true).Parse(operations);
                    }
                    catch (RazorVueCompilationIssueException exception)
                    {
                        var message =
                            $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' declares extra value parameters and therefore must produce a self-contained fragment in component '{_snapshot.Descriptor.FullName}'. Inner helper body failure: {exception.Issue.Message}";
                        var origins = exception.Origin is { } origin
                            ? ImmutableArray.Create(origin)
                            : CreateOrigins(invocation, RazorVueOriginKind.Template);
                        throw CreateStructuralIssue(origins, message);
                    }
                });

            if (fragment.Children.IsDefaultOrEmpty)
                return fragment;

            var wrappedFragment = fragment;
            var invocationOrigins = CreateOrigins(invocation, RazorVueOriginKind.Template);
            for (var index = extraArgumentBindings.Length - 1; index >= 0; index--)
            {
                var binding = extraArgumentBindings[index];
                wrappedFragment = new RazorVueRenderFragment(
                    [new RazorVueTemplateScopeNode(
                        ScopeName: binding.ParameterSymbol.Name,
                        ScopeParameterSymbol: binding.ParameterSymbol,
                        Initializer: binding.Initializer,
                        Children: wrappedFragment,
                        Origins: invocationOrigins)]);
            }

            return wrappedFragment;
        }

        private void ExecuteRenderHelperBody(
            IInvocationOperation invocation,
            Action<ImmutableArray<IOperation>> action)
            => ExecuteRenderHelperBody<object?>(
                invocation,
                operations =>
                {
                    action(operations);
                    return null;
                });

        private T ExecuteRenderHelperBody<T>(
            IInvocationOperation invocation,
            Func<ImmutableArray<IOperation>, T> action)
        {
            var canonicalMethod = RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod);
            if (!_activeRenderHelperMethods.Add(canonicalMethod))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree helper method '{GetBuilderCallDisplayName(invocation)}' is recursive; RazorVue does not support recursive render helpers in component '{_snapshot.Descriptor.FullName}'.");
            }

            try
            {
                var operations = GetRenderHelperOperations(invocation);
                return action(operations);
            }
            finally
            {
                _activeRenderHelperMethods.Remove(canonicalMethod);
            }
        }

        private ImmutableArray<IOperation> GetRenderHelperOperations(IInvocationOperation invocation)
        {
            foreach (var syntaxReference in RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod).DeclaringSyntaxReferences)
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

                if (TryGetOperationStatements(operation, out var statements))
                    return statements;
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

        private static IEnumerable<IOperation> EnumerateSelfAndDescendants(IOperation root)
        {
            yield return root;
            foreach (var descendant in root.Descendants())
                yield return descendant;
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

            if (!propertyReference.Property.GetAttributes().Any(static attribute =>
                    string.Equals(
                        attribute.AttributeClass?.ToDisplayString(),
                        "Microsoft.AspNetCore.Components.ParameterAttribute",
                        StringComparison.Ordinal)))
            {
                return false;
            }

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

        private IOperation? TryGetLocalMarkupStringInitializer(ILocalSymbol local)
            => _localStaticMarkupCarriers.TryGetValue(local, out var initializer)
                ? initializer
                : TryGetLocalStaticMarkupInitializer(local);

        private IOperation? TryGetLocalStaticMarkupInitializer(ILocalSymbol local)
        {
            foreach (var reference in local.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is not VariableDeclaratorSyntax declarator ||
                    declarator.Initializer?.Value is null)
                {
                    continue;
                }

                var semanticModel = _compilation.GetSemanticModel(declarator.SyntaxTree);
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

        private IOperation? TryGetPropertyMarkupStringInitializer(IPropertySymbol property)
        {
            foreach (var reference in property.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                    continue;

                var semanticModel = _compilation.GetSemanticModel(declaration.SyntaxTree);
                if (RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(semanticModel, declaration, out var propertyOperation))
                    return propertyOperation;
            }

            return null;
        }

        private IOperation? TryGetFieldMarkupStringInitializer(IFieldSymbol field)
        {
            foreach (var reference in field.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is not VariableDeclaratorSyntax declarator ||
                    declarator.Initializer?.Value is null)
                {
                    continue;
                }

                var semanticModel = _compilation.GetSemanticModel(declarator.SyntaxTree);
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

        private IOperation CreateLiteralStringOperation(string value)
        {
            if (_literalStringOperationCache.TryGetValue(value, out var cached))
                return cached;

            var parseOptions = _compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
                               ?? CSharpParseOptions.Default;
            var source = "file static class __RazorVueLiteralHolder { internal static object Value => "
                         + SymbolDisplay.FormatLiteral(value, quote: true)
                         + "; }";
            var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
            var compilation = CSharpCompilation.Create(
                "__RazorVueLiteralHolder",
                [syntaxTree],
                _compilation.References,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var literal = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .Single();
            var operation = compilation.GetSemanticModel(syntaxTree).GetOperation(literal)
                            ?? throw new InvalidOperationException("Could not materialize a Roslyn literal operation for static BuildRenderTree markup.");

            _literalStringOperationCache[value] = operation;
            return operation;
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
            if (currentNode is not ComponentBuilder componentBuilder)
                return false;

            if (!TryParseSlotTemplate(value, out var slotTemplate))
            {
                if (IsDeclaredComponentSlot(componentBuilder.ComponentType, name) &&
                    value is not null &&
                    IsRenderFragmentLikeValue(value))
                {
                    throw CreateUnsupportedBuilderCall(
                        invocation,
                        $"BuildRenderTree call '{GetBuilderCallDisplayName(invocation)}' passes child content parameter '{name}' on component '{componentBuilder.ComponentFullName}' using a RenderFragment shape that RazorVue cannot canonicalize in component '{_snapshot.Descriptor.FullName}'.");
                }

                return false;
            }

            if (string.Equals(name, "ChildContent", StringComparison.Ordinal) &&
                string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
            {
                var childContent = MaterializeCapturedTemplateChildren(
                    slotTemplate,
                    CreateOrigins(invocation, RazorVueOriginKind.Template));
                currentNode.AddImplicitDefaultSlotAssignment(new RazorVueImplicitDefaultSlotAssignmentNode(
                    childContent,
                    CreateOrigins(invocation, RazorVueOriginKind.Template)));
                foreach (var child in childContent.Children)
                    currentNode.AddChild(child);
                return true;
            }

            var slotOrigins = CreateOrigins(invocation, RazorVueOriginKind.Template);
            if (TryCreateCurrentComponentForwardedSlotAttribute(componentBuilder.ComponentType, name, value, slotOrigins, out var forwardedSlotAttribute))
            {
                currentNode.AddAttribute(forwardedSlotAttribute);
                return true;
            }

            currentNode.AddSlotTemplate(new RazorVueComponentSlotTemplateNode(
                PublicName: name,
                SlotName: string.Equals(name, "ChildContent", StringComparison.Ordinal)
                    ? "default"
                    : ToLowerCamelCase(name),
                ParameterName: slotTemplate.ParameterName,
                ParameterSymbol: slotTemplate.ParameterSymbol,
                Children: MaterializeCapturedTemplateChildren(
                    slotTemplate,
                    slotOrigins),
                Origins: slotOrigins));
            return true;
        }

        private static bool IsRenderFragmentLikeValue(IOperation operation)
            => RazorVueRenderFragmentTypeHelper.IsRenderFragmentType(Unwrap(operation)?.Type);

        private bool TryCreateCurrentComponentForwardedSlotAttribute(
            INamedTypeSymbol componentType,
            string parameterName,
            IOperation? value,
            ImmutableArray<RazorVueSourceOrigin> origins,
            out RazorVueAttributeNode attribute)
        {
            attribute = default!;
            var current = Unwrap(value);
            if (current is not IPropertyReferenceOperation propertyReference ||
                !TryResolveSlotOutlet(propertyReference, out _) ||
                !TryGetDeclaredComponentSlotProperty(componentType, parameterName, out var slotProperty) ||
                !IsParameterizedRenderFragmentType(slotProperty.Type))
            {
                return false;
            }

            attribute = new RazorVueAttributeNode(parameterName, propertyReference, origins);
            return true;
        }

        private bool TryParseChildContent(IOperation? operation, out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (!TryParseSlotTemplate(operation, out var slotTemplate))
                return false;

            if (!string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
                return false;

            fragment = MaterializeCapturedTemplateChildren(
                slotTemplate,
                operation is null
                    ? ImmutableArray<RazorVueSourceOrigin>.Empty
                    : CreateOrigins(operation, RazorVueOriginKind.Template));
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

        private bool TryParseAddContentFragmentFactory(
            IInvocationOperation addContentInvocation,
            IOperation value,
            out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (Unwrap(value) is not IInvocationOperation factoryInvocation)
                return false;

            if (!IsCurrentComponentMethod(factoryInvocation.TargetMethod, factoryInvocation.Instance) ||
                !IsRenderFragmentType(factoryInvocation.TargetMethod.ReturnType))
            {
                return false;
            }

            if (!TryGetSupportedRenderFragmentFactorySignature(
                    factoryInvocation.TargetMethod,
                    out _,
                    out var failureMessage))
            {
                throw CreateUnsupportedBuilderCall(factoryInvocation, failureMessage);
            }

            if (!TryGetRenderFragmentFactoryInvocationBindings(
                    factoryInvocation,
                    out var extraArgumentBindings,
                    out failureMessage))
            {
                throw CreateUnsupportedBuilderCall(factoryInvocation, failureMessage);
            }

            if (!TryResolveFactoryCarrier(factoryInvocation, requireZeroArguments: false, out var slotTemplate))
                return false;

            return TryCreateBoundAddContentFragment(
                addContentInvocation,
                factoryInvocation,
                slotTemplate,
                extraArgumentBindings,
                out fragment);
        }

        private bool TryParseSlotTemplateFragmentFactory(
            IInvocationOperation slotInvocation,
            IOperation value,
            out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (Unwrap(value) is not IInvocationOperation factoryInvocation)
                return false;

            if (!IsCurrentComponentMethod(factoryInvocation.TargetMethod, factoryInvocation.Instance) ||
                !IsRenderFragmentType(factoryInvocation.TargetMethod.ReturnType))
            {
                return false;
            }

            if (!TryGetSupportedRenderFragmentFactorySignature(
                    factoryInvocation.TargetMethod,
                    out _,
                    out var failureMessage))
            {
                throw CreateUnsupportedBuilderCall(factoryInvocation, failureMessage);
            }

            if (!TryGetRenderFragmentFactoryInvocationBindings(
                    factoryInvocation,
                    out var extraArgumentBindings,
                    out failureMessage))
            {
                throw CreateUnsupportedBuilderCall(factoryInvocation, failureMessage);
            }

            if (!TryResolveFactoryCarrier(factoryInvocation, requireZeroArguments: false, out var parsedFactoryTemplate))
                return false;

            if (extraArgumentBindings.IsDefaultOrEmpty)
            {
                slotTemplate = parsedFactoryTemplate;
                return true;
            }

            slotTemplate = parsedFactoryTemplate.PrependCapturedBindings(extraArgumentBindings);
            return true;
        }

        private bool TryCreateBoundAddContentFragment(
            IInvocationOperation addContentInvocation,
            IInvocationOperation factoryInvocation,
            ParsedSlotTemplate slotTemplate,
            ImmutableArray<RenderHelperValueBinding> extraArgumentBindings,
            out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            var invocationOrigins = CreateOrigins(factoryInvocation, RazorVueOriginKind.Template);
            if (IsTypedRenderFragmentAddContent(addContentInvocation))
            {
                if (addContentInvocation.Arguments.Length != 3 ||
                    string.IsNullOrWhiteSpace(slotTemplate.ParameterName) ||
                    slotTemplate.ParameterSymbol is null)
                {
                    return false;
                }

                var initializer = GetInvocationArgument(addContentInvocation, 2);
                if (initializer is null || IsConstantNull(initializer))
                    return false;

                fragment = CreateTypedFragmentScope(
                    addContentInvocation,
                    slotTemplate,
                    initializer);
                fragment = WrapCapturedTemplateScopes(fragment, extraArgumentBindings, invocationOrigins);
                return true;
            }

            if (!IsRenderFragmentAddContent(addContentInvocation) ||
                addContentInvocation.Arguments.Length != 2 ||
                !string.IsNullOrWhiteSpace(slotTemplate.ParameterName))
            {
                return false;
            }

            fragment = MaterializeCapturedTemplateChildren(slotTemplate, invocationOrigins);
            fragment = WrapCapturedTemplateScopes(fragment, extraArgumentBindings, invocationOrigins);
            return true;
        }

        private static RazorVueRenderFragment MaterializeCapturedTemplateChildren(
            ParsedSlotTemplate slotTemplate,
            ImmutableArray<RazorVueSourceOrigin> origins)
            => WrapCapturedTemplateScopes(slotTemplate.Children, slotTemplate.CapturedBindings, origins);

        private RazorVueRenderFragment CreateTypedFragmentScope(
            IInvocationOperation invocation,
            ParsedSlotTemplate slotTemplate,
            IOperation initializer)
        {
            var fragment = new RazorVueRenderFragment(
            [
                new RazorVueTemplateScopeNode(
                    ScopeName: slotTemplate.ParameterName!,
                    ScopeParameterSymbol: slotTemplate.ParameterSymbol,
                    Initializer: initializer,
                    Children: slotTemplate.Children,
                    Origins: CreateOrigins(invocation, RazorVueOriginKind.Template))
            ]);

            return WrapCapturedTemplateScopes(
                fragment,
                slotTemplate.CapturedBindings,
                CreateOrigins(invocation, RazorVueOriginKind.Template));
        }

        private static RazorVueRenderFragment WrapCapturedTemplateScopes(
            RazorVueRenderFragment fragment,
            ImmutableArray<RenderHelperValueBinding> extraArgumentBindings,
            ImmutableArray<RazorVueSourceOrigin> origins)
        {
            var wrappedFragment = fragment;
            for (var index = extraArgumentBindings.Length - 1; index >= 0; index--)
            {
                var binding = extraArgumentBindings[index];
                wrappedFragment = new RazorVueRenderFragment(
                [
                    new RazorVueTemplateScopeNode(
                        ScopeName: binding.ParameterSymbol.Name,
                        ScopeParameterSymbol: binding.ParameterSymbol,
                        Initializer: binding.Initializer,
                        Children: wrappedFragment,
                        Origins: origins)
                ]);
            }

            return wrappedFragment;
        }

        private bool TryParseTypedAddContentTemplate(
            IInvocationOperation invocation,
            IOperation value,
            out RazorVueRenderFragment fragment)
        {
            fragment = RazorVueRenderFragment.Empty;
            if (!IsTypedRenderFragmentAddContent(invocation))
                return false;

            if (invocation.Arguments.Length != 3)
                return false;

            if (!TryParseSlotTemplate(value, out var slotTemplate))
                return false;

            if (string.IsNullOrWhiteSpace(slotTemplate.ParameterName) ||
                slotTemplate.ParameterSymbol is null)
            {
                return false;
            }

            var initializer = GetInvocationArgument(invocation, 2);
            if (initializer is null || IsConstantNull(initializer))
                return false;

            fragment = CreateTypedFragmentScope(invocation, slotTemplate, initializer);
            return true;
        }

        private bool TryParseSlotTemplate(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (TryParseSlotTemplateFragmentFactoryOperation(operation, out slotTemplate))
                return true;

            if (TryParseCurrentComponentSlotSource(operation, out slotTemplate))
                return true;

            if (TryResolveStoredSlotTemplate(operation, out slotTemplate))
                return true;

            if (TryResolveCurrentComponentMemberSlotTemplate(operation, out slotTemplate))
                return true;

            if (TryResolveCurrentComponentFragmentFactory(operation, out slotTemplate))
                return true;

            if (!TryGetAnonymousFunction(operation, out var anonymousFunction))
                return false;

            if (TryParseUntypedSlotTemplate(anonymousFunction, out slotTemplate))
                return true;

            if (TryParseTypedSlotTemplate(anonymousFunction, out slotTemplate))
                return true;

            return false;
        }

        private bool TryParseCurrentComponentSlotSource(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            var current = Unwrap(operation);
            if (current is null || !TryResolveSlotOutlet(current, out var slotName))
                return false;

            slotTemplate = ParsedSlotTemplate.Create(
                parameterName: null,
                parameterSymbol: null,
                children: new RazorVueRenderFragment(
                [
                    new RazorVueSlotOutletNode(
                        slotName,
                        null,
                        CreateOrigins(current, RazorVueOriginKind.Template))
                ]));
            return true;
        }

        private bool TryParseSlotTemplateFragmentFactoryOperation(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            return Unwrap(operation) is IInvocationOperation invocation &&
                   TryParseSlotTemplateFragmentFactory(invocation, invocation, out slotTemplate);
        }

        private bool TryResolveStoredSlotTemplate(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (Unwrap(operation) is not ILocalReferenceOperation localReference)
                return false;

            return _localRenderFragmentCarriers.TryGetValue(localReference.Local, out slotTemplate);
        }

        private bool TryResolveCurrentComponentMemberSlotTemplate(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            var current = Unwrap(operation);
            switch (current)
            {
                case IPropertyReferenceOperation propertyReference
                    when IsCurrentComponentMember(propertyReference.Property, propertyReference.Instance):
                    return TryResolveMemberCarrier(propertyReference.Property, propertyReference, out slotTemplate);
                case IFieldReferenceOperation fieldReference
                    when IsCurrentComponentMember(fieldReference.Field, fieldReference.Instance):
                    return TryResolveMemberCarrier(fieldReference.Field, fieldReference, out slotTemplate);
                default:
                    return false;
            }
        }

        private bool TryResolveCurrentComponentFragmentFactory(IOperation? operation, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (Unwrap(operation) is not IInvocationOperation invocation)
                return false;

            if (!IsCurrentComponentMethod(invocation.TargetMethod, invocation.Instance))
                return false;

            return TryResolveFactoryCarrier(invocation, requireZeroArguments: true, out slotTemplate);
        }

        private bool TryResolveMemberCarrier(
            ISymbol member,
            IOperation referenceOperation,
            out ParsedSlotTemplate slotTemplate)
        {
            if (_memberRenderFragmentCarriers.TryGetValue(member, out slotTemplate))
                return true;

            if (!_activeRenderFragmentMembers.Add(member))
            {
                throw CreateStructuralIssue(
                    referenceOperation,
                    $"BuildRenderTree uses current-component RenderFragment member '{member.Name}' recursively; RazorVue does not support cyclic current-component RenderFragment member carriers in component '{_snapshot.Descriptor.FullName}'.");
            }

            try
            {
                if (!TryCreateMemberCarrier(member, out slotTemplate))
                    return false;

                _memberRenderFragmentCarriers[member] = slotTemplate;
                return true;
            }
            finally
            {
                _activeRenderFragmentMembers.Remove(member);
            }
        }

        private bool TryResolveFactoryCarrier(
            IInvocationOperation invocation,
            bool requireZeroArguments,
            out ParsedSlotTemplate slotTemplate)
        {
            if (requireZeroArguments &&
                (invocation.TargetMethod.Parameters.Length != 0 || invocation.Arguments.Length != 0))
            {
                slotTemplate = default;
                return false;
            }

            var method = RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod);
            if (_factoryRenderFragmentCarriers.TryGetValue(method, out slotTemplate))
                return true;

            if (!_activeRenderFragmentFactories.Add(method))
            {
                throw CreateStructuralIssue(
                    invocation,
                    $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' is recursive; RazorVue does not support recursive RenderFragment factory methods in component '{_snapshot.Descriptor.FullName}'.");
            }

            try
            {
                if (!TryCreateFactoryCarrier(invocation, out slotTemplate))
                    return false;

                _factoryRenderFragmentCarriers[method] = slotTemplate;
                return true;
            }
            finally
            {
                _activeRenderFragmentFactories.Remove(method);
            }
        }

        private bool TryCreateMemberCarrier(ISymbol member, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (!IsRenderFragmentType(member switch
                {
                    IPropertySymbol property => property.Type,
                    IFieldSymbol field => field.Type,
                    _ => null
                }))
            {
                return false;
            }

            if (!IsSupportedCurrentComponentRenderFragmentCarrierMember(member))
                return false;

            IOperation? initializer = member switch
            {
                IPropertySymbol property => TryGetPropertyRenderFragmentInitializer(property),
                IFieldSymbol field => TryGetFieldRenderFragmentInitializer(field),
                _ => null
            };

            if (initializer is null)
                return false;

            return TryGetParsedSlotTemplateFromCarrierInitializer(initializer, out slotTemplate);
        }

        private bool IsSupportedCurrentComponentRenderFragmentCarrierMember(ISymbol member)
        {
            switch (member)
            {
                case IPropertySymbol propertySymbol:
                    if (propertySymbol.SetMethod is null)
                        return true;

                    if (!RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(propertySymbol))
                        return false;

                    return !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(_compilation, propertySymbol);
                case IFieldSymbol fieldSymbol:
                    if (fieldSymbol.IsReadOnly)
                        return true;

                    if (!RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(fieldSymbol))
                        return false;

                    return !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(_compilation, fieldSymbol);
                default:
                    return false;
            }
        }

        private bool TryCreateFactoryCarrier(
            IInvocationOperation invocation,
            out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            var method = invocation.TargetMethod;
            if (!TryGetSupportedRenderFragmentFactorySignature(
                    method,
                    out _,
                    out var failureMessage))
            {
                if (!IsRenderFragmentType(method.ReturnType))
                    return false;

                throw CreateUnsupportedBuilderCall(invocation, failureMessage);
            }

            if (!TryGetRenderFragmentFactoryReturnedValue(invocation, out var returnedValue))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' must be source-authored with an analyzable return value in component '{_snapshot.Descriptor.FullName}'.");
            }

            if (!TryGetParsedSlotTemplateFromCarrierInitializer(returnedValue, out slotTemplate))
            {
                throw CreateUnsupportedBuilderCall(
                    invocation,
                    $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' must return an analyzable RenderFragment template shape in component '{_snapshot.Descriptor.FullName}'.");
            }

            return true;
        }

        private bool TryGetSupportedRenderFragmentFactorySignature(
            IMethodSymbol method,
            out ImmutableArray<IParameterSymbol> extraParameters,
            out string failureMessage)
        {
            extraParameters = ImmutableArray<IParameterSymbol>.Empty;
            failureMessage = string.Empty;
            if (!IsRenderFragmentType(method.ReturnType))
                return false;

            var helperDisplayName = method.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            if (ContainsRenderTreeBuilderParameter(method))
            {
                failureMessage =
                    $"BuildRenderTree fragment factory method '{helperDisplayName}' must not declare RenderTreeBuilder parameters in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            foreach (var parameter in method.Parameters)
            {
                if (parameter.RefKind != RefKind.None)
                {
                    var modifier = parameter.RefKind switch
                    {
                        RefKind.Ref => "ref",
                        RefKind.Out => "out",
                        RefKind.In => "in",
                        _ => parameter.RefKind.ToString().ToLowerInvariant()
                    };
                    failureMessage =
                        $"BuildRenderTree fragment factory method '{helperDisplayName}' cannot declare '{modifier}' parameter '{parameter.Name}' in component '{_snapshot.Descriptor.FullName}'. Only ordinary by-value parameters are supported.";
                    return false;
                }
            }

            extraParameters = method.Parameters
                .Select(parameter => RazorVueMethodSymbolNormalizer.NormalizeParameter(method, parameter))
                .ToImmutableArray();
            return true;
        }

        private bool TryGetRenderFragmentFactoryInvocationBindings(
            IInvocationOperation invocation,
            out ImmutableArray<RenderHelperValueBinding> extraArgumentBindings,
            out string failureMessage)
        {
            extraArgumentBindings = ImmutableArray<RenderHelperValueBinding>.Empty;
            failureMessage = string.Empty;

            if (invocation.Arguments.Length != invocation.TargetMethod.Parameters.Length)
            {
                failureMessage =
                    $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' must be invoked with arguments matching its declared signature in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            var boundParameters = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
            var extraBindingsBuilder = ImmutableArray.CreateBuilder<RenderHelperValueBinding>(invocation.Arguments.Length);
            foreach (var argument in invocation.Arguments)
            {
                if (argument.Parameter is not { } rawParameter)
                {
                    failureMessage =
                        $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' must use direct one-to-one argument binding in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                var parameter = RazorVueMethodSymbolNormalizer.NormalizeParameter(invocation.TargetMethod, rawParameter);
                if (!boundParameters.Add(parameter))
                {
                    failureMessage =
                        $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' must use direct one-to-one argument binding in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                var initializer = Unwrap(argument.Value);
                if (initializer is null)
                {
                    failureMessage =
                        $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' contains an unsupported argument value for parameter '{parameter.Name}' in component '{_snapshot.Descriptor.FullName}'.";
                    return false;
                }

                extraBindingsBuilder.Add(new RenderHelperValueBinding(parameter, initializer));
            }

            if (boundParameters.Count != invocation.TargetMethod.Parameters.Length)
            {
                failureMessage =
                    $"BuildRenderTree fragment factory method '{GetBuilderCallDisplayName(invocation)}' must be invoked with arguments matching its declared signature in component '{_snapshot.Descriptor.FullName}'.";
                return false;
            }

            extraArgumentBindings = extraBindingsBuilder.ToImmutable();
            return true;
        }

        private bool TryGetRenderFragmentFactoryReturnedValue(
            IInvocationOperation invocation,
            out IOperation returnedValue)
        {
            returnedValue = default!;
            foreach (var syntaxReference in RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod).DeclaringSyntaxReferences)
            {
                var syntax = syntaxReference.GetSyntax();
                var semanticModel = _compilation.GetSemanticModel(syntax.SyntaxTree);
                switch (syntax)
                {
                    case MethodDeclarationSyntax methodDeclaration:
                        if (methodDeclaration.ExpressionBody?.Expression is { } methodExpressionBody &&
                            RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(semanticModel, methodExpressionBody, out var methodExpressionBodyOperation) &&
                            methodExpressionBodyOperation is not null)
                        {
                            returnedValue = methodExpressionBodyOperation;
                            return true;
                        }

                        if (methodDeclaration.Body is not null &&
                            semanticModel.GetOperation(methodDeclaration.Body) is IBlockOperation methodBlock &&
                            TryGetSingleReturnedValue(methodBlock, out var methodReturnValue) &&
                            methodReturnValue is not null)
                        {
                            returnedValue = methodReturnValue;
                            return true;
                        }

                        break;
                    case LocalFunctionStatementSyntax localFunction:
                        if (localFunction.ExpressionBody?.Expression is { } localExpressionBody &&
                            RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(semanticModel, localExpressionBody, out var localExpressionBodyOperation) &&
                            localExpressionBodyOperation is not null)
                        {
                            returnedValue = localExpressionBodyOperation;
                            return true;
                        }

                        if (localFunction.Body is not null &&
                            semanticModel.GetOperation(localFunction.Body) is IBlockOperation localBlock &&
                            TryGetSingleReturnedValue(localBlock, out var localReturnValue) &&
                            localReturnValue is not null)
                        {
                            returnedValue = localReturnValue;
                            return true;
                        }

                        break;
                }
            }

            return false;
        }

        private IOperation? TryGetPropertyRenderFragmentInitializer(IPropertySymbol property)
        {
            foreach (var reference in property.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                    continue;

                var semanticModel = _compilation.GetSemanticModel(declaration.SyntaxTree);
                if (RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(semanticModel, declaration, out var propertyOperation))
                    return propertyOperation;
            }

            return null;
        }

        private IOperation? TryGetFieldRenderFragmentInitializer(IFieldSymbol field)
        {
            foreach (var reference in field.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is not VariableDeclaratorSyntax declarator ||
                    declarator.Initializer?.Value is null)
                {
                    continue;
                }

                var semanticModel = _compilation.GetSemanticModel(declarator.SyntaxTree);
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

        private bool TryGetParsedSlotTemplateFromCarrierInitializer(IOperation initializer, out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            return TryParseSlotTemplate(initializer, out slotTemplate);
        }

        private bool TryParseUntypedSlotTemplate(
            IAnonymousFunctionOperation anonymousFunction,
            out ParsedSlotTemplate slotTemplate)
        {
            slotTemplate = default;
            if (!TryGetSingleBuilderParameter(anonymousFunction, out _))
                return false;

            slotTemplate = ParsedSlotTemplate.Create(
                parameterName: null,
                parameterSymbol: null,
                children: ParseAnonymousFunctionBody(anonymousFunction));
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

            slotTemplate = ParsedSlotTemplate.Create(
                parameterName: slotContextParameter.Name,
                parameterSymbol: slotContextParameter,
                children: ParseAnonymousFunctionBody(builderAnonymousFunction));
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
                return new Parser(
                    _snapshot,
                    _compilation,
                    _symbols,
                    builderParameters,
                    activeRenderHelperMethods: _activeRenderHelperMethods,
                    activeRenderFragmentMembers: _activeRenderFragmentMembers,
                    activeRenderFragmentFactories: _activeRenderFragmentFactories,
                    localRenderFragmentCarriers: GetLocalRenderFragmentCarrierSnapshot(),
                    memberRenderFragmentCarriers: GetMemberRenderFragmentCarrierSnapshot(),
                    factoryRenderFragmentCarriers: GetFactoryRenderFragmentCarrierSnapshot(),
                    accessibleTemplateLocals: _accessibleTemplateLocals,
                    accessibleTemplateParameters: _accessibleTemplateParameters,
                    allowTemplateScopedLocals: true).Parse(block.Operations);

            return new Parser(
                _snapshot,
                _compilation,
                _symbols,
                builderParameters,
                activeRenderHelperMethods: _activeRenderHelperMethods,
                activeRenderFragmentMembers: _activeRenderFragmentMembers,
                activeRenderFragmentFactories: _activeRenderFragmentFactories,
                localRenderFragmentCarriers: GetLocalRenderFragmentCarrierSnapshot(),
                memberRenderFragmentCarriers: GetMemberRenderFragmentCarrierSnapshot(),
                factoryRenderFragmentCarriers: GetFactoryRenderFragmentCarrierSnapshot(),
                accessibleTemplateLocals: _accessibleTemplateLocals,
                accessibleTemplateParameters: _accessibleTemplateParameters,
                allowTemplateScopedLocals: true).Parse([body]);
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

        private static bool TryGetOperationStatements(
            IOperation? operation,
            out ImmutableArray<IOperation> statements)
        {
            statements = ImmutableArray<IOperation>.Empty;
            var current = Unwrap(operation);
            if (current is null)
                return false;

            if (current is IBlockOperation block)
            {
                statements = block.Operations;
                return true;
            }

            if (current is IInvocationOperation invocation)
            {
                statements = [invocation];
                return true;
            }

            return false;
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

        private static bool IsAnonymousFunctionParameter(IParameterSymbol parameter)
            => parameter.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.LambdaMethod or MethodKind.AnonymousFunction };

        private ImmutableArray<RenderFragmentLocalCarrier> GetLocalRenderFragmentCarrierSnapshot()
            => [.. _localRenderFragmentCarriers.Select(static pair => new RenderFragmentLocalCarrier(pair.Key, pair.Value))];

        private ImmutableArray<RenderFragmentMemberCarrier> GetMemberRenderFragmentCarrierSnapshot()
            => [.. _memberRenderFragmentCarriers.Select(static pair => new RenderFragmentMemberCarrier(pair.Key, pair.Value))];

        private ImmutableArray<RenderFragmentFactoryCarrier> GetFactoryRenderFragmentCarrierSnapshot()
            => [.. _factoryRenderFragmentCarriers.Select(static pair => new RenderFragmentFactoryCarrier(pair.Key, pair.Value))];

        private static Dictionary<ILocalSymbol, ParsedSlotTemplate> CreateLocalRenderFragmentCarrierDictionary(
            IEnumerable<RenderFragmentLocalCarrier> carriers)
        {
            var dictionary = new Dictionary<ILocalSymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default);
            foreach (var carrier in carriers)
                dictionary[carrier.LocalSymbol] = carrier.Template;

            return dictionary;
        }

        private static Dictionary<ISymbol, ParsedSlotTemplate> CreateMemberRenderFragmentCarrierDictionary(
            IEnumerable<RenderFragmentMemberCarrier> carriers)
        {
            var dictionary = new Dictionary<ISymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default);
            foreach (var carrier in carriers)
                dictionary[carrier.MemberSymbol] = carrier.Template;

            return dictionary;
        }

        private static Dictionary<IMethodSymbol, ParsedSlotTemplate> CreateFactoryRenderFragmentCarrierDictionary(
            IEnumerable<RenderFragmentFactoryCarrier> carriers)
        {
            var dictionary = new Dictionary<IMethodSymbol, ParsedSlotTemplate>(SymbolEqualityComparer.Default);
            foreach (var carrier in carriers)
                dictionary[carrier.MethodSymbol] = carrier.Template;

            return dictionary;
        }

        private bool IsDeclaredComponentSlot(INamedTypeSymbol componentType, string parameterName)
            => TryGetDeclaredComponentSlotProperty(componentType, parameterName, out _);

        private bool TryGetDeclaredComponentSlotProperty(
            INamedTypeSymbol componentType,
            string parameterName,
            out IPropertySymbol property)
        {
            property = default!;
            if (_symbols.ParameterAttribute is null)
                return false;

            for (var current = componentType; current is not null; current = current.BaseType)
            {
                foreach (var member in current.GetMembers(parameterName))
                {
                    if (member is not IPropertySymbol candidateProperty ||
                        candidateProperty.IsStatic ||
                        !IsRenderFragmentType(candidateProperty.Type))
                    {
                        continue;
                    }

                    if (candidateProperty.GetAttributes().Any(attribute =>
                            SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, _symbols.ParameterAttribute)))
                    {
                        property = candidateProperty;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsRenderFragmentAddContent(IInvocationOperation invocation)
            => invocation.Arguments.Length >= 2 &&
               IsRenderFragmentType(invocation.TargetMethod.Parameters[1].Type);

        private bool IsTypedRenderFragmentAddContent(IInvocationOperation invocation)
            => invocation.Arguments.Length >= 3 &&
               IsRenderFragmentType(invocation.TargetMethod.Parameters[1].Type);

        private bool IsMarkupStringAddContent(IInvocationOperation invocation)
            => invocation.Arguments.Length >= 2 &&
               RazorVueStaticMarkupValueHelper.IsMarkupStringType(invocation.TargetMethod.Parameters[1].Type);

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

        private bool IsParameterizedRenderFragmentType(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null)
                return false;

            if (typeSymbol is INamedTypeSymbol namedType &&
                namedType.IsGenericType &&
                namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                typeSymbol = namedType.TypeArguments[0];
            }

            return typeSymbol is INamedTypeSymbol renderFragmentType &&
                   _symbols.RenderFragmentOfT is not null &&
                   SymbolEqualityComparer.Default.Equals(renderFragmentType.OriginalDefinition, _symbols.RenderFragmentOfT);
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

        private static string GetOperationDisplay(IOperation operation)
        {
            var display = operation.Syntax?.ToString()?.Trim();
            return string.IsNullOrWhiteSpace(display)
                ? operation.Kind.ToString()
                : display!;
        }

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
                : [RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), originKind)];

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

        private bool HasPendingImmediateAssignmentDeclarations()
            => _pendingRenderFragmentLocalCarriers.Count > 0 || _pendingTemplateScopedDeclarations.Count > 0;

        private bool IsPendingImmediateAssignment(IOperation operation)
        {
            if (Unwrap(operation) is not IExpressionStatementOperation expressionStatement ||
                Unwrap(expressionStatement.Operation) is not ISimpleAssignmentOperation assignment ||
                assignment.Target is not ILocalReferenceOperation localReference)
            {
                return false;
            }

            return _pendingRenderFragmentLocalCarriers.ContainsKey(localReference.Local) ||
                   _pendingTemplateScopedDeclarations.ContainsKey(localReference.Local);
        }

        private void EnsureNoPendingImmediateAssignmentDeclarations()
        {
            if (!HasPendingImmediateAssignmentDeclarations())
                return;

            ThrowPendingImmediateAssignmentRequiresImmediateAssignment(null);
        }

        private void ThrowPendingImmediateAssignmentRequiresImmediateAssignment(IOperation? currentOperation)
        {
            string message;
            IOperation originOperation;
            if (_pendingRenderFragmentLocalCarriers.Count > 0)
            {
                var pendingDeclaration = _pendingRenderFragmentLocalCarriers.Values.First();
                originOperation = pendingDeclaration.Declarator;
                message =
                    $"RazorVue RenderFragment local '{pendingDeclaration.Declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be assigned exactly once by the immediately following simple assignment statement.";
            }
            else
            {
                var pendingDeclaration = _pendingTemplateScopedDeclarations.Values.First();
                originOperation = pendingDeclaration.Declarator;
                message =
                    $"RazorVue template-scoped local '{pendingDeclaration.Declarator.Symbol.Name}' in component '{_snapshot.Descriptor.FullName}' must be assigned exactly once by the immediately following simple assignment statement.";
            }

            throw CreateStructuralIssue(
                currentOperation ?? originOperation,
                message);
        }

        private readonly record struct PendingRenderFragmentLocalCarrierDeclaration(
            IVariableDeclaratorOperation Declarator);

        private readonly record struct PendingTemplateScopedDeclaration(
            IVariableDeclaratorOperation Declarator);

    }

    private abstract class OpenFrame(ImmutableArray<RazorVueSourceOrigin> origins)
	{
		public ImmutableArray<RazorVueSourceOrigin> Origins { get; } = origins;

		public abstract string Describe();
    }

    private abstract class OpenNodeBuilder : OpenFrame
    {
        private RazorVueNodeKey? _key;
        private readonly List<RazorVueAttributeEntry> _attributes = [];
        private readonly List<RazorVueComponentSlotTemplateNode> _slotTemplates = [];
        private readonly List<RazorVueImplicitDefaultSlotAssignmentNode> _implicitDefaultSlotAssignments = [];
        private readonly List<RazorVueRenderNode> _ambientDefaultSlotChildren = [];
        private readonly List<RazorVueRenderNode> _children = [];

        protected OpenNodeBuilder(ImmutableArray<RazorVueSourceOrigin> origins)
            : base(origins)
        {
        }

        public void AddAttribute(RazorVueAttributeEntry attribute)
            => _attributes.Add(attribute);

        public void SetKey(IOperation? key, ImmutableArray<RazorVueSourceOrigin> origins)
            => _key = key is null ? null : new RazorVueNodeKey(key, origins);

        public void AddSlotTemplate(RazorVueComponentSlotTemplateNode slotTemplate)
            => _slotTemplates.Add(slotTemplate);

        public void AddImplicitDefaultSlotAssignment(RazorVueImplicitDefaultSlotAssignmentNode assignment)
            => _implicitDefaultSlotAssignments.Add(assignment);

        public void AddAmbientDefaultSlotChild(RazorVueRenderNode child)
            => _ambientDefaultSlotChildren.Add(child);

        public void AddChild(RazorVueRenderNode child)
            => _children.Add(child);

        protected ImmutableArray<RazorVueAttributeEntry> BuildAttributes()
            => [.. _attributes];

        protected RazorVueNodeKey? BuildKey()
            => _key;

        protected ImmutableArray<RazorVueComponentSlotTemplateNode> BuildSlotTemplates()
            => [.. _slotTemplates];

        protected ImmutableArray<RazorVueImplicitDefaultSlotAssignmentNode> BuildImplicitDefaultSlotAssignments()
            => [.. _implicitDefaultSlotAssignments];

        protected RazorVueRenderFragment BuildAmbientDefaultSlotChildren()
            => new([.. _ambientDefaultSlotChildren]);

        protected RazorVueRenderFragment BuildChildren()
            => new([.. _children]);

        public abstract override string Describe();

        public abstract RazorVueRenderNode Build();
    }

    private sealed class ElementBuilder(string tagName, ImmutableArray<RazorVueSourceOrigin> origins)
        : OpenNodeBuilder(origins)
    {
        public override string Describe()
            => $"element <{tagName}>";

        public override RazorVueRenderNode Build()
            => new RazorVueElementNode(tagName, BuildKey(), BuildAttributes(), BuildChildren(), Origins);
    }

    private sealed class ComponentBuilder(string componentName, string componentFullName, string resolutionName, INamedTypeSymbol componentType, ImmutableArray<RazorVueSourceOrigin> origins)
        : OpenNodeBuilder(origins)
    {
        public string ComponentFullName { get; } = componentFullName;

        public INamedTypeSymbol ComponentType { get; } = componentType;

        public override string Describe()
            => $"component '{ComponentFullName}'";

        public override RazorVueRenderNode Build()
            => new RazorVueComponentNode(componentName, ComponentFullName, resolutionName, BuildKey(), BuildAttributes(), BuildSlotTemplates(), BuildImplicitDefaultSlotAssignments(), BuildAmbientDefaultSlotChildren(), BuildChildren(), Origins);
    }
}
